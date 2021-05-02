using Books.API.Filters;
using Books.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Books.API.Controllers
{
    [Route("api/synchronousbooks")]
    [ApiController]
    public class SynchronousBooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;

        public SynchronousBooksController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        }

        [HttpGet]
        [BooksResultFilter()] //MVC's Result Filter
        public IActionResult GetBooks()
        {
            var bookEntities = _bookRepository.GetBooks();
            return Ok(bookEntities);
        }

        [HttpGet("{id}")]
        [BookResultFilter()] //MVC's Result Filter
        public IActionResult GetBook(Guid id)
        {
            var bookEntity = _bookRepository.GetBook(id);
            if (bookEntity == null)
            {
                return NotFound();
            }

            return Ok(bookEntity);
        }
    }
}