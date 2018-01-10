using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.DTOs;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services.LibService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        public ILibraryRepository Repository { get; }
        public ILogger<BooksController> Logger { get; }

        public BooksController(ILibraryRepository repository, ILogger<BooksController> logger)
        {
            Repository = repository;
            Logger = logger;
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

            if (createDto.Description == createDto.Title)
                ModelState.AddModelError(nameof(createDto), "The title and description must be different.");

            if (!ModelState.IsValid)
                //return StatusCode(422, ModelState.ErrorCount);
                //return new StatusCodeResult(StatusCodes.Status422UnprocessableEntity);
                return new UnprocessableEntityObjectResult(ModelState);

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
        public IActionResult Put(Guid authorId, Guid id, [FromBody]BookUpdateDto bookDto)
        {
            if (bookDto == null) return BadRequest();

            if (bookDto.Description == bookDto.Title)
                ModelState.AddModelError(nameof(BookUpdateDto), "The description must be different from the title.");

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            if (!Repository.AuthorExists(authorId))
                return NotFound();

            Book book = Repository.GetBook(authorId, id);

            if (book == null)
            {
                var bookToAdd = Mapper.Map<Book>(bookDto);
                //var bookToAdd = Mapper.Map(bookDto, book);
                bookToAdd.Id = id;
                Repository.CreateBook(authorId, bookToAdd);

                if (!Repository.Save())
                    throw new Exception($"Upserting book {id} for author {authorId} failed.");

                var bookCreated = Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBook",new { authorId, id = bookCreated.Id }, bookCreated);
            }

            // Mapper.Map<Book>(bookDto);
            Mapper.Map(bookDto, book);

            Repository.UpdateBook(book);

            if (!Repository.Save())
                throw new Exception($"Failed to update book {id} for author {authorId}");

            return NoContent();

        }

        // PATCH = Partially Update
        [HttpPatch("{id}")]
        public IActionResult Patch(Guid authorId, Guid id, [FromBody]JsonPatchDocument<BookUpdateDto> bookPatch)
        {
            if (bookPatch == null)
            {
                return BadRequest();
            }

            if (!Repository.AuthorExists(authorId))
                return NotFound();

            var book = Repository.GetBook(authorId, id);

            if (book == null)
            {
                var bookUpdate = new BookUpdateDto();

                bookPatch.ApplyTo(bookUpdate, ModelState);

                if (bookUpdate.Description == bookUpdate.Title)
                    ModelState.AddModelError(nameof(BookUpdateDto), "The description and title must be different!");

                TryValidateModel(bookUpdate);

                if (!ModelState.IsValid)
                    return new UnprocessableEntityObjectResult(ModelState);

                var bookToAdd = Mapper.Map<Book>(bookUpdate);

                bookToAdd.Id = id;

                Repository.CreateBook(authorId, bookToAdd);

                if (!Repository.Save())
                    throw new Exception($"Upserting book {id} for author {authorId} failed.");

                var bookCreated = Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBook", new { authorId, id = bookCreated.Id }, bookCreated);
            }

            var bookUpdateDto = Mapper.Map<BookUpdateDto>(book);

            bookPatch.ApplyTo(bookUpdateDto, ModelState);

            if (bookUpdateDto.Description == bookUpdateDto.Title)
                ModelState.AddModelError(nameof(BookUpdateDto), "The description and title must be different!");

            TryValidateModel(bookUpdateDto);

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            Mapper.Map(bookUpdateDto, book);

            Repository.UpdateBook(book);

            if (!Repository.Save())
                throw new Exception($"Patching book {id} for author {authorId} failed.");

            return NoContent();
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid authorId, Guid id)
        {
            if (!Repository.AuthorExists(authorId))
                return NotFound();

            Book book = Repository.GetBook(authorId, id);

            if (book == null) return NotFound();

            Repository.DeleteBook(book);

            if (!Repository.Save())
                throw new Exception($"Failed to delete book {id} for author {authorId}.");

            Logger.LogInformation(100, $"Book {id} for author {authorId} was deleted.");

            return NoContent();
        }
    }
}
