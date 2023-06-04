using Hubsson.Hackathon.Arcade.Client.Dotnet.Services;
using Hubsson.Hackathon.Arcade.Client.Dotnet.Settings;
using Microsoft.Extensions.Options;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json");
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<ArcadeSettings>(hostContext.Configuration.GetSection("ArcadeConfig"));
        services.Configure<BotSettings>(hostContext.Configuration.GetSection("BotConfig"));

        services.AddHostedService<Worker>();
        services.AddTransient<MatchService>();
        services.AddTransient(provider => provider.GetRequiredService<IOptions<ArcadeSettings>>().Value);
        services.AddTransient(provider => provider.GetRequiredService<IOptions<BotSettings>>().Value);

    })
    .Build();

await host.RunAsync();