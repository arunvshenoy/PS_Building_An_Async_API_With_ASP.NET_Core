using AutoMapper;
using Books.API.Filters;
using Books.API.ModelBinders;
using Books.API.Models;
using Books.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Books.API.Controllers
{
    [Route("api/bookcollections")]
    [ApiController]
    [BooksResultFilter] //Both actions should be mapped to DTO models. Hence result filter will be applied to all the controller
    public class BookCollectionsController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public BookCollectionsController(IBookRepository bookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository ??
                throw new ArgumentNullException(nameof(bookRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("({bookIds})", Name = "GetBookCollection")]
        public async Task<IActionResult> GetBookCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> bookIds) //By default cannot bind the IEnumerable<Guid>, hence need custom model binder
        {
            var bookEntities = await _bookRepository.GetBooksAsync(bookIds);

            //To check if all of them are found
            if (bookIds.Count() != bookEntities.Count())
            {
                return NotFound();
            }

            return Ok(bookEntities);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBookCollection(IEnumerable<BookForCreation> bookCollection)
        {
            var bookEntities = _mapper.Map<IEnumerable<Entities.Book>>(bookCollection);

            //Loop each book entity object in the collection and add to the repository
            foreach (var bookEntity in bookEntities)
            {
                //Not an async action, runs in the same thread (less overhead from spinning up from the pool)
                _bookRepository.AddBook(bookEntity);
            }

            //Async action
            await _bookRepository.SaveChangesAsync();

            //Fetch the books to return after persisting the changes. Like this the authors are fetched for these
            var booksToReturn = await _bookRepository.GetBooksAsync(bookEntities.Select(b => b.Id).ToList());

            //Create list of Ids to return
            var bookIds = string.Join(",", booksToReturn.Select(a => a.Id));

            return CreatedAtRoute("GetBookCollection",
                new { bookIds },
                booksToReturn);
        }
    }
}