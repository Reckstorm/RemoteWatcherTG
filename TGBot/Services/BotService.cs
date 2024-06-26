using System.Globalization;
using Domain;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TGBot.Services
{
    public class BotService : BackgroundService
    {
        private readonly IMediator _mediator;
        private readonly TelegramBotClient _botClient;
        private readonly ReceiverOptions _receiverOptions;

        public BotService(IMediator mediator)
        {
            _mediator = mediator;
            _botClient = new TelegramBotClient("6616145643:AAGcDgfj23x8wMez9zeiiTXbTEfS9EBM8rk");
            _receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: _receiverOptions,
                cancellationToken: stoppingToken
            );
        }


        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            messageText = messageText.Trim();

            var chatId = message.Chat.Id;
            string response = "Command is not supported";


            if (messageText.StartsWith("Start", true, CultureInfo.CurrentCulture))
            {
                var result = await _mediator.Send(new Application.Logic.Start.Command());
                if (result.IsSuccess && result.Value != null)
                {
                    response = $"Blocker started successfully at {DateTime.Now.ToLongTimeString()}";
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: response,
                        cancellationToken: cancellationToken);
                }
                return;
            }
            if (messageText.StartsWith("Rules set", true, CultureInfo.CurrentCulture))
            {
                var result = await _mediator.Send(new Application.RProcesses.List.Query());
                if (result.IsSuccess && result.Value != null)
                {
                    response = $"{result.Value}";
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: response,
                        cancellationToken: cancellationToken);
                }
                return;
            }
            if (messageText.StartsWith("Edit rule", true, CultureInfo.CurrentCulture))
            {
                response = "Invalid rule data.\nExpected format is:\n\"Edit Rule:ProcessName;BlockStartTime;BlockEndTime\".\nTime should be in the following format: \"00:00:00\"";

                var ruleInfo = messageText.Substring(messageText.IndexOf(':') + 1).Split(';');

                if (ruleInfo.Length < 3)
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: response,
                        cancellationToken: cancellationToken);
                else
                {
                    TimeOnly BlockStartTime;
                    TimeOnly BlockEndTime;

                    string processName = ruleInfo[0];
                    bool startTimeConvRes = TimeOnly.TryParse(ruleInfo[1], out BlockStartTime);
                    bool endTimeConvRes = TimeOnly.TryParse(ruleInfo[2], out BlockEndTime);
                    if (string.IsNullOrEmpty(processName) || !startTimeConvRes || !endTimeConvRes)
                        await botClient.SendTextMessageAsync(
                             chatId: chatId,
                             text: response,
                             cancellationToken: cancellationToken);
                    else
                    {
                        var result = await _mediator.Send(new Application.RProcesses.Edit.Command { Process = new RProcess(processName, BlockStartTime, BlockEndTime) });

                        if (result.IsSuccess && result.Value != null)
                        {
                            response = "Rule updated successfully";
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: response,
                                cancellationToken: cancellationToken);
                        }
                    }
                }
                return;
            }
            else
            {
                // Echo received message text
                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: response,
                   cancellationToken: cancellationToken);
                return;
            }
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}