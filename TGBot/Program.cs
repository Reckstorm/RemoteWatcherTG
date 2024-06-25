using System.Globalization;
using Domain;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBot.Extensions;

var services = Services.CreateProvider();

var botClient = new TelegramBotClient("6616145643:AAGcDgfj23x8wMez9zeiiTXbTEfS9EBM8rk");

using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

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
    Message sentMessage;

    var mediator = services.GetService(typeof(IMediator)) as IMediator;

    if (messageText.StartsWith("Start", true, CultureInfo.CurrentCulture))
    {
        var result = await mediator.Send(new Application.Logic.Start.Command());
        if (result.IsSuccess && result.Value != null)
        {
            response = $"Blocker started successfully at {DateTime.Now.ToLongTimeString()}";
            sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: response,
                cancellationToken: cancellationToken);
        }
        return;
    }
    if (messageText.StartsWith("Rules set", true, CultureInfo.CurrentCulture))
    {
        var result = await mediator.Send(new Application.RProcesses.List.Query());
        if (result.IsSuccess && result.Value != null)
        {
            response = $"{result.Value}";
            sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: response,
                cancellationToken: cancellationToken);
        }
        return;
    }
    if (messageText.StartsWith("Edit rule", true, CultureInfo.CurrentCulture))
    {
        response = "Invalid rule data.\nExpected format is:\n\"Edit Rule:ProcessName;BlockStartTime;BlockEndTime\".\nTime should be in the following format: \"00:00:00\"";

        var ruleInfo = messageText.Substring(messageText.IndexOf(':')+1).Split(';');

        if (ruleInfo.Length < 3)
            sentMessage = await botClient.SendTextMessageAsync(
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
                sentMessage = await botClient.SendTextMessageAsync(
                     chatId: chatId,
                     text: response,
                     cancellationToken: cancellationToken);
            else
            {
                var result = await mediator.Send(new Application.RProcesses.Edit.Command { Process = new RProcess(processName, BlockStartTime, BlockEndTime) });

                if (result.IsSuccess && result.Value != null)
                {
                    response = "Rule updated successfully";
                    sentMessage = await botClient.SendTextMessageAsync(
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
        sentMessage = await botClient.SendTextMessageAsync(
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