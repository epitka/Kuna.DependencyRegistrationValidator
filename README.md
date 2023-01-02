# Kuna.Extensions.DependencyInjection.Validation

Effortless testing of dependency registrations with Microsoft.Extensions.DependencyInjection

### Installation

Install the [Nuget Package](https://www.nuget.org/packages/Kuna.Extensions.DependencyInjection.Validation)

### Package Manager Console

```Install-Package Kuna.Extensions.DependencyInjection.Validation```

### .NET Core CLI

```dotnet add package Kuna.Extensions.DependencyInjection.Validation```

### Usage

This library allows you to scan assemblies and verify that all dependencies required for the instantiation of types are registered with the dependency injection container. It has been tested exclusively with the base container implementation provided by Microsoft.

To utilize the library, you may add a test to your API project using the structure provided in the example.

First, create an instance of the generic ```RegistrationValidator```, where the generic type parameter is the entry point for your application (e.g. Program). Then, call the ```Validate``` method and provide the types to be tested. The ```RegistrationValidator``` will scan the assemblies for definitions of the specified types, including concrete types and open generic interface definitions. For example, if you are using MediatR to dispatch commands or events, you may specify open generic interfaces such as ```IRequest<>``` or ```INotificationHandler<>```.

The ```WebApplicationFactory``` is used to initiate the bootstrapping process for the application, which configures the ```ServiceCollection```. To prevent the app from running, it is necessary to handle two exceptions: ```FailureException``` and ```SuccessException```. When the ```SuccessException``` is thrown, it can be safely ignored, and optional feedback can be provided by calling ```Result.ToString()``` and writing it to the output console. In the event of a ```FailureException```, the test will fail when you call ```Assert.Failure``` (which works for both XUnit and NUnit). You can then call ```Result.ToString()``` to display the failures. If you desire a different output format, you may create an extension for the ```Result``` type.


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
