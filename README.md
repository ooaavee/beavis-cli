# Beavis CLI

Beavis CLI library adds a web-cli (terminal in web browser) support for ASP.NET Core applications. It's very easy to configure and develope new custom commands with .NET Core.

## Install

You can get the library from <a href="https://www.nuget.org/packages/Ooaavee.Xxxx">NuGet</a>.

```
PM> Install-Package Ooaavee.BeavisCli
```

## Basic configuration

To configure your ASP.NET Core application to use the Beavis CLI library: Just add services to the container and configure the HTTP request pipeline in the application `Startup` code.

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddBeavisCli();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseBeavisCli();
    }
}
```


That's all, voilà! After this, just start your application and open the address `/terminal` in your web browser and the terminal is shown.

<img src="/doc/images/empty_terminal.jpg" style="width: 100%">


## Developing custom commands

xx
