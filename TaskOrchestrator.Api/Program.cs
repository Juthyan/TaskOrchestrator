using System.Text.Json.Serialization;
using Anthropic.SDK;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using TaskOrchestrator.Application;
using TaskOrchestrator.Domain;
using TaskOrchestrator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<TaskChannels>();
builder.Services.AddSingleton<TaskMetrics>();
builder.Services.AddSingleton<TaskActivitySource>();
builder.Services.AddHostedService<TaskWorker>();
builder.Services.AddScoped<EnqueueTaskCommandHandler>();
builder.Services.AddScoped<RestartTaskCommandHandler>();
builder.Services.AddScoped<CancelTaskCommandHandler>();
builder.Services.AddScoped<ClassifyAndEnqueueTaskCommandHandler>();
builder.Services.AddScoped<ITaskClassifier, AnthropicTaskClassifier>();

var anthropicKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "dummy-key";
builder.Services.AddSingleton(new AnthropicClient(anthropicKey));

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (databaseUrl != null)
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};Trust Server Certificate=true";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Database=taskdb;Username=postgres;Password=postgres";
}

builder.Services.AddDbContext<TaskOrchestratorDbContext>(options => 
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ITaskRepository, EfCoreTaskRepository>();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource("TaskOrchestrator")
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter("TaskOrchestrator")
        .AddConsoleExporter());

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://task-orchestrator-ks92u1tv2-judithyann971-5200s-projects.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskOrchestratorDbContext>();
    db.Database.Migrate();
}

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

app.MapPost("/tasks/classify-and-enqueue", async (ClassifyAndEnqueueTaskCommand command, ClassifyAndEnqueueTaskCommandHandler handler, CancellationToken ct) =>
{
    var id = await handler.HandleAsync(command, ct);
    return Results.Created($"/tasks/{id}", new { id });
});

app.MapGet("/tasks", async (ITaskRepository taskRepository, CancellationToken ct) =>
{
    var tasks = await taskRepository.GetAllAsync(ct);
    return Results.Ok(tasks);
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
