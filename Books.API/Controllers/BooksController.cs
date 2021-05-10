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
        //[BookResultFilter()] //MVC's Result Filter (keeps the controller code simple) - result-filter attribute for Book return type
        [BookWithCoversResultFilter()]
        //ActionResult<T> - actual type is inferred from the generic type parameter (ASP.NET Core 2.1 & higher) - Best practice
        //IActionResult - actual type could no longer be inferred, hence use [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Book))]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var bookEntity = await _bookRepository.GetBookAsync(id);
            if (bookEntity == null)
            {
                return NotFound();
            }

            //return Ok(bookEntity); //Old object

            //Single call to external API
            //var bookCover = await _booksRespository.GetBookCoverAsync("dummycover");

            //Multiple calls to external API
            var bookCovers = await _bookRepository.GetBookCoversAsync(id);

            ////Old way of writing Property bag or Tuple
            //var propertyBag = new Tuple<Entities.Book, IEnumerable<ExternalModels.BookCover>>
            //    (bookEntity, bookCovers);
            ////propertyBag.Item1 (for bookEntity) & propertyBag.Item2 (for bookCovers)

            ////Property bag or Tuple as Value (Value Tuple structure)
            //(Entities.Book book, IEnumerable<ExternalModels.BookCover> bookCovers)
            //    propertyBag = (bookEntity, bookCovers);
            ////propertyBag.book (for bookEntity) & propertyBag.bookCovers (for bookCovers)

            /////return Ok((book: bookEntity, bookCovers: bookCovers)); //Naming the properties
            return Ok((bookEntity, bookCovers)); //No need to name the properties
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