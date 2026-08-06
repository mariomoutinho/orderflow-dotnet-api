using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrderFlow.Api.Domain;

namespace OrderFlow.Api.Repositories;

public sealed class JsonOrderRepository : IOrderRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonOrderRepository(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("O caminho do arquivo de pedidos é obrigatório.", nameof(filePath));
        }

        _filePath = Path.GetFullPath(filePath);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            return await ReadOrdersAsync(cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var orders = await ReadOrdersAsync(cancellationToken);
            return orders.FirstOrDefault(order => order.Id == id);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var orders = await ReadOrdersAsync(cancellationToken);
            orders.Add(order);
            await WriteOrdersAsync(orders, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var orders = await ReadOrdersAsync(cancellationToken);
            var index = orders.FindIndex(existingOrder => existingOrder.Id == order.Id);

            if (index < 0)
            {
                throw new KeyNotFoundException($"Pedido {order.Id} não encontrado para atualização.");
            }

            orders[index] = order;
            await WriteOrdersAsync(orders, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<Order>> ReadOrdersAsync(CancellationToken cancellationToken)
    {
        await EnsureStorageAsync(cancellationToken);
        var json = await File.ReadAllTextAsync(_filePath, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidDataException(
                $"O arquivo de pedidos '{_filePath}' está vazio. Use [] para representar uma lista vazia.");
        }

        try
        {
            return JsonSerializer.Deserialize<List<Order>>(json, _jsonOptions)
                ?? throw new InvalidDataException(
                    $"O arquivo de pedidos '{_filePath}' deve conter uma lista JSON.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"O arquivo de pedidos '{_filePath}' contém JSON inválido. Corrija o arquivo antes de continuar.",
                exception);
        }
    }

    private async Task EnsureStorageAsync(CancellationToken cancellationToken)
    {
        var directoryPath = Path.GetDirectoryName(_filePath)
            ?? throw new InvalidOperationException("Não foi possível determinar o diretório de dados.");

        Directory.CreateDirectory(directoryPath);

        if (!File.Exists(_filePath))
        {
            await File.WriteAllTextAsync(_filePath, "[]", Encoding.UTF8, cancellationToken);
        }
    }

    private async Task WriteOrdersAsync(List<Order> orders, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(orders, _jsonOptions);
        var temporaryFilePath = $"{_filePath}.tmp";

        await File.WriteAllTextAsync(temporaryFilePath, json, Encoding.UTF8, cancellationToken);
        File.Move(temporaryFilePath, _filePath, overwrite: true);
    }
}
