using RabbitMQ.Client;

public class RabbitMqService : IDisposable
{
    private IConnection _connection;
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(string hostName, string userName, string password, ILogger<RabbitMqService> logger)
    {
        _factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password
        };

        _logger = logger;

        CreateConnection();
    }

    public void CreateConnection()
    {
        try
        {
            _connection = _factory.CreateConnection();
            _logger.LogInformation("RabbitMQ connection established successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ connection.");
            throw;
        }
    }

    public void DisposeConnection()
    {
        if (_connection != null && _connection.IsOpen)
        {
            _connection.Close();
            _connection.Dispose();
            _logger.LogInformation("RabbitMQ connection disposed.");
        }
    }

    public IModel CreateChannel()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            _logger.LogInformation("RabbitMQ connection is not open. Recreating connection.");
            CreateConnection();
        }

        return _connection.CreateModel();
    }

    public void Dispose()
    {
        DisposeConnection();
    }
}
