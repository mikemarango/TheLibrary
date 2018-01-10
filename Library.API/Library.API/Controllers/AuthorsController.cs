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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        public ILibraryRepository Repository { get; }
        public IUrlHelper UrlHelper { get; }

        public AuthorsController(ILibraryRepository repository, IUrlHelper urlHelper)
        {
            Repository = repository;
            UrlHelper = urlHelper;
        }

        // GET: api/authors
        [HttpGet(Name = "GetAuthors")]
        public IActionResult Get(AuthorsResourceParameters resourceParameters)
        {
            var authors = Repository.GetAuthors(resourceParameters);

            var previousPageLink = authors.HasPrevious ?
                CreateAuthorResourceUri(resourceParameters, ResourceUriType.PreviousPage) : null;

            var nextPageLink = authors.HasNext ?
                CreateAuthorResourceUri(resourceParameters, ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = authors.TotalCount,
                pageSize = authors.PageSize,
                currentPage = authors.CurrentPage,
                totalPages = authors.TotalPages,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            var authorDto = Mapper.Map<IEnumerable<AuthorDto>>(authors);

            return Ok(authorDto);
        }

        private string CreateAuthorResourceUri(AuthorsResourceParameters resourceParameters, ResourceUriType pageContext)
        {
            switch (pageContext)
            {
                case ResourceUriType.PreviousPage:
                    return UrlHelper.Link("GetAuthors", new
                    {
                        pageNumber = resourceParameters.PageNumber - 1,
                        pageSize = resourceParameters.PageSize
                    });

                case ResourceUriType.NextPage:
                    return UrlHelper.Link("GetAuthors", new
                    {
                        pageNumber = resourceParameters.PageNumber + 1,
                        pageSize = resourceParameters.PageSize
                    });
                default:
                    return UrlHelper.Link("GetAuthors", new 
                    {
                        pageNumber = resourceParameters.PageNumber,
                        pageSize = resourceParameters.PageSize
                    });
            }
        }

        // GET api/authors/76053df4-6687-4353-8937-b45556748abe
        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult Get(Guid id)
        {
            var author = Repository.GetAuthor(id);

            if (author == null)
                return NotFound();

            var authorDto = Mapper.Map<AuthorDto>(author);

            return Ok(authorDto);
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

            return CreatedAtRoute("GetAuthor", new { id = authorDto.Id }, authorDto);
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
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            Author author = Repository.GetAuthor(id);

            if (author == null) return NotFound();

            Repository.DeleteAuthor(author);

            if (!Repository.Save())
                throw new Exception($"Delete for author {id} failed on save.");

            return NoContent();
        }
    }
}
