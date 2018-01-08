using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.DTOs;
using Library.API.Services.LibService;
using Microsoft.AspNetCore.Mvc;
using Library.API.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        public ILibraryRepository Repository { get; }

        public AuthorsController(ILibraryRepository repository)
        {
            Repository = repository;
        }

        // GET: api/authors
        [HttpGet]
        public IActionResult Get()
        {
            var authors = Repository.GetAuthors();
            var authorDto = new List<AuthorDto>();
            foreach (var author in authors)
            {
                authorDto.Add(new AuthorDto()
                {
                    Id = author.Id,
                    Name = $"{author.FirstName}, {author.LastName}",
                    Genre = author.Genre,
                    Age = author.DateOfBirth.GetCurrentAge()
                });
            }
            return new JsonResult(authorDto);
        }

        // GET api/authors/76053df4-6687-4353-8937-b45556748abe
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return null;
        }

        // POST api/<controller>
        [HttpPost]
        public IActionResult Post([FromBody]string value)
        {
            return null;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return null;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return null;
        }
    }
}
