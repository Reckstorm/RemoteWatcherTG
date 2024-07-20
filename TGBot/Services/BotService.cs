using System.Globalization;
using System.Text.RegularExpressions;
using Application.DTOs;
using Application.RProcesses;
using Domain;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TGBot.Menus;

namespace TGBot.Services
{
    public class BotService : BackgroundService
    {
        private readonly IMediator _mediator;
        private readonly TelegramBotClient _botClient;
        private readonly ReceiverOptions _receiverOptions;
        private readonly Dictionary<long, UserRequest> _userRequests;
        public IConfiguration _configuration { get; }

        public BotService(IMediator mediator, IConfiguration configuration)
        {
            _configuration = configuration;
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
            string response = "Invalid command";

            if (callBackData == null)
            {
                await botClient.SendTextMessageAsync(chatId, response);
                return;
            }

            response = "Choose an action";

            //Base menu actions
            if (_userRequests[chatId].BaseMenuSection == CommonMenuItems.BackToBase)
            {
                if (callBackData == BaseMenu.RProcess)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.RProcessesMenuKeyboard(), response, cancellationToken);
                    return;
                }

                if (callBackData == BaseMenu.Process)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.ProcessesMenuKeyboard(), response, cancellationToken);
                    return;
                }

                if (callBackData == BaseMenu.Logic)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.LogicMenuKeyboard(), response, cancellationToken);
                    return;
                }
            }

            //Logic menu actions
            if (_userRequests[chatId].BaseMenuSection == BaseMenu.Logic)
            {
                if (callBackData == LogicMenu.Start)
                {
                    await HandleFinalRequest(botClient, update, await _mediator.Send(new Application.Logic.Start.Command()), "Blocker logic has been started successfully", cancellationToken);
                    return;
                }

                if (callBackData == LogicMenu.Stop)
                {
                    await HandleFinalRequest(botClient, update, await _mediator.Send(new Application.Logic.Stop.Command()), "Blocker logic has been stopped successfully", cancellationToken);
                    return;
                }

                if (callBackData == CommonMenuItems.BackToBase)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.BaseMenuKeyboard(), response, cancellationToken);
                    return;
                }
            }

            //Processes menu actions
            if (_userRequests[chatId].BaseMenuSection == BaseMenu.Process)
            {
                //Send a list of processes
                if (callBackData == ProcessesMenu.List)
                {
                    await HandleListRequest(botClient, update, await _mediator.Send(new Application.Processes.List.Query()), cancellationToken);
                    return;
                }

                //Back to base menu
                if (callBackData == CommonMenuItems.BackToBase)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.BaseMenuKeyboard(), response, cancellationToken);
                    return;
                }

                //Process menu actions
                if (_userRequests[chatId].Items != null && _userRequests[chatId].Items.Any(x => x.ProcessName.Equals(callBackData)))
                {
                    await HandleDetailsRequest(botClient, update, await _mediator.Send(new Application.Processes.Details.Query { ProcessName = callBackData }),
                    InlineKeyboards.ProcessMenuKeyboard(),
                    "You can either kill this process with the relevant button below or go back", cancellationToken);
                    return;
                }

                //Back to Processes menu
                if (callBackData == CommonMenuItems.BackToProcesses)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.ProcessesMenuKeyboard(), response, cancellationToken);
                    return;
                }

                //Specific process actions
                if (callBackData == ProcessMenu.Kill)
                {
                    await HandleFinalRequest(botClient, update, await _mediator.Send(new Application.Processes.Kill.Command { ProcessName = _userRequests[chatId].Item }), "Process successfully killed", cancellationToken);
                    return;
                }

                //Back to Processes list
                if (callBackData == CommonMenuItems.BackToList)
                {
                    await HandleListRequest(botClient, update, await _mediator.Send(new Application.Processes.List.Query()), cancellationToken);
                    return;
                }
            }

            //RProcesses menu actions
            if (_userRequests[chatId].BaseMenuSection == BaseMenu.RProcess)
            {
                if (callBackData == RProcessesMenu.Add)
                {
                    response = "Provide a process name in a form of \"Name: ProcessName\"\nWithout quotes!\nIn case you want to cancell simply send /menu to start the flow over";
                    await HandleMenuRequestWithTextResponse(botClient, update, response, cancellationToken);
                    return;
                }

                if (_userRequests[chatId].RProcess != null &&
                    _userRequests[chatId].RProcess.BlockStartTime == TimeOnly.MaxValue &&
                    TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                {
                    response = "Select blocker end time";
                    _userRequests[chatId].RProcess.BlockStartTime = TimeOnly.Parse(callBackData);
                    await _botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                    await _botClient.SendTextMessageAsync(chatId: chatId, text: response, replyMarkup: await InlineKeyboards.ListKeyboard());
                    return;
                }

                if (_userRequests[chatId].RProcess.BlockStartTime != TimeOnly.MaxValue &&
                    TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                {
                    response = "New rule successfully added";
                    _userRequests[chatId].RProcess.BlockEndTime = TimeOnly.Parse(callBackData);
                    await HandleFinalRequest(_botClient, update, await _mediator.Send(new Application.RProcesses.Add.Command { Process = _userRequests[chatId].RProcess }),
                        response, cancellationToken);
                    return;
                }

                if (callBackData == RProcessesMenu.EditAll)
                {
                    response = "Send time boundaries that are to be set for all of the rules in a format:\n\"Boundaries: 00:00:00-00:00:00\"\nIn case you want to cancell simply send /menu to start the flow over";
                    await HandleMenuRequestWithTextResponse(botClient, update, response, cancellationToken);
                    return;
                }

                if (callBackData == RProcessesMenu.DeleteAll)
                {
                    await HandleFinalRequest(botClient, update, await _mediator.Send(new DeleteAll.Command()), "All have been rules successfully deleted", cancellationToken);
                    return;
                }

                if (callBackData == CommonMenuItems.BackToBase)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.BaseMenuKeyboard(), response, cancellationToken);
                    return;
                }

                if (callBackData == RProcessesMenu.List)
                {
                    await HandleListRequest(botClient, update, await _mediator.Send(new List.Query()), cancellationToken);
                    return;
                }

                if (_userRequests[chatId].Items != null && _userRequests[chatId].Items.Any(x => x.ProcessName.Equals(callBackData)))
                {
                    await HandleDetailsRequest(botClient, update, await _mediator.Send(new Application.RProcesses.Details.Query { ProcessName = callBackData }),
                    InlineKeyboards.RProcessMenuKeyboard(),
                    "Choose an action towards the process", cancellationToken);
                    return;
                }

                //Back to RProcesses menu
                if (callBackData == CommonMenuItems.BackToProcesses)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.RProcessesMenuKeyboard(), response, cancellationToken);
                    return;
                }


                // if (callBackData == RProcessMenu.EditName)
                // {
                //     response = "Provide a process name in a form of \"Name: ProcessName\"\nWithout quotes!\nIn case you want to cancell simply send /menu to start the flow over";
                //     await HandleMenuRequestWithTextResponse(botClient, update, response, cancellationToken);
                //     return;
                // }

                if (callBackData == RProcessMenu.Delete)
                {
                    await HandleFinalRequest(botClient, update, await _mediator.Send(new Delete.Command() { ProcessName = _userRequests[chatId].Item }), "Process have been removed successfully", cancellationToken);
                    return;
                }

                if (callBackData == CommonMenuItems.BackToList)
                {
                    await HandleListRequest(botClient, update, await _mediator.Send(new Application.RProcesses.List.Query()), cancellationToken);
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

            if (username == _configuration["BotAdminUsername"] && messageText.StartsWith("/menu", true, CultureInfo.CurrentCulture))
            {
                response = "Choose an action";
                UpdateUserRequests(chatId, baseMenuSection: CommonMenuItems.BackToBase);
                await _botClient.SendTextMessageAsync(chatId: chatId, text: response, replyMarkup: InlineKeyboards.BaseMenuKeyboard());
                return;
            }
            else if (_userRequests[chatId].BaseMenuSection == BaseMenu.RProcess)
            {
                if (CheckName(messageText))
                {
                    response = "Select blocker start time";
                    var name = messageText.Split(' ')[1];
                    _userRequests[chatId].RProcess = new RProcess
                    {
                        ProcessName = name
                    };
                    await _botClient.SendTextMessageAsync(chatId: chatId,
                        text: response, replyMarkup: await InlineKeyboards.ListKeyboard());
                    return;
                }
                // else if (CheckBoundaries(messageText) && _userRequests[chatId].RProcess != null)
                // {
                //     var boundaries = messageText.Split(' ')[1].Split('-');
                //     _userRequests[chatId].RProcess.BlockStartTime = TimeOnly.Parse(boundaries[0]);
                //     _userRequests[chatId].RProcess.BlockStartTime = TimeOnly.Parse(boundaries[1]);

                //     var result = await _mediator.Send(new Add.Command { Process = _userRequests[chatId].RProcess });

                //     if (!result.IsSuccess && result.Error != null)
                //     {
                //         await _botClient.SendTextMessageAsync(chatId: chatId,
                //         text: $"{result.Error}.");
                //         return;
                //     }

                //     _userRequests[chatId].RProcess = null;

                //     await _botClient.SendTextMessageAsync(chatId: chatId,
                //         text: "Success!\nChanges are applied right away, you don't need to restart blocker if it is running");
                //     return;
                // }
                // else if (CheckBoundaries(messageText) && _userRequests[chatId].RProcess == null)
                // {
                //     var boundaries = messageText.Split(' ')[1].Split('-');
                //     var start = TimeOnly.Parse(boundaries[0]);
                //     var end = TimeOnly.Parse(boundaries[1]);

                //     var result = await _mediator.Send(new EditAll.Command { Boundaries = new RProcessDTO { StartTime = start, EndTime = end } });

                //     if (result == null)
                //     {
                //         await _botClient.SendTextMessageAsync(chatId: chatId,
                //         text: $"Error.\nSend time boundaries in a format:\n\"Boundaries: 00:00:00-00:00:00\"");
                //         return;
                //     }
                //     if (!result.IsSuccess && result.Error != null)
                //     {
                //         await _botClient.SendTextMessageAsync(chatId: chatId,
                //         text: $"{result.Error}.\nSend time boundaries in a format:\n\"Boundaries: 00:00:00-00:00:00\"");
                //         return;
                //     }

                //     await _botClient.SendTextMessageAsync(chatId: chatId,
                //         text: "Success!\nChanges are applied right away, you don't need to restart blocker if it is running",
                //         replyMarkup: InlineKeyboards.RProcessesMenuKeyboard());
                //     return;
                // }
                await _botClient.SendTextMessageAsync(chatId: chatId,
                        text: "Failed to add data.\nTry again according to the provided formats");
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

        private bool CheckBoundaries(string text)
        {
            return Regex.Match(text, "Boundaries: (0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]-(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]").Success;
        }

        private bool CheckName(string text)
        {
            return Regex.Match(text, "Name: .+").Success;
        }

        private async Task HandleMenuRequestWithTextResponse(ITelegramBotClient botClient, Update update, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            UpdateUserRequests(chatId: chatId, baseMenuSection: BaseMenu.RProcess);
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.SendTextMessageAsync(chatId: chatId,
                text: response);
            return;
        }

        private async Task HandleSimpleMenuRequest(ITelegramBotClient botClient, Update update, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            UpdateUserRequests(chatId: chatId, baseMenuSection: update.CallbackQuery.Data);
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: markup);
            return;
        }

        private async Task HandleFinalRequest<T>(ITelegramBotClient botClient, Update update, Result<T> result, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var callbackQueryId = update.CallbackQuery.Id;
            if (result == null)
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Failed to process command");
                return;
            }
            if (!result.IsSuccess && result.Error != null)
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, result.Error);
                return;
            }
            UpdateUserRequests(chatId: chatId,
                baseMenuSection: _userRequests[chatId].BaseMenuSection);
            await botClient.AnswerCallbackQueryAsync(callbackQueryId, response);
            return;
        }

        private async Task HandleListRequest<T>(ITelegramBotClient botClient, Update update, Result<T> result, CancellationToken cancellationToken) where T : List<CommonProcessDto>
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var callbackQueryId = update.CallbackQuery.Id;
            if (result == null)
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Failed to process command");
                return;
            }
            if (!result.IsSuccess && result.Error != null)
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, result.Error);
                return;
            }

            UpdateUserRequests(
                chatId: chatId,
                baseMenuSection: _userRequests[chatId].BaseMenuSection,
                items: result.Value);
            await botClient.AnswerCallbackQueryAsync(callbackQueryId);
            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: update.CallbackQuery.Message.MessageId,
                text: "Choose a process:",
                replyMarkup: await InlineKeyboards.ListKeyboard(result.Value));
            return;
        }

        private async Task HandleDetailsRequest(ITelegramBotClient botClient, Update update, Result<CommonProcessDto> result, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var callbackQueryId = update.CallbackQuery.Id;
            if (result == null)
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Failed to process command");
                return;
            }
            if (!result.IsSuccess && result.Error != null)
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, result.Error);
                return;
            }

            UpdateUserRequests(
                chatId,
                baseMenuSection: _userRequests[chatId].BaseMenuSection,
                item: update.CallbackQuery.Data,
                items: _userRequests[chatId].Items);

            await botClient.AnswerCallbackQueryAsync(callbackQueryId);

            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: update.CallbackQuery.Message.MessageId,
                text: $"Currently chosen process:\nName: {result.Value.ProcessName}\n{response}",
                replyMarkup: markup);
            return;
        }

        private void UpdateUserRequests(long chatId, string baseMenuSection = null, string item = null, List<CommonProcessDto> items = null)
        {
            if (_userRequests.ContainsKey(chatId))
            {
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