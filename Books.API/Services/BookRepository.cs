using Books.API.Contexts;
using Books.API.Entities;
using Books.API.ExternalModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Books.API.Services
{
    public class BookRepository : IBookRepository, IDisposable
    {
        private BooksContext _context;
        private CancellationTokenSource _cancellationTokenSource; //Implements IDisposble
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BookRepository> _logger;

        public BookRepository(BooksContext context,
            IHttpClientFactory httpClientFactory, ILogger<BookRepository> logger)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
            _httpClientFactory = httpClientFactory ??
                throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
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

        //Set of Books
        public async Task<IEnumerable<Entities.Book>> GetBooksAsync(IEnumerable<Guid> bookIds)
        {
            return await _context.Books.Where(b => bookIds.Contains(b.Id))
                            .Include(b => b.Author).ToListAsync();
        }

        public async Task<BookCover> GetBookCoverAsync(string coverId)
        {
            //Call external API using HttpClient services (services.AddHttpClient() in Startup.cs)
            var httpClient = _httpClientFactory.CreateClient(); //Returns HttpClient instance

            //Find the Host URL from BookCovers.API project properties
            var response = await httpClient
                .GetAsync($"http://localhost:22710/api/bookcovers/{coverId}"); //Network call

            //If the response in Successful
            if (response.IsSuccessStatusCode)
            {
                //Read the book cover from response body
                return JsonSerializer.Deserialize<BookCover>(
                    await response.Content.ReadAsStringAsync(), //Reads the response object which isn't IO operation. ReadAsStringAsync() waits for the request to finish and response to start arriving. That is the response isn't neccesarily fully transferred yet nor completely bufferred, (allows streaming of large responses without having to hold the entire response in memory)
                    new JsonSerializerOptions //By default, JSON starts with lower case letter
                    {
                        //Tell JSON Serializer that it should be case-insensitive when matching field names from JSON response to propery names
                        PropertyNameCaseInsensitive = true
                    });
            }

            return null;
        }

        public async Task<IEnumerable<BookCover>> GetBookCoversAsync(Guid bookId)
        {
            var httpClient = _httpClientFactory.CreateClient(); //Create HTTP client instance
            var bookCovers = new List<BookCover>();
            _cancellationTokenSource = new CancellationTokenSource(); //Used to freeup threads if one Task fails

            //Create a list of fake book covers
            var bookCoverUrls = new[]
            {
                $"http://localhost:22710/api/bookcovers/{bookId}-dummycover1",
                //$"http://localhost:22710/api/bookcovers/{bookId}-dummycover2?returnFault=true",
                $"http://localhost:22710/api/bookcovers/{bookId}-dummycover2",
                $"http://localhost:22710/api/bookcovers/{bookId}-dummycover3",
                $"http://localhost:22710/api/bookcovers/{bookId}-dummycover4",
                $"http://localhost:22710/api/bookcovers/{bookId}-dummycover5",
            };

            #region Multiple API calls (one-by-one and not in parallel) i.e., Tasks in one-by-one
            //HttpResponseMessage response = null;
            //foreach (var bookCoverUrl in bookCoverUrls)
            //{
            //    response = await httpClient.GetAsync(bookCoverUrl);

            //    if (response.IsSuccessStatusCode)
            //    {
            //        bookCovers.Add(JsonSerializer.Deserialize<BookCover>(
            //                await response.Content.ReadAsStringAsync(),
            //                new JsonSerializerOptions //By default, JSON starts with lower case letter
            //                {
            //                    //Tell JSON Serializer that it should be case-insensitive when matching field names from JSON response to propery names
            //                    PropertyNameCaseInsensitive = true
            //                }));
            //    }
            //}

            //return bookCovers;
            #endregion

            #region #region Multiple API calls (in parallel) i.e., Tasks in parallel
            //Create the list tasks (Deferred execution)
            var downloadBookCoverTasksQuery = from bookCoverUrl
                                              in bookCoverUrls
                                              select DownloadBookCoverAsync(httpClient, bookCoverUrl, _cancellationTokenSource.Token);

            //Start the tasks (Query evaluation by calling ToList)
            var downloadBookCoverTasks = downloadBookCoverTasksQuery.ToList();
            try
            {
                //WhenAll returns a single Task that isn't complete until every task in the collection has been completed.
                //Task.WhenAll ensures that it still returns list of book covers in the order they started.
                //Single Task has a result of List of BookCover
                //WhenAny can be used if completion of one task in sufficient
                return await Task.WhenAll(downloadBookCoverTasks);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                _logger.LogInformation($"{operationCanceledException.Message}");
                foreach (var task in downloadBookCoverTasksQuery)
                {
                    _logger.LogInformation($"Task {task.Id} has status {task.Status}");
                }

                return Enumerable.Empty<BookCover>(); //Returning 'empty' to BookCovers property
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                throw;
            }
            #endregion

            return null;
        }

        private async Task<BookCover> DownloadBookCoverAsync(HttpClient httpClient, string bookCoverUrl, CancellationToken cancellationToken)
        {
            //Await - control is yield to the caller and a new task is created.
            //Through that task's Status property, the state of the task can be determined which the State machine then monitors to return control back to this method

            //Passing through the cancellation token when calling async
            //HttpClient is aware of cancellation tokens and will cancel the call when its notified of cancellation
            var response = await httpClient
                            .GetAsync(bookCoverUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                //return the BookCover (Deserialized from API service response data)
                var bookCover = JsonSerializer.Deserialize<BookCover>(
                        await response.Content.ReadAsStringAsync(),
                        new JsonSerializerOptions //By default, JSON starts with lower case letter
                        {
                            //Tell JSON Serializer that it should be case-insensitive when matching field names from JSON response to propery names
                            PropertyNameCaseInsensitive = true
                        });

                return bookCover;
            }

            //We aren't effectively cancelling anything. We are simply requesting cancellation and letting listeners know that cancellation is requested
            _cancellationTokenSource.Cancel();
            return null;
        }

        //To add new book
        public void AddBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            _context.Books.Add(book);
        }

        //To Save the context changes
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

                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }
    }
}