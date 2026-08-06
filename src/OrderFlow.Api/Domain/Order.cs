namespace OrderFlow.Api.Domain;

public sealed class Order
{
    public Guid Id { get; init; }
    public string Cliente { get; init; } = string.Empty;
    public decimal ValorTotal { get; init; }
    public OrderStatus Status { get; set; }
    public DateTimeOffset CriadoEm { get; init; }
}
