using Books.API.Contexts;
using Books.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Books.API.Services
{
    public class BookRepository : IBookRepository, IDisposable
    {
        private BooksContext _context;

        public BookRepository(BooksContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        //GetBook - Asynchronous version
        public async Task<Book> GetBookAsync(Guid id)
        {
            return await _context.Books.Include(b => b.Author).Where(b => b.Id == id).FirstOrDefaultAsync();
        }

        //GetBook - Asynchronous version
        public Book GetBook(Guid id)
        {
            return _context.Books.Include(b => b.Author).Where(b => b.Id == id).FirstOrDefault();
        }

        //GetBooks - Asynchronous version
        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            return await _context.Books.Include(b => b.Author).ToListAsync();
        }

        //GetBooks - Synchronous version
        public IEnumerable<Book> GetBooks()
        {
            return _context.Books.Include(b => b.Author).ToList();
        }

        public void AddBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            _context.Books.Add(book);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }
    }
}