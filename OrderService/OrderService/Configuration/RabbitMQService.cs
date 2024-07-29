using RabbitMQ.Client;

public class RabbitMqService : IDisposable
{
    private IConnection _connection;
    private readonly ConnectionFactory _factory;

    public RabbitMqService(string hostName, string userName, string password)
    {
        _factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password
        };

        CreateConnection();
    }

    public void CreateConnection()
    {
        try
        {
            _connection = _factory.CreateConnection();
            Console.WriteLine("RabbitMQ connection established successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex + "Failed to establish RabbitMQ connection.");
            throw;
        }
    }

    public void DisposeConnection()
    {
        if (_connection != null && _connection.IsOpen)
        {
            _connection.Close();
            _connection.Dispose();
            Console.WriteLine("RabbitMQ connection disposed.");
        }
    }

    public IModel CreateChannel()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            Console.WriteLine("RabbitMQ connection is not open. Recreating connection.");
            CreateConnection();
        }

        return _connection.CreateModel();
    }

    public void Dispose()
    {
        DisposeConnection();
    }
}
