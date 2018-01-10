using Library.API.Data;
using Library.API.Helpers;
using Library.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Services.LibService
{
    public class LibraryRepository : ILibraryRepository
    {
        public LibraryContext Context { get; }

        public LibraryRepository(LibraryContext context)
        {
            Context = context;
        }

        public PagedList<Author> GetAuthors(AuthorsResourceParameters resourceParameters)
        {
            var collectionBeforePaging = Context.Authors
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName);

            return PagedList<Author>.Create(collectionBeforePaging, resourceParameters.PageNumber, resourceParameters.PageSize);

        }

        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            return Context.Authors.Where(a => authorIds.Contains(a.Id))
                .OrderBy(a => a.FirstName).OrderBy(a => a.LastName).ToList();
        }

        public Author GetAuthor(Guid authorId)
        {
            return Context.Authors.FirstOrDefault(a => a.Id == authorId);
        }

        public void CreateAuthor(Author author)
        {
            author.Id = Guid.NewGuid();
            Context.Authors.Add(author);

            // the repository fills the id (instead of using identity columns)
            if (author.Books.Any())
            {
                foreach (var book in author.Books)
                {
                    book.Id = Guid.NewGuid();
                }
            }
        }

        public void UpdateAuthor(Author author)
        {
            throw new NotImplementedException();
        }

        public bool AuthorExists(Guid authorId)
        {
            return Context.Authors.Any(a => a.Id == authorId);
        }

        public void DeleteAuthor(Author author)
        {
            Context.Authors.Remove(author);
        }

        public IEnumerable<Book> GetBooks(Guid authorId)
        {
            return Context.Books
                .Where(b => b.AuthorId == authorId).OrderBy(b => b.Title).ToList();
        }
        
        public Book GetBook(Guid authorId, Guid bookId)
        {
            return Context.Books
                .Where(b => b.AuthorId == authorId && b.Id == bookId).FirstOrDefault();
        }

        public void CreateBook(Guid authorId, Book book)
        {
            var author = GetAuthor(authorId);
            if (author != null)
            {
                // if there isn't an id filled out (ie: we're not upserting),
                // we should generate one
                if (book.Id == Guid.Empty)
                    book.Id = Guid.NewGuid();

                author.Books.Add(book);
            }
        }

        public void UpdateBook(Book book)
        {
            //throw new NotImplementedException();
        }

        public void DeleteBook(Book book)
        {
            Context.Books.Remove(book);
        }

        public bool Save()
        {
            return (Context.SaveChanges() >= 0);
        }

    }
}
