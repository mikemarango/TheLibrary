using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.DTOs;
using Library.API.Services.LibService;
using Microsoft.AspNetCore.Mvc;
using Library.API.Helpers;
using AutoMapper;
using Library.API.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Library.API.Services.PropertyService;
using Library.API.Services.TypeService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        public ILibraryRepository Repository { get; }
        public IUrlHelper UrlHelper { get; }
        public IPropertyMappingService MappingService { get; }
        public ITypeHelperService TypeHelper { get; }

        public AuthorsController(ILibraryRepository repository, 
            IUrlHelper urlHelper, 
            IPropertyMappingService mappingService,
            ITypeHelperService typeHelper)
        {
            Repository = repository;
            UrlHelper = urlHelper;
            MappingService = mappingService;
            TypeHelper = typeHelper;
        }

        // GET: api/authors
        [HttpGet(Name = "GetAuthors")]
        public IActionResult Get(AuthorsResourceParameters resourceParameters)
        {
            if (!MappingService.ValidMappingExistsFor<AuthorDto, Author>
                (resourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!TypeHelper.TypeHasProperties<AuthorDto>(resourceParameters.Fields))
                return BadRequest();

            var authors = Repository.GetAuthors(resourceParameters);

           

            var paginationMetadata = new
            {
                totalCount = authors.TotalCount,
                pageSize = authors.PageSize,
                currentPage = authors.CurrentPage,
                totalPages = authors.TotalPages,
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            var authorCollection = Mapper.Map<IEnumerable<AuthorDto>>(authors);

            var links = CreateAuthorCollectionLinks(resourceParameters, authors.HasNext, authors.HasPrevious);

            var shapedAuthors = authorCollection.ShapeData(resourceParameters.Fields);

            var shapedAuthorLinks = shapedAuthors.Select(author =>
            {
                var authorAsDictionary = author as IDictionary<string, object>;
                var authorLinks = CreateAuthorLinks((Guid)authorAsDictionary["Id"], resourceParameters.Fields);
                authorAsDictionary.Add("links", authorLinks);
                return authorAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorLinks,
                links = links
            };

            return Ok(linkedCollectionResource);

        }

        private IEnumerable<LinkDto> CreateAuthorCollectionLinks(AuthorsResourceParameters authorsResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>
            {
                // self
                new LinkDto(CreateAuthorResourceUri(authorsResourceParameters, ResourceUriType.CurrentPage), "self", "GET")
            };

            if (hasNext)
            {
                links.Add(new LinkDto(CreateAuthorResourceUri(authorsResourceParameters, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateAuthorResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateAuthorResourceUri(AuthorsResourceParameters resourceParameters, ResourceUriType pageContext)
        {
            switch (pageContext)
            {
                case ResourceUriType.PreviousPage:
                    return UrlHelper.Link("GetAuthors", new
                    {
                        fields = resourceParameters.Fields,
                        orderby = resourceParameters.OrderBy,
                        search = resourceParameters.SearchQuery,
                        genre = resourceParameters.Genre,
                        pageNumber = resourceParameters.PageNumber - 1,
                        pageSize = resourceParameters.PageSize
                    });

                case ResourceUriType.NextPage:
                    return UrlHelper.Link("GetAuthors", new
                    {
                        fields = resourceParameters.Fields,
                        orderby = resourceParameters.OrderBy,
                        search = resourceParameters.SearchQuery,
                        genre = resourceParameters.Genre,
                        pageNumber = resourceParameters.PageNumber + 1,
                        pageSize = resourceParameters.PageSize
                    });
                case ResourceUriType.CurrentPage:
                default:
                    return UrlHelper.Link("GetAuthors", new 
                    {
                        fields = resourceParameters.Fields,
                        orderby = resourceParameters.OrderBy,
                        search = resourceParameters.SearchQuery,
                        genre = resourceParameters.Genre,
                        pageNumber = resourceParameters.PageNumber,
                        pageSize = resourceParameters.PageSize
                    });
            }
        }

        // GET api/authors/76053df4-6687-4353-8937-b45556748abe
        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult Get(Guid id, [FromQuery]string fields)
        {
            if (!TypeHelper.TypeHasProperties<AuthorDto>(fields))
                return BadRequest();

            var author = Repository.GetAuthor(id);

            if (author == null)
                return NotFound();

            var authorDto = Mapper.Map<AuthorDto>(author);

            var authorLinks = CreateAuthorLinks(id, fields);

            var authorResourceLinks = authorDto.ShapeData(fields) as IDictionary<string, object>;

            authorResourceLinks.Add("links", authorLinks);

            return Ok(authorResourceLinks);
        }

        // POST api/<controller>
        [HttpPost]
        public IActionResult Post([FromBody]AuthorCreateDto authorCreate)
        {
            if (authorCreate == null)
                return BadRequest();

            var author = Mapper.Map<Author>(authorCreate);

            Repository.CreateAuthor(author);

            if (!Repository.Save())
                throw new Exception("Author creation unsuccessful.");

            var authorDto = Mapper.Map<AuthorDto>(author);

            var links = CreateAuthorLinks(authorDto.Id, null);

            var authorResourceLinks = authorDto.ShapeData(null)
                as IDictionary<string, object>;

            authorResourceLinks.Add("links", links);

            return CreatedAtRoute("GetAuthor", new { id = authorResourceLinks["Id"] }, authorResourceLinks);
        }

        [HttpPost("{id}")]
        public IActionResult Post(Guid id)
        {
            if (Repository.AuthorExists(id))
                return StatusCode(409);

            return NotFound();
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return null;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}", Name = "DeleteAuthor")]
        public IActionResult Delete(Guid id)
        {
            Author author = Repository.GetAuthor(id);

            if (author == null) return NotFound();

            Repository.DeleteAuthor(author);

            if (!Repository.Save())
                throw new Exception($"Delete for author {id} failed on save.");

            return NoContent();
        }

        private IEnumerable<LinkDto> CreateAuthorLinks(Guid id, string fields)
        {
            var links = new List<LinkDto>();
            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(UrlHelper.Link("GetAuthor", new {  id }), "self", "GET"));
            }
            else
            {
                links.Add(new LinkDto(UrlHelper.Link("GetAuthor", new { id,  fields }), "self", "GET"));
            }

            links.Add(new LinkDto(UrlHelper.Link("DeleteAuthor", new { id, fields }), "delete_author", "DELETE"));

            links.Add(new LinkDto(UrlHelper.Link("CreateBook", new { authorId = id }), "create_book", "POST"));

            links.Add(new LinkDto(UrlHelper.Link("GetBooks", new { authorId = id }), "get_books", "GET"));

            return links;
        }
    }
}
