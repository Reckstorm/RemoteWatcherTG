using Application.RProcesses;
using TGBot;
using TGBot.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(List.Handler).Assembly));

builder.Services.AddHostedService<BotService>();

builder.Services.AddWindowsService();

var host = builder.Build();
host.Run();