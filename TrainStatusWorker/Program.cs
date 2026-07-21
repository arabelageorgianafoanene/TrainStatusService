using Microsoft.EntityFrameworkCore;
using TrainStatusWorker;
using TrainStatusWorker.Messaging;
using TrainStatusWorker.Persistence;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<ReadModelDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("ReadModelDb")));

builder.Services.AddSingleton(sp =>
    RabbitMqConnection.CreateAsync(sp.GetRequiredService<IConfiguration>()).GetAwaiter().GetResult());

var host = builder.Build();
host.Run();
