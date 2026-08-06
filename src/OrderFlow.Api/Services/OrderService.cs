using OrderFlow.Api.Domain;
using OrderFlow.Api.Repositories;

namespace OrderFlow.Api.Services;

public sealed class OrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Order> CreateAsync(
        string? cliente,
        decimal valorTotal,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cliente))
        {
            throw new ArgumentException("Cliente é obrigatório.");
        }

        if (valorTotal <= 0)
        {
            throw new ArgumentException("Valor total deve ser maior que zero.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Cliente = cliente.Trim(),
            ValorTotal = valorTotal,
            Status = OrderStatus.Pending,
            CriadoEm = DateTimeOffset.UtcNow
        };

        await _repository.AddAsync(order, cancellationToken);
        return order;
    }

    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetAllAsync(cancellationToken);
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Order?> UpdateStatusAsync(
        Guid id,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = status?.Trim();
        if (string.IsNullOrEmpty(normalizedStatus)
            || !Enum.GetNames<OrderStatus>().Contains(
                normalizedStatus,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Status inválido. Use Pending, Processing, Shipped ou Cancelled.");
        }

        var parsedStatus = Enum.Parse<OrderStatus>(normalizedStatus, ignoreCase: true);
        var order = await _repository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        order.Status = parsedStatus;
        await _repository.UpdateAsync(order, cancellationToken);
        return order;
    }
}
