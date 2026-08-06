using OrderFlow.Api.Domain;
using OrderFlow.Api.Repositories;
using OrderFlow.Api.Services;
using Xunit;

namespace OrderFlow.Tests;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidData_SetsPendingStatus()
    {
        var service = CreateService();

        var order = await service.CreateAsync("Ana", 149.90m);

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.NotEqual(Guid.Empty, order.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task CreateAsync_WithNonPositiveValue_ThrowsValidationError(decimal value)
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync("Ana", value));

        Assert.Equal("Valor total deve ser maior que zero.", exception.Message);
    }

    [Theory]
    [InlineData("Delivered")]
    [InlineData("1")]
    public async Task UpdateStatusAsync_WithInvalidStatus_ThrowsValidationError(string status)
    {
        var service = CreateService();
        var order = await service.CreateAsync("Ana", 149.90m);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateStatusAsync(order.Id, status));

        Assert.Equal(
            "Status inválido. Use Pending, Processing, Shipped ou Cancelled.",
            exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
    {
        var service = CreateService();

        var order = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(order);
    }

    private static OrderService CreateService()
    {
        return new OrderService(new InMemoryOrderRepository());
    }

    private sealed class InMemoryOrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = [];

        public Task<IReadOnlyList<Order>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Order>>(_orders.ToList());
        }

        public Task<Order?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_orders.FirstOrDefault(order => order.Id == id));
        }

        public Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            _orders.Add(order);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            var index = _orders.FindIndex(existingOrder => existingOrder.Id == order.Id);
            if (index < 0)
            {
                throw new KeyNotFoundException();
            }

            _orders[index] = order;
            return Task.CompletedTask;
        }
    }
}
