using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;


var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

//add the endpoint for calling the service
// first thing is the route
// than the parameters that we're going to pass => 
// the delagate method


app.MapGet("observation/{zip}", (string zip, [FromQuery] int? days) =>
{
    //this is actually the delegate method that will call into the precipitation service business logic

    //this is kind of a unit test that the service is responding
    //return Results.Ok(zip);
});

app.Run();
