﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using AutoMapper;
using Library.API.Data;
using Library.API.DTOs;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services.LibService;
using Library.API.Services.PropertyService;
using Library.API.Services.TypeService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;

namespace Library.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(setupAction =>
            {
                var xmlInFormatter = setupAction.InputFormatters
                .OfType<XmlSerializerInputFormatter>().FirstOrDefault();

                if (xmlInFormatter != null)
                {
                    xmlInFormatter.SupportedMediaTypes
                    .Add("application/vnd.netXworks.authorwithdateofdeath.full+xml");
                }

                var jsonInFormatter = setupAction.InputFormatters
                .OfType<JsonInputFormatter>().FirstOrDefault();

                if (jsonInFormatter != null)
                {
                    jsonInFormatter.SupportedMediaTypes
                    .Add("application/vnd.netXworks.author.full+json");
                    jsonInFormatter.SupportedMediaTypes
                    .Add("application/vnd.netXworks.authorwithdateofdeath.full+json");
                }

                var jsonOutFormatter = setupAction.OutputFormatters
                .OfType<JsonOutputFormatter>().FirstOrDefault();

                if (jsonOutFormatter != null)
                {
                    jsonOutFormatter.SupportedMediaTypes.Add("application/vnd.netXworks.hateoas+json");
                }

            }).AddXmlDataContractSerializerFormatters()
            .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new
                    CamelCasePropertyNamesContractResolver();
                });

            services.AddDbContext<LibraryContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ILibraryRepository, LibraryRepository>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<ITypeHelperService, TypeHelperService>();
            services.AddHttpCacheHeaders(
                expirationModelOptions =>
                {
                    expirationModelOptions.MaxAge = 600;
                }, 
                validationModelOptions =>
                {
                    validationModelOptions.AddMustRevalidate = true;
                });

            services.AddResponseCaching();

            services.AddMemoryCache();

            services.Configure<IpRateLimitOptions>(options =>
            {
                options.GeneralRules = new List<RateLimitRule>()
                {
                    new RateLimitRule() { Endpoint = "*", Limit = 1000, Period = "5m" },
                    new RateLimitRule() { Endpoint = "*", Limit = 200, Period = "10s" }
                };
            });

            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (exceptionHandlerFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500,
                                exceptionHandlerFeature.Error, 
                                exceptionHandlerFeature.Error.Message);
                        }
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected error occured while processing the request.");
                    });
                });
            }


            Mapper.Initialize(config =>
            {
                config.CreateMap<Author, AuthorDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    $"{src.FirstName} {src.LastName}"))
                    .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
                    src.DateOfBirth.GetCurrentAge(src.DateOfDeath)));

                config.CreateMap<Book, BookDto>();
                config.CreateMap<AuthorCreateDto, Author>();
                config.CreateMap<AuthorCreateWithDeathDateDto, Author>();
                config.CreateMap<BookCreateDto, Book>();
                config.CreateMap<BookUpdateDto, Book>();
                config.CreateMap<Book, BookUpdateDto>();
            });

            app.UseIpRateLimiting();
            app.UseResponseCaching();
            app.UseHttpCacheHeaders();
            app.UseMvc();
        }
    }
}
