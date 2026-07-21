using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TrainEventsContract;
using TrainStatusService.Persistence;
using TrainStatusService.ReadModels;
using TrainStatusWorker.Messaging;
using TrainStatusWorker.Persistence;
using TrainStatusWorker.ReadModels;

namespace TrainStatusService.Messaging;

public class TrainEventsConsumer : BackgroundService
{
    private readonly RabbitMqConnection _rabbitConnection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TrainEventsConsumer> _logger;
    private IModel? _channel;

    private const string ExchangeName = "train.events";
    private const string QueueName = "worker.train-readmodel";

    public TrainEventsConsumer(
        RabbitMqConnection rabbitConnection,
        IServiceScopeFactory scopeFactory,
        ILogger<TrainEventsConsumer> logger)
    {
        _rabbitConnection = rabbitConnection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _channel = _rabbitConnection.Connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(QueueName, ExchangeName, routingKey: "train.*");

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        _logger.LogInformation("RabbitMQ consumer initialized. Exchange: {Exchange}, Queue: {Queue}",
            ExchangeName, QueueName);

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (sender, ea) =>
        {
            var routingKey = ea.RoutingKey;
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());

            _logger.LogInformation("Received message. RoutingKey: {RoutingKey}, Body: {Body}",
                routingKey, body);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ReadModelDbContext>();

                await HandleMessageAsync(routingKey, body, db, stoppingToken);

                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                _logger.LogInformation("Message processed and acked.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message. RoutingKey: {RoutingKey}", routingKey);
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel!.BasicConsume(QueueName, autoAck: false, consumer);

        return Task.CompletedTask;
    }

    private static async Task HandleMessageAsync(
        string routingKey, string json, ReadModelDbContext db, CancellationToken ct)
    {
        switch (routingKey)
        {
            case "train.registered":
                var registered = JsonSerializer.Deserialize<TrainRegistered>(json)!;
                db.TrainSummaries.Add(new TrainSummary
                {
                    TrainId = registered.TrainId,
                    CurrentStatus = registered.InitialStatus,
                    LastUpdatedAtUtc = DateTime.UtcNow
                });
                break;

            case "train.status-changed":
                var statusChanged = JsonSerializer.Deserialize<TrainStatusChanged>(json)!;
                var summary = await db.TrainSummaries.FindAsync(new object[] { statusChanged.TrainId }, ct);
                if (summary is not null)
                {
                    summary.CurrentStatus = statusChanged.NewStatus;
                    summary.LastUpdatedAtUtc = DateTime.UtcNow;
                }
                break;

            default:
                break;
        }

        await db.SaveChangesAsync(ct);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}