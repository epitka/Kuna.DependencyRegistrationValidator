using Kuna.Extensions.DependencyInjection.Validation.Model;

namespace Kuna.Extensions.DependencyInjection.Validation.Exceptions;

public class SuccessException : Exception
{
    public SuccessException(Result result)
    {
        this.Result = result;
    }

    public Result Result { get; }
}
