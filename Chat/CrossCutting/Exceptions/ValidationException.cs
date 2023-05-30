namespace Chat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class ValidationException : ArgumentException
{
    private readonly ModelStateDictionary _modelState;

    public ModelStateDictionary ModelState => _modelState;
    public ValidationException(ModelStateDictionary modelState) : base("")
    {
        _modelState = modelState;
    }
}