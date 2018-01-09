using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.DTOs;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services.LibService;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    [Route("api/author_list")]
    public class AuthorListController : Controller
    {
        public ILibraryRepository Repository { get; }

        public AuthorListController(ILibraryRepository repository)
        {
            Repository = repository;
        }

        [HttpGet("{id}", Name = "GetAuthorCollection")]
        public IActionResult Get([ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> id)
        {
            if (id == null)
                return BadRequest();

            var authors = Repository.GetAuthors(id);

            if (id.Count() != authors.Count())
                return NotFound();

            var authorDto = Mapper.Map<IEnumerable<AuthorDto>>(authors);

            return Ok(authorDto);
        }

        // POST api/<controller>
        [HttpPost]
        public IActionResult Post([FromBody]IEnumerable<AuthorCreateDto> authorList)
        {
            if (authorList == null)
                return BadRequest();

            var authors = Mapper.Map<IEnumerable<Author>>(authorList);

            foreach (var author in authors)
            {
                Repository.CreateAuthor(author);
            }

            if (!Repository.Save())
                throw new Exception("Unsuccessful creation of authors collection.");

            var authorDto = Mapper.Map<IEnumerable<AuthorDto>>(authors);

            var idString = string.Join(",", authorDto.Select(a => a.Id));

            return CreatedAtRoute("GetAuthorCollection", new { id = idString }, authorDto);
        }
    }
}
