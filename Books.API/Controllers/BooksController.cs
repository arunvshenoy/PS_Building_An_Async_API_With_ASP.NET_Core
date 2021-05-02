using AutoMapper;
using Books.API.Filters;
using Books.API.Models;
using Books.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Books.API.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository bookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository ??
                throw new ArgumentNullException(nameof(bookRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [BooksResultFilter()] //MVC's Result Filter (keeps the controller code simple)
        public async Task<IActionResult> GetBooks()
        {
            var bookEntities = await _bookRepository.GetBooksAsync();
            return Ok(bookEntities);
        }

        [HttpGet]
        [Route("{id}", Name = "GetBook")]
        [BookResultFilter()] //MVC's Result Filter (keeps the controller code simple)
        public async Task<IActionResult> GetBook(Guid id)
        {
            var bookEntity = await _bookRepository.GetBookAsync(id);
            if (bookEntity == null)
            {
                return NotFound();
            }

            return Ok(bookEntity);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(BookForCreation bookForCreation)
        {
            if (bookForCreation == null)
            {
                throw new ArgumentNullException(nameof(bookForCreation));
            }

            //Mapping BookForCreation object to actual Book entity object
            var bookEntity = _mapper.Map<Entities.Book>(bookForCreation);

            //Call repository service to add the book
            _bookRepository.AddBook(bookEntity);

            //Async call to Save the book to database/store
            await _bookRepository.SaveChangesAsync(); //Author won't be refreshed here

            //Fetch (refresh) the book from the data store, including the author
            await _bookRepository.GetBookAsync(bookEntity.Id);

            //Generate the "201" status code along with "Location" header value containing URI pointed for created author
            return CreatedAtRoute(
                "GetBook",
                new { id = bookEntity.Id },
                bookEntity);
        }
    }
}