using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Books.API.Filters
{
    //Result filter - Keeps the controller code simple
    public class BookResultFilterAttribute : ResultFilterAttribute
    {
        public override async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next)
        {
            var resultFromAction = context.Result as ObjectResult;

            //If no result value or responses other than 200 statuses
            if (resultFromAction?.Value == null
                 || resultFromAction.StatusCode < 200
                 || resultFromAction.StatusCode >= 300)
            {
                await next();
                return;
            }

            //Get Automapper services
            var mapper = context.HttpContext.RequestServices.GetRequiredService<IMapper>();

            //Map the result value using Automapper
            resultFromAction.Value = mapper.Map<Models.Book>(resultFromAction.Value);

            await next();
        }
    }
}
