using ChatApp.Bot.Consumers;
using ChatApp.Infrastructure.ExternalApis;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true);

builder.Services.AddHttpClient<StooqClient>();
builder.Services.AddHostedService<StockCommandConsumer>();

var host = builder.Build();
await host.RunAsync();
