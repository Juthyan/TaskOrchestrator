using System.Text.Json.Serialization;
using System.Threading.Channels;
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
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

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
    var task =  await taskRepository.GetAsync(id, ct);
    return task is null ? Results.NotFound() : Results.Ok(task);

});

app.Run();
