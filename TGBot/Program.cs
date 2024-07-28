using Application.Rules;
using MediatR;
using TGBot.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(List.Handler).Assembly));

builder.Services.AddHostedService<BotService>();

builder.Services.AddWindowsService();

var host = builder.Build();

var services = host.Services;

try
{
    await services.GetService<IMediator>().Send(new Application.Logic.Start.Command());
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during the migration");
}

host.Run();