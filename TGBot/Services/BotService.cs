using System.Globalization;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBot.Menus;

namespace TGBot.Services
{
    public class BotService : BackgroundService
    {
        private string _username { get; set; }
        private readonly IMediator _mediator;
        private readonly TelegramBotClient _botClient;
        private readonly ReceiverOptions _receiverOptions;
        private readonly Dictionary<long, UserRequest> _userRequests;

        public BotService(IMediator mediator)
        {
            _username = "reckstorm";
            _mediator = mediator;
            _botClient = new TelegramBotClient("6616145643:AAGcDgfj23x8wMez9zeiiTXbTEfS9EBM8rk");
            _receiverOptions = new()
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
            };
            _userRequests = [];
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
            await Task.Run(async () =>
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
            });
        }

        private async Task HandleCallback(ITelegramBotClient botClient, CancellationToken cancellationToken, Update update)
        {
            var callBackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var callbackQueryId = update.CallbackQuery.Id;
            var messageID = update.CallbackQuery.Message.MessageId;
            string response = "Invalid command";

            if (callBackData == null)
                await botClient.SendTextMessageAsync(chatId, response);

            response = "Choose an action:";


            //Base menu actions
            if (callBackData == BaseMenu.RProcess)
            {
                UpdateUserRequests(chatId, baseMenuSection: callBackData, step: 1);
                await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageID, text: response, replyMarkup: InlineKeyboards.RProcessesMenuKeyboard());
                return;
            }

            if (callBackData == BaseMenu.Process)
            {
                UpdateUserRequests(chatId, baseMenuSection: callBackData, step: 1);
                await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageID, text: response, replyMarkup: InlineKeyboards.ProcessesMenuKeyboard());
                return;
            }

            if (callBackData == BaseMenu.Logic)
            {
                UpdateUserRequests(chatId, baseMenuSection: callBackData, step: 1);
                await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageID, text: response, replyMarkup: InlineKeyboards.LogicMenuKeyboard());
                return;
            }

            //Logic menu actions
            if (_userRequests[chatId].BaseMenuSection == BaseMenu.Logic)
            {
                if (callBackData == LogicMenu.Start)
                {
                    var result = await _mediator.Send(new Application.Logic.Start.Command());
                    if (result.IsSuccess)
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId, "Blocker logic has been started successfully");
                        return;
                    }
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId, "Failed to start blocker logic");
                    return;
                }
                if (callBackData == LogicMenu.Stop)
                {
                    var result = await _mediator.Send(new Application.Logic.Start.Command());
                    if (result.IsSuccess)
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId, "Blocker logic has been started successfully");
                        return;
                    }
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId, "Failed to start blocker logic");
                    return;
                }
            }

            //Process menu actions
            if (_userRequests[chatId].BaseMenuSection == BaseMenu.Process)
            {
                if (callBackData == ProcessesMenu.List)
                {
                    var result = await _mediator.Send(new Application.Processes.List.Query());
                    if (!result.IsSuccess && result.Error != null)
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId, result.Error);
                        return;
                    }
                    UpdateUserRequests(chatId, baseMenuSection: _userRequests[chatId].BaseMenuSection, step: 2);
                    response = "Choose a process:";
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageID,
                        text: response,
                        replyMarkup: await InlineKeyboards.ListKeyboard(result.Value));

                }
            }

            // Back button actions
            if (callBackData == BaseMenu.Back && _userRequests[chatId].Step == 1)
            {
                UpdateUserRequests(chatId, baseMenuSection: null, step: 0);
                await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageID, text: response, replyMarkup: InlineKeyboards.BaseMenuKeyboard());
                return;
            }
            if (callBackData == BaseMenu.Back && _userRequests[chatId].Step == 2 && _userRequests[chatId].BaseMenuSection == BaseMenu.Process)
            {
                UpdateUserRequests(chatId, step: 1);
                await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageID, text: response, replyMarkup: InlineKeyboards.ProcessesMenuKeyboard());
                return;
            }
            //             var rprocesses = (await _mediator.Send(new List.Query())).Value;

            // if (rprocesses == null)
            // {
            //     await _botClient.SendTextMessageAsync(chatId: chatId, text: response);
            // }

            // var inlineKeyboard = new InlineKeyboardMarkup(await BuildInlineKeyBoard(rprocesses));

            // await _botClient.SendTextMessageAsync(chatId, "Choose what to start from", replyMarkup: inlineKeyboard);



            // await botClient.AnswerCallbackQueryAsync(callbackQueryId);
            // await botClient.SendTextMessageAsync(chatId, "Send process time boundaries in a format \"00:00:00 - 12:00:00\".\nWhere the first number is a blocker start time, and the second is a blocker end time");
        }

        private async Task HandleMessage(ITelegramBotClient botClient, CancellationToken cancellationToken, Update update)
        {
            var username = update.Message.Chat.Username;
            var messageText = update.Message.Text.Trim();
            var chatId = update.Message.Chat.Id;
            string response = "Invalid command";

            if (messageText.StartsWith("/menu", true, CultureInfo.CurrentCulture) && username.Equals(_username))
            {
                UpdateUserRequests(chatId);
                await _botClient.SendTextMessageAsync(chatId: chatId, text: "Choose an action", replyMarkup: InlineKeyboards.BaseMenuKeyboard());
                return;
            }
            else
            {
                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: response,
                   cancellationToken: cancellationToken);
                return;
            }
        }

        private void UpdateUserRequests(long chatId, string baseMenuSection = null, int step = 0, string processName = null, string rprocessName = null)
        {
            if (_userRequests.ContainsKey(chatId))
            {
                _userRequests[chatId].Step = step;
                _userRequests[chatId].BaseMenuSection = baseMenuSection;
                _userRequests[chatId].RProcessName = rprocessName;
                _userRequests[chatId].ProcessName = processName;
                return;
            }
            _userRequests.Add(chatId, new UserRequest() { BaseMenuSection = baseMenuSection });
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