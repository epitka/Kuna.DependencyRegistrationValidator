# Kuna.Extensions.DependencyInjection.Validation

Effortless testing of dependency registrations with Microsoft.Extensions.DependencyInjection

### Installation

Install the [Nuget Package](https://www.nuget.org/packages/Kuna.Extensions.DependencyInjection.Validation)

### Package Manager Console

```Install-Package Kuna.Extensions.DependencyInjection.Validation```

### .NET Core CLI

```dotnet add package Kuna.Extensions.DependencyInjection.Validation```

### Usage

This library allows you to scan assemblies and verify that all dependencies needed to instantiate types are registered with container. It has been tested only with Microsoft's base container implementation.

To use it just add test in your API project with structure as provided in example.

First, we instantiate generic ```RegistrationValidator``` where generic type parameter is entry point into your application such as ```Program```.
Next we call ```Validate``` method and provide types to be tested. ```RegitrationValidator``` scans assemblies for definitions of the types provided.  It currently supports concrete types, and open generic interface definitions. If you are using MediatR to dispatch commands/events then you can specify open generic interaces for the types such as ```IRequest<>``` or ```INotificationHandler<>``` for example.            

```WebApplicationFactory``` is used to start application bootstrapping process that configures ```ServiceCollection```. In order to stop the app from running, 2 exceptions must be handled (```FailureException``` and ```SuccessException```). When ```SuccessException``` is thrown we just ignore it and optionaly provide some feedback by calling ```Result.ToString()``` and write it to output console. In case of ```FailureException```, we call ```Assert.Failure``` (this works for both XUnit and NUnit), which causes test to fail and then we call ```Result.ToString()``` to display failures.  If you wish to have different output format, you can provide extension for ```Result``` type.


### Example
```c#
public class ServicesConfigurationTest
{
    private readonly ITestOutputHelper console;

    public ServicesConfigurationTest(ITestOutputHelper console)
    {
        this.console = console;
    }

    [Fact]
    public void VerifyDependencyRegistrations()
    {
        var assemblies = new[]
        {
            typeof(ShoppingCartsController).Assembly, // api
            typeof(IShoppingCartRepository).Assembly, // application
        };

        var verifier = new RegistrationValidator<Program>(assemblies);

        try
        {
            verifier.Validate(
                typeof(Controller),
                typeof(IHandleCommand<>));
        }
        catch (FailureException fe)
        {
            this.console.WriteLine(fe.Result.ToString());

            Assert.Fail("Could not resolve all dependencies");
        }
        catch (SuccessException e)
        {
            // short cut app bootstrap process
            // ignore
            this.console.WriteLine(e.Result.ToString());
        }
    }
}
```

#### Example of output

```
Xunit.Sdk.FailException
Assert.Fail(): Could not resolve all dependencies
   at Carts.Api.Tests.ServicesConfigurationTest.VerifyDependencyRegistrations() in C:\Github\Kuna\Kuna.EventSourcing\Sample\ECommerce\Carts\Carts.Api.Tests\ServicesConfigurationTests.cs:line 43



Number of types to verify: 6

Failure messages:
-----------------------------------------
Unable to resolve service for type 'Carts.Domain.Services.IProductPriceCalculator' while attempting to activate 'Carts.Application.CommandHandlers.AddProductHandler'.

Failed to resolve:
-----------------------------------------
Kuna.EventSourcing.Core.Commands.IHandleCommand`1[[Carts.Domain.Commands.AddProduct, Carts.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]

Resolved successfuly:
-----------------------------------------
Carts.Api.Controllers.ShoppingCartsController
Kuna.EventSourcing.Core.Commands.IHandleCommand`1[[Carts.Domain.Commands.CancelShoppingCart, Carts.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
Kuna.EventSourcing.Core.Commands.IHandleCommand`1[[Carts.Domain.Commands.ConfirmShoppingCart, Carts.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
Kuna.EventSourcing.Core.Commands.IHandleCommand`1[[Carts.Domain.Commands.OpenShoppingCart, Carts.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
Kuna.EventSourcing.Core.Commands.IHandleCommand`1[[Carts.Domain.Commands.RemoveProduct, Carts.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
```
