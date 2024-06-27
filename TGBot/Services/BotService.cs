using System.Globalization;
using Application.RProcesses;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TGBot.Services
{
    public class BotService : BackgroundService
    {
        private readonly IMediator _mediator;
        private readonly TelegramBotClient _botClient;
        private readonly ReceiverOptions _receiverOptions;
        private readonly List<UserRequestHandling> _userRequestHandlings;

        public BotService(IMediator mediator)
        {
            _mediator = mediator;
            _botClient = new TelegramBotClient("6616145643:AAGcDgfj23x8wMez9zeiiTXbTEfS9EBM8rk");
            _receiverOptions = new()
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
            };
            _userRequestHandlings = [];
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
            try
            {
                if (update.Type == UpdateType.Message && update.Message.Text != null)
                {
                    await HandleMessage(botClient, cancellationToken, update);
                }
                if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                {
                    await HandleCallback(botClient, cancellationToken, update);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private async Task HandleCallback(ITelegramBotClient botClient, CancellationToken cancellationToken, Update update)
        {
            // var callbackQuery = update.CallbackQuery;
            var callBackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var callbackQueryId = update.CallbackQuery.Id;
            string response = "Command is not supported";

            // if (callBackData == "notepad")
            // {
            //     await botClient.SendTextMessageAsync(callbackQueryId, $"Send process name you want to edit");
            //     return;
            // }
            await botClient.AnswerCallbackQueryAsync(callbackQueryId);
            await botClient.SendTextMessageAsync(chatId, "Send process time boundaries in a format \"00:00:00 - 12:00:00\".\nWhere the first number is a blocker start time, and the second is a blocker end time");
        }

        private async Task HandleMessage(ITelegramBotClient botClient, CancellationToken cancellationToken, Update update)
        {
            var messageText = update.Message.Text.Trim();

            var chatId = update.Message.Chat.Id;
            string response = "Command is not supported";

            if (messageText.StartsWith("/start", true, CultureInfo.CurrentCulture))
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
            if (messageText.StartsWith("/rules_set", true, CultureInfo.CurrentCulture))
            {
                var result = await _mediator.Send(new List.Query());
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
            if (messageText.StartsWith("/edit_rule", true, CultureInfo.CurrentCulture))
            {
                // response = "Invalid rule data.\nExpected format is:\n\"Edit Rule:ProcessName;BlockStartTime;BlockEndTime\".\nTime should be in the following format: \"00:00:00\"";
                response = "There no rules to edit";

                var rprocesses = (await _mediator.Send(new List.Query())).Value;

                if (rprocesses == null)
                {
                    await _botClient.SendTextMessageAsync(chatId: chatId, text: response);
                }

                List<List<InlineKeyboardButton>> buttons = [];

                for (int i = 0, j = 0; i < rprocesses.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        buttons.Add(new List<InlineKeyboardButton>() { InlineKeyboardButton.WithCallbackData(rprocesses[i].ProcessName, rprocesses[i].ProcessName) });
                    }
                    else
                    {
                        buttons[j].Add(InlineKeyboardButton.WithCallbackData(rprocesses[i].ProcessName, rprocesses[i].ProcessName));
                        j++;
                    }
                }

                var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                await _botClient.SendTextMessageAsync(chatId, "Choose what to start from", replyMarkup: inlineKeyboard);

                // var ruleInfo = messageText.Substring(messageText.IndexOf(':') + 1).Split(';');

                // if (ruleInfo.Length < 3)
                //     await botClient.SendTextMessageAsync(
                //         chatId: chatId,
                //         text: response,
                //         cancellationToken: cancellationToken);
                // else
                // {
                //     TimeOnly BlockStartTime;
                //     TimeOnly BlockEndTime;

                //     string processName = ruleInfo[0];
                //     bool startTimeConvRes = TimeOnly.TryParse(ruleInfo[1], out BlockStartTime);
                //     bool endTimeConvRes = TimeOnly.TryParse(ruleInfo[2], out BlockEndTime);
                //     if (string.IsNullOrEmpty(processName) || !startTimeConvRes || !endTimeConvRes)
                //         await botClient.SendTextMessageAsync(
                //              chatId: chatId,
                //              text: response,
                //              cancellationToken: cancellationToken);
                //     else
                //     {
                //         var result = await _mediator.Send(new Application.RProcesses.Edit.Command { Process = new RProcess(processName, BlockStartTime, BlockEndTime) });

                //         if (result.IsSuccess && result.Value != null)
                //         {
                //             response = "Rule updated successfully";
                //             await botClient.SendTextMessageAsync(
                //                 chatId: chatId,
                //                 text: response,
                //                 cancellationToken: cancellationToken);
                //         }
                //     }
                // }



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