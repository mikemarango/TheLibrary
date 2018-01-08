using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.DTOs;
using Library.API.Models;
using Library.API.Services.LibService;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        public ILibraryRepository Repository { get; }

        public BooksController(ILibraryRepository repository)
        {
            Repository = repository;
        }

        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(Guid authorId)
        {
            if (!Repository.AuthorExists(authorId))
                return NotFound();

            var books = Repository.GetBooks(authorId);

            var bookDto = Mapper.Map<IEnumerable<BookDto>>(books);

            return Ok(bookDto);
        }

        // GET api/<controller>/5
        [HttpGet("{id}", Name = "GetBook")]
        public IActionResult Get(Guid authorId, Guid id)
        {
            if (!Repository.AuthorExists(authorId))
                return NotFound();

            var book = Repository.GetBook(authorId, id);

            if (book == null)
                return NotFound();

            var bookDto = Mapper.Map<BookDto>(book);

            return Ok(bookDto);
        }

        // POST api/<controller>
        [HttpPost]
        public IActionResult Post(Guid authorId, [FromBody]BookCreateDto createDto)
        {
            if (createDto == null)
                return BadRequest();

            if (!Repository.AuthorExists(authorId))
                return NotFound();

            var book = Mapper.Map<Book>(createDto);

            Repository.CreateBook(authorId, book);

            if (!Repository.Save())
                throw new Exception($"Creating book for author {authorId} was unsuccessful.");

            var bookDto = Mapper.Map<BookDto>(book);

            return CreatedAtRoute("GetBook", new { authorId, id = bookDto.Id }, bookDto );
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
