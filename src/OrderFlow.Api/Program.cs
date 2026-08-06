using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using OrderFlow.Api.Contracts;
using OrderFlow.Api.Repositories;
using OrderFlow.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<IOrderRepository>(_ =>
    new JsonOrderRepository(
        Path.Combine(builder.Environment.ContentRootPath, "Data", "orders.json")));
builder.Services.AddSingleton<OrderService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var (statusCode, message) = exception switch
        {
            BadHttpRequestException => (
                StatusCodes.Status400BadRequest,
                "Corpo da requisição inválido."),
            InvalidDataException invalidDataException => (
                StatusCodes.Status500InternalServerError,
                invalidDataException.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Ocorreu um erro interno.")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { erro = message });
    });
});

app.MapPost("/orders", async (
    CreateOrderRequest? request,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    if (request is null)
    {
        return Results.BadRequest(new { erro = "Corpo da requisição é obrigatório." });
    }

    try
    {
        var order = await service.CreateAsync(
            request.Cliente,
            request.ValorTotal,
            cancellationToken);

        return Results.Created($"/orders/{order.Id}", order);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { erro = exception.Message });
    }
});

app.MapGet("/orders", async (
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var orders = await service.GetAllAsync(cancellationToken);
    return Results.Ok(orders);
});

app.MapGet("/orders/{id:guid}", async (
    Guid id,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var order = await service.GetByIdAsync(id, cancellationToken);
    return order is null
        ? Results.NotFound(new { erro = "Pedido não encontrado." })
        : Results.Ok(order);
});

app.MapPatch("/orders/{id:guid}/status", async (
    Guid id,
    UpdateOrderStatusRequest? request,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    if (request is null)
    {
        return Results.BadRequest(new { erro = "Corpo da requisição é obrigatório." });
    }

    try
    {
        var order = await service.UpdateStatusAsync(id, request.Status, cancellationToken);
        return order is null
            ? Results.NotFound(new { erro = "Pedido não encontrado." })
            : Results.Ok(order);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { erro = exception.Message });
    }
});

app.Run();
