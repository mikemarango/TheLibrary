using Library.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Services.LibService
{
    public interface ILibraryRepository
    {
        IEnumerable<Author> GetAuthors();
        Author GetAuthor(Guid authorId);
        IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds);
        void CreateAuthor(Author author);
        void DeleteAuthor(Author author);
        void UpdateAuthor(Author author);
        bool AuthorExists(Guid authorId);
        IEnumerable<Book> GetBooks(Guid authorId);
        Book GetBook(Guid authorId, Guid bookId);
        void CreateBook(Guid authorId, Book book);
        void UpdateBook(Book book);
        void DeleteBook(Book book);
        bool Save();
    }
}
