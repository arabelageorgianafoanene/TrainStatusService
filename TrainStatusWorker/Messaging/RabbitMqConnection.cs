
using RabbitMQ.Client;

namespace TrainStatusWorker.Messaging
{
    public class RabbitMqConnection : IAsyncDisposable
    {
        public IConnection Connection { get; }

        private RabbitMqConnection(IConnection connection)
        {
            Connection = connection;
        }

        public static async Task<RabbitMqConnection> CreateAsync(IConfiguration configuration)
        {
            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMq:Host"] ?? "localhost",
                UserName = configuration["RabbitMq:User"] ?? "guest",
                Password = configuration["RabbitMq:Password"] ?? "guest",
            };
            var connection = await factory.CreateConnectionAsync();
            return new RabbitMqConnection(connection);
        }

        public async ValueTask DisposeAsync()
        {
            if (Connection is not null)
                await Connection.DisposeAsync();
        }
    }
}
