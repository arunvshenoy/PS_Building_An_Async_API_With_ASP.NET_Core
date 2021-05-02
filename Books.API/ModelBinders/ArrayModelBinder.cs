using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Books.API.ModelBinders
{
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            //Check if CustomBinder is used with other types. Because this custom model binder only works with IEnumerable types
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            //Get the inputted value through the value provider
            var value = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName).ToString();

            //Ensure the value is not null or empty
            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            //Otherwise - check and convert list of Guid
            //Type info from the current model type and take first Generic Type Argument (i.e., Guid here)
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);

            //Convert each item in the value list to enumerable type (i.e., each guid string to Guid object)
            var values = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(x => converter.ConvertFromString(x.Trim()))
                              .ToArray();

            //Create an array of that type and set it to Model value
            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues; //set to model context

            //Return a successful result, passing in the Model
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }
}