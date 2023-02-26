var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.UseWhen(
    context => context.Request.Path == "/time_short", // condition: if the request path "/time_short"
    HandleTimeShortRequest
);

app.MapWhen(
    context => context.Request.Path == "/time", // condition: if the request path "/time"
    HandleTimeRequest
);

app.Run(async context =>
{
    await context.Response.WriteAsync("Hello world");
});

app.Run();

void HandleTimeShortRequest(IApplicationBuilder appBuilder)
{
    appBuilder.Use(async (context, next) =>
    {
        var time = DateTime.Now.ToShortTimeString();
        Console.WriteLine($"current time_short: {time}");
        await next();   // calling the following middleware
    });

    appBuilder.Run(async context =>
    {
        var time = DateTime.Now.ToShortTimeString();
        await context.Response.WriteAsync($"Time: {time}");
    });
}

void HandleTimeRequest(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async context =>
    {
        var time = DateTime.Now.ToShortTimeString();
        await context.Response.WriteAsync($"current time: {time}");
    });
}