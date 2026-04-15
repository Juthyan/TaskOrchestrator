using System.Text.Json.Serialization;
using System.Threading.Channels;
using Microsoft.AspNetCore.Diagnostics;
using TaskOrchestrator.Application;
using TaskOrchestrator.Domain;
using TaskOrchestrator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
builder.Services.AddSingleton<TaskChannels>();
builder.Services.AddHostedService<TaskWorker>();
builder.Services.AddScoped<EnqueueTaskCommandHandler>();
builder.Services.AddScoped<RestartTaskCommandHandler>();
builder.Services.AddScoped<CancelTaskCommandHandler>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        
        if (exception is DomainException domainEx)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = domainEx.Message });
        }
        else
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        }
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/tasks", async (EnqueueTaskCommand command, EnqueueTaskCommandHandler handler, CancellationToken ct) =>
{
    var id = await handler.HandleAsync(command, ct);
    return Results.Created($"/tasks/{id}", new { id });
});

app.MapGet("/tasks/{id}", async (Guid id, ITaskRepository taskRepository, CancellationToken ct) =>
{
    var task = await taskRepository.GetAsync(id, ct);
    return task is null ? Results.NotFound() : Results.Ok(task);
});

app.MapPost("/tasks/{id}/restart", async (Guid id, RestartTaskCommandHandler handler, CancellationToken ct) =>
{
    var command = new RestartTaskCommand(id);
    var taskId = await handler.HandleAsync(command, ct);
    return Results.Ok(new { id = taskId });
});

app.MapPost("/tasks/{id}/cancel", async (Guid id, CancelTaskCommandHandler handler, CancellationToken ct) =>
{
    var command = new CancelTaskCommand(id);
    await handler.HandleAsync(command, ct);
    return Results.NoContent();
});

app.Run();
