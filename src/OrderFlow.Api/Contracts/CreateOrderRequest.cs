namespace OrderFlow.Api.Contracts;

public sealed record CreateOrderRequest(string? Cliente, decimal ValorTotal);
