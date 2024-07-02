using System.Globalization;
using System.Text.RegularExpressions;
using Domain;
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
            {
                await botClient.SendTextMessageAsync(chatId, response);
                return;
            }

            response = "Choose a submenu:";

            //Base menu actions
            if (_userRequests[chatId].BaseMenuSection == null)
            {
                if (callBackData == BaseMenu.RProcess)
                {
                    UpdateUserRequests(chatId, baseMenuSection: callBackData, step: ++_userRequests[chatId].Step);
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageID, text: response, replyMarkup: InlineKeyboards.RProcessesMenuKeyboard());
                    return;
                }

                if (callBackData == BaseMenu.Process)
                {
                    UpdateUserRequests(chatId, baseMenuSection: callBackData, step: ++_userRequests[chatId].Step);
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageID, text: response, replyMarkup: InlineKeyboards.ProcessesMenuKeyboard());
                    return;
                }

                if (callBackData == BaseMenu.Logic)
                {
                    UpdateUserRequests(chatId, baseMenuSection: callBackData, step: ++_userRequests[chatId].Step);
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageID, text: response, replyMarkup: InlineKeyboards.LogicMenuKeyboard());
                    return;
                }
            }

            //Logic menu actions
            if (_userRequests[chatId].BaseMenuSection == BaseMenu.Logic)
            {
                response = "Choose an action";
                if (callBackData == LogicMenu.Start)
                {
                    var result = await _mediator.Send(new Application.Logic.Start.Command());
                    if (!result.IsSuccess)
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId, result.Error);
                        return;
                    }
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId, "Blocker logic has been started successfully");
                    return;
                }
                if (callBackData == LogicMenu.Stop)
                {
                    var result = await _mediator.Send(new Application.Logic.Stop.Command());
                    if (!result.IsSuccess)
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId, result.Error);
                        return;
                    }
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId, "Blocker logic has been stopped successfully");
                    return;
                }
                if (callBackData == LogicMenu.Back)
                {
                    UpdateUserRequests(chatId, baseMenuSection: null);
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageID, text: response, replyMarkup: InlineKeyboards.BaseMenuKeyboard());
                    return;
                }
            }

            //Processes menu actions
            if (_userRequests[chatId].BaseMenuSection == BaseMenu.Process)
            {
                //Send a list of processes
                if (callBackData == ProcessesMenu.List)
                {
                    var result = await _mediator.Send(new Application.Processes.List.Query());
                    if (!result.IsSuccess && result.Error != null)
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId, result.Error);
                        return;
                    }
                    var items = result.Value.Select(x => x.ProcessName).ToList();
                    UpdateUserRequests(
                        chatId: chatId,
                        baseMenuSection:
                        _userRequests[chatId].BaseMenuSection,
                        items: items
                        );
                    response = "Choose a process:";
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageID,
                        text: response,
                        replyMarkup: await InlineKeyboards.ListKeyboard(items, ProcessesListMenu.Back));
                    return;
                }

                //Back to base menu
                if (callBackData == ProcessesMenu.Back)
                {
                    UpdateUserRequests(chatId, baseMenuSection: null);
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageID, text: response, replyMarkup: InlineKeyboards.BaseMenuKeyboard());
                    return;
                }

                //Process menu actions
                if (_userRequests[chatId].Items.Any(x => x.Equals(callBackData)))
                {
                    var result = await _mediator.Send(new Application.Processes.Details.Query { ProcessName = callBackData });
                    if (!result.IsSuccess)
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId, result.Error);
                        return;
                    }
                    UpdateUserRequests(
                        chatId,
                        baseMenuSection: _userRequests[chatId].BaseMenuSection,
                        item: callBackData,
                        items: _userRequests[chatId].Items);
                    response = $"Currently chosen process:\nID: {result.Value.ProcessId}\nName: {result.Value.ProcessName}\nStatus: {result.Value.IsRunning}.\nYou can either kill this process with the relevant button below or go back";
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageID,
                        text: response,
                        replyMarkup: InlineKeyboards.ProcessMenuKeyboard());
                    return;
                }

                //Back to Processes menu
                if (callBackData == ProcessesListMenu.Back)
                {
                    UpdateUserRequests(chatId, baseMenuSection: _userRequests[chatId].BaseMenuSection);
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageID, text: response, replyMarkup: InlineKeyboards.ProcessesMenuKeyboard());
                    return;
                }

                //Specific process actions
                if (callBackData == ProcessMenu.Kill)
                {
                    var result = await _mediator.Send(new Application.Processes.Kill.Command { ProcessName = _userRequests[chatId].Item });
                    if (!result.IsSuccess)
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId, result.Error);
                        return;
                    }
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId, $"Process {result.Value.ProcessName} successfully killed");
                    return;
                }

                //Back to Processes list
                if (callBackData == ProcessMenu.Back)
                {
                    var result = await _mediator.Send(new Application.Processes.List.Query());
                    if (!result.IsSuccess && result.Error != null)
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId, result.Error);
                        return;
                    }
                    var items = result.Value.Select(x => x.ProcessName).ToList();
                    UpdateUserRequests(
                        chatId: chatId,
                        baseMenuSection: _userRequests[chatId].BaseMenuSection,
                        items: items);
                    response = "Choose a process:";
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageID,
                        text: response,
                        replyMarkup: await InlineKeyboards.ListKeyboard(items, ProcessesListMenu.Back));
                    return;
                }
            }

            //RProcesses menu actions
            if (_userRequests[chatId].BaseMenuSection == BaseMenu.RProcess)
            {
                response = "Choose an action";

                if (callBackData == RProcessesMenu.Add)
                {
                    UpdateUserRequests(chatId: chatId, baseMenuSection: BaseMenu.RProcess, step: 1);
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.SendTextMessageAsync(chatId: chatId,
                        text: "Provide a process name in a form of \"Name: ProcessName\"\nWithout quotes!\nIn case you want to cancell this action send /cancell");
                    return;
                }

                if (callBackData == RProcessesMenu.List)
                {
                    var result = await _mediator.Send(new Application.RProcesses.List.Query());
                    if (result == null)
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId, "There are no rules to display");
                        return;
                    }
                    if (!result.IsSuccess && result.Error != null)
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId, result.Error);
                        return;
                    }
                    var items = result.Value.Select(x => x.ProcessName).ToList();
                    UpdateUserRequests(chatId: chatId,
                        baseMenuSection: _userRequests[chatId].BaseMenuSection,
                        items: items);
                    response = "Choose a process:";
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageID,
                        text: response,
                        replyMarkup: await InlineKeyboards.ListKeyboard(items, RProcessesListMenu.Back));
                    return;
                }

                if (callBackData == RProcessesMenu.EditAll)
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    return;
                }

                if (callBackData == RProcessesMenu.DeleteAll)
                {
                    var result = await _mediator.Send(new Application.RProcesses.DeleteAll.Command());
                    if (!result.IsSuccess && result.Error != null)
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId, result.Error);
                        return;
                    }
                    UpdateUserRequests(chatId: chatId,
                        baseMenuSection: _userRequests[chatId].BaseMenuSection);
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId, "All have been rules successfully deleted");
                    return;
                }

                if (callBackData == RProcessesMenu.Back)
                {
                    UpdateUserRequests(chatId, baseMenuSection: null);
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId);
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageID, text: response, replyMarkup: InlineKeyboards.BaseMenuKeyboard());
                    return;
                }
            }
        }

        private async Task HandleMessage(ITelegramBotClient botClient, CancellationToken cancellationToken, Update update)
        {
            var username = update.Message.Chat.Username;
            var messageText = update.Message.Text.Trim();
            var chatId = update.Message.Chat.Id;
            string response = "Invalid command";

            if (messageText.StartsWith("/menu", true, CultureInfo.CurrentCulture))
            {
                UpdateUserRequests(chatId);
                await _botClient.SendTextMessageAsync(chatId: chatId, text: "Choose an action", replyMarkup: InlineKeyboards.BaseMenuKeyboard());
                return;
            }
            if (_userRequests[chatId].BaseMenuSection == BaseMenu.RProcess)
            {
                if (Regex.Match(messageText, "Name: .+").Success)
                {
                    var name = messageText.Split(' ')[1];
                    _userRequests[chatId].RProcess = new RProcess
                    {
                        ProcessName = name
                    };
                    await _botClient.SendTextMessageAsync(chatId: chatId,
                        text: "Success!\nSend time boundaries in a format:\n\"Boundaries: 00:00:00-00:00:00\"");
                    return;
                }
                else if (Regex.Match(messageText, "Boundaries: (0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]-(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]").Success)
                {
                    var boundaries = messageText.Split(' ')[1].Split('-');
                    _userRequests[chatId].RProcess.BlockStartTime = TimeOnly.Parse(boundaries[0]);
                    _userRequests[chatId].RProcess.BlockStartTime = TimeOnly.Parse(boundaries[1]);

                    var result = await _mediator.Send(new Application.RProcesses.Add.Command { Process = _userRequests[chatId].RProcess });

                    if (!result.IsSuccess && result.Error != null)
                    {
                        await _botClient.SendTextMessageAsync(chatId: chatId,
                        text: $"{result.Error}.\nSend time boundaries in a format:\n\"Boundaries: 00:00:00-00:00:00\"");
                        return;
                    }

                    _userRequests[chatId].RProcess = null;

                    await _botClient.SendTextMessageAsync(chatId: chatId,
                        text: "Success!\nChanges are applied right away, you don't need to restart blocker",
                        replyMarkup: InlineKeyboards.RProcessesMenuKeyboard());
                    return;
                }
                await _botClient.SendTextMessageAsync(chatId: chatId,
                        text: "Failed to add data.\nTry again according to the provided formats",
                        replyMarkup: InlineKeyboards.RProcessesMenuKeyboard());
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

        private void UpdateUserRequests(long chatId, string baseMenuSection = null, int step = 0, string item = null, List<string> items = null)
        {
            if (_userRequests.ContainsKey(chatId))
            {
                _userRequests[chatId].Step = step;
                _userRequests[chatId].BaseMenuSection = baseMenuSection;
                _userRequests[chatId].Item = item;
                _userRequests[chatId].Items = items;
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