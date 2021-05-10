using AutoMapper;
using Books.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Books.API.Filters
{
    public class BookWithCoversResultFilterAttribute : ResultFilterAttribute
    {
        public override async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next)
        {
            var resultFromAction = context.Result as ObjectResult;
            if (resultFromAction.Value == null
                || resultFromAction.StatusCode < 200
                || resultFromAction.StatusCode >= 300)
            {
                await next();
                return;
            }

            //Value contains a tuple, so need to cast it back

            //Deconstructing the named properties from the value Tuple (Book & BookCover)
            //var (book, bookCovers) = ((Entities.Book book,
            //        IEnumerable<ExternalModels.BookCover> bookCovers))resultFromAction.Value;

            //Using var will resolve into temp.Item1 & temp.Item2
            //var temp = ((Entities.Book book,
            //        IEnumerable<ExternalModels.BookCover> bookCovers))resultFromAction.Value;

            //No need to mentioned named properties in our case. We simply desconstruct the tuple to the variables we named anyway we choose
            var (book, bookCovers) = ((Entities.Book,
                    IEnumerable<ExternalModels.BookCover>))resultFromAction.Value;

            var mapper = context.HttpContext.RequestServices.GetRequiredService<IMapper>();

            var mappedBook = mapper.Map<BookWithCovers>(book);
            resultFromAction.Value = mapper.Map(bookCovers, mappedBook);

            await next();
        }
    }
}