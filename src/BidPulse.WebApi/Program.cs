using BidPulse.Application;
using BidPulse.Persistence;
using BidPulse.WebApi;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddResponseCompression();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseResponseCompression();
app.UseExceptionHandler();
app.MapOpenApi();

app.Run();