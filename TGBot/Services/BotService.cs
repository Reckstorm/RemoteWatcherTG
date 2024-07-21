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
                    response = "Blocker logic has been started successfully";
                    await HandleFinalRequest(botClient, update, await _mediator.Send(new Application.Logic.Start.Command()), response, cancellationToken);
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
                //Add new rProcess
                if (callBackData == RProcessesMenu.Add)
                {
                    response = "Provide a process name in a form of \"Name: ProcessName\"\nWithout quotes!\nIn case you want to cancell simply send /menu to start the flow over";
                    await HandleMenuRequestWithTextResponse(botClient, update, response, cancellationToken);
                    return;
                }

                if (_userRequests[chatId].Item != null &&
                    _userRequests[chatId].Boundaries.StartTime == TimeOnly.MaxValue &&
                    !_userRequests[chatId].Items.Any(x => x.ProcessName.Equals(_userRequests[chatId].Item)) &&
                    TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                {
                    response = "Select blocker end time";
                    await HandleStartTimeInput(botClient, update, await InlineKeyboards.ListKeyboard(), response, cancellationToken);
                    return;
                }

                if (_userRequests[chatId].Item != null &&
                    _userRequests[chatId].Boundaries.StartTime != TimeOnly.MaxValue &&
                    !_userRequests[chatId].Items.Any(x => x.ProcessName.Equals(_userRequests[chatId].Item)) &&
                    TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                {
                    _userRequests[chatId].Boundaries.EndTime = TimeOnly.Parse(callBackData);
                    var rProcess = new RProcess()
                    {
                        ProcessName = _userRequests[chatId].Item,
                        BlockStartTime = _userRequests[chatId].Boundaries.StartTime,
                        BlockEndTime = _userRequests[chatId].Boundaries.EndTime
                    };
                    await HandleEndTimeInput(botClient, update, await _mediator.Send(new Add.Command { Process = rProcess }), InlineKeyboards.RProcessesMenuKeyboard(), response, cancellationToken);
                    return;
                }

                //Edit all rProcesses
                if (callBackData == RProcessesMenu.EditAll)
                {
                    response = "Select blocker start time";
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: await InlineKeyboards.ListKeyboard());
                    return;
                }

                if (_userRequests[chatId].Item == null &&
                    _userRequests[chatId].Boundaries.StartTime == TimeOnly.MaxValue &&
                    TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                {
                    response = "Select blocker end time";
                    await HandleStartTimeInput(botClient, update, await InlineKeyboards.ListKeyboard(), response, cancellationToken);
                    return;
                }

                if (_userRequests[chatId].Item == null &&
                    _userRequests[chatId].Boundaries.StartTime != TimeOnly.MaxValue &&
                    TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                {
                    _userRequests[chatId].Boundaries.EndTime = TimeOnly.Parse(callBackData);
                    await HandleEndTimeInput(botClient, update, await _mediator.Send(new EditAll.Command { Boundaries = _userRequests[chatId].Boundaries }), InlineKeyboards.RProcessesMenuKeyboard(), response, cancellationToken);
                    return;
                }

                //Delete all rProcesses
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

                //List all rProcesses
                if (callBackData == RProcessesMenu.List)
                {
                    await HandleListRequest(botClient, update, await _mediator.Send(new List.Query()), cancellationToken);
                    return;
                }

                //View list item details
                if (_userRequests[chatId].Items != null &&
                    _userRequests[chatId].Items.Any(x => x.ProcessName.Equals(callBackData)))
                {
                    response = "Choose an action towards the process";
                    await HandleDetailsRequest(botClient, update, await _mediator.Send(new Details.Query { ProcessName = callBackData }), InlineKeyboards.RProcessMenuKeyboard(), response, cancellationToken);
                    return;
                }

                //Back to RProcesses menu
                if (callBackData == CommonMenuItems.BackToProcesses)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.RProcessesMenuKeyboard(), response, cancellationToken);
                    return;
                }

                if (callBackData == RProcessMenu.EditName)
                {
                    response = $"Current name: <b>{_userRequests[chatId].Item}</b>\nProvide a new process name in a form of \"Name: ProcessName\"\nWithout quotes!\nIn case you want to cancell simply send /menu to start the flow over";
                    await HandleMenuRequestWithTextResponse(botClient, update, response, cancellationToken);
                    return;
                }


                //Edit RProcess time boundaries
                if (callBackData == RProcessMenu.EditTime)
                {
                    response = $"Current name: <b>{_userRequests[chatId].Item}</b>\nSelect new blocker start time";
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: await InlineKeyboards.ListKeyboard(), parseMode: ParseMode.Html);
                    return;
                }

                if (_userRequests[chatId].Item != null &&
                    _userRequests[chatId].Boundaries.StartTime == TimeOnly.MaxValue &&
                    _userRequests[chatId].Items.Any(x => x.ProcessName.Equals(_userRequests[chatId].Item)) &&
                    TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                {
                    response = "Select blocker end time";
                    await HandleStartTimeInput(botClient, update, await InlineKeyboards.ListKeyboard(), response, cancellationToken);
                    return;
                }

                if (_userRequests[chatId].Item != null &&
                    _userRequests[chatId].Boundaries.StartTime != TimeOnly.MaxValue &&
                    _userRequests[chatId].Items.Any(x => x.ProcessName.Equals(_userRequests[chatId].Item)) &&
                    TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                {
                    _userRequests[chatId].Boundaries.EndTime = TimeOnly.Parse(callBackData);
                    var rProcess = new RProcess()
                    {
                        ProcessName = _userRequests[chatId].Item,
                        BlockStartTime = _userRequests[chatId].Boundaries.StartTime,
                        BlockEndTime = _userRequests[chatId].Boundaries.EndTime
                    };

                    await HandleFinalRequest(_botClient, update, await _mediator.Send(new Edit.Command { ProcessName = _userRequests[chatId].Item, Process = rProcess }), "Success", cancellationToken);
                    response = "Choose an action towards the process";
                    await HandleDetailsRequest(botClient, update, await _mediator.Send(new Details.Query { ProcessName = rProcess.ProcessName }), InlineKeyboards.RProcessMenuKeyboard(), response, cancellationToken);
                    return;
                }

                if (callBackData == RProcessMenu.Delete)
                {
                    response = "Process have been removed successfully";
                    await HandleFinalRequest(botClient, update, await _mediator.Send(new Delete.Command() { ProcessName = _userRequests[chatId].Item }), response, cancellationToken);
                    await HandleListRequest(botClient, update, await _mediator.Send(new List.Query()), cancellationToken);
                    return;
                }

                if (callBackData == CommonMenuItems.BackToList)
                {
                    await HandleListRequest(botClient, update, await _mediator.Send(new List.Query()), cancellationToken);
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

            if (username != _configuration["BotAdminUsername"]) return;

            if (messageText.StartsWith("/menu", true, CultureInfo.CurrentCulture))
            {
                response = "Choose an action";
                UpdateUserRequests(chatId, baseMenuSection: CommonMenuItems.BackToBase);
                await _botClient.SendTextMessageAsync(chatId: chatId, text: response, replyMarkup: InlineKeyboards.BaseMenuKeyboard(), parseMode: ParseMode.Html);
                return;
            }

            else if (_userRequests[chatId].BaseMenuSection == BaseMenu.RProcess && _userRequests[chatId].Item == null)
            {
                if (CheckName(messageText))
                {
                    response = "Select blocker start time";
                    var name = messageText.Split(' ')[1];
                    _userRequests[chatId].Item = name;
                    await _botClient.SendTextMessageAsync(chatId: chatId,
                        text: response, replyMarkup: await InlineKeyboards.ListKeyboard());
                    return;
                }
                await _botClient.SendTextMessageAsync(chatId: chatId,
                        text: "Failed to add data.\nTry again according to the provided formats");
                return;
            }

            else if (_userRequests[chatId].BaseMenuSection == BaseMenu.RProcess && _userRequests[chatId].Item != null)
            {
                if (CheckName(messageText))
                {
                    var name = messageText.Split(' ')[1];
                    var temp = _userRequests[chatId].Items.FirstOrDefault(x => x.ProcessName == _userRequests[chatId].Item);
                    var updatedRprocess = new RProcess { ProcessName = name, BlockEndTime = temp.EndTime, BlockStartTime = temp.StartTime };

                    response = "Process name successfully updated";
                    await HandleFinalRequest(botClient, update, await _mediator.Send(new Edit.Command { ProcessName = _userRequests[chatId].Item, Process = updatedRprocess }), response, cancellationToken);

                    response = "Choose an action towards the process";
                    await HandleDetailsRequest(botClient, update, await _mediator.Send(new Details.Query { ProcessName = name }), InlineKeyboards.RProcessMenuKeyboard(), response, cancellationToken);
                    return;
                }
                await _botClient.SendTextMessageAsync(chatId: chatId,
                        text: "Failed to update name.\nTry again according to the provided formats");
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

        private bool CheckName(string text)
        {
            return Regex.Match(text, "Name: .+").Success;
        }

        private async Task HandleMenuRequestWithTextResponse(ITelegramBotClient botClient, Update update, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var tempUserRequest = _userRequests[chatId];
            UpdateUserRequests(chatId: chatId, baseMenuSection: tempUserRequest.BaseMenuSection, item: tempUserRequest.Item, items: tempUserRequest.Items);
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId,
                text: response, parseMode: ParseMode.Html);
            return;
        }

        private async Task HandleSimpleMenuRequest(ITelegramBotClient botClient, Update update, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;

            string baseMenuSection;
            if (_userRequests[chatId].BaseMenuSection == CommonMenuItems.BackToBase || update.CallbackQuery.Data == CommonMenuItems.BackToBase) baseMenuSection = update.CallbackQuery.Data;
            else baseMenuSection = _userRequests[chatId].BaseMenuSection;

            UpdateUserRequests(chatId: chatId, baseMenuSection: baseMenuSection);
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: markup);
            return;
        }

        private async Task HandleFinalRequest<T>(ITelegramBotClient botClient, Update update, Result<T> result, string response, CancellationToken cancellationToken)
        {
            long chatId;
            if (update.Type == UpdateType.Message)
            {
                chatId = update.Message.Chat.Id;
                if (result == null)
                {
                    await botClient.SendTextMessageAsync(chatId, "Failed to process command");
                    return;
                }
                if (!result.IsSuccess && result.Error != null)
                {
                    await botClient.SendTextMessageAsync(chatId, result.Error);
                    return;
                }

                await botClient.SendTextMessageAsync(chatId, response);
            }
            else
            {
                chatId = update.CallbackQuery.Message.Chat.Id;
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
                await botClient.AnswerCallbackQueryAsync(callbackQueryId, response);
            }
            UpdateUserRequests(chatId: chatId, baseMenuSection: _userRequests[chatId].BaseMenuSection);
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
            long chatId;
            if (update.Type == UpdateType.Message)
            {
                chatId = update.Message.Chat.Id;
                if (result == null)
                {
                    await botClient.SendTextMessageAsync(chatId, "Failed to process command");
                    return;
                }
                if (!result.IsSuccess && result.Error != null)
                {
                    await botClient.SendTextMessageAsync(chatId, result.Error);
                    return;
                }
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Currently chosen process:\nName: <b>{result.Value.ProcessName}</b>\nProcess is blocked from <b>{result.Value.StartTime}</b> to <b>{result.Value.EndTime}</b>\n{response}",
                    replyMarkup: markup, parseMode: ParseMode.Html);
            }
            else
            {
                chatId = update.CallbackQuery.Message.Chat.Id;
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
                await botClient.AnswerCallbackQueryAsync(callbackQueryId);

                await botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: update.CallbackQuery.Message.MessageId,
                    text: $"Currently chosen process:\nName: <b>{result.Value.ProcessName}</b>\nProcess is blocked from <b>{result.Value.StartTime}</b> to <b>{result.Value.EndTime}</b>\n{response}",
                    replyMarkup: markup, parseMode: ParseMode.Html);
            }

            UpdateUserRequests(
                chatId,
                baseMenuSection: _userRequests[chatId].BaseMenuSection,
                item: update.CallbackQuery.Data,
                items: _userRequests[chatId].Items);
            return;
        }

        private async Task HandleStartTimeInput(ITelegramBotClient botClient, Update update, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var callBackData = update.CallbackQuery.Data;

            _userRequests[chatId].Boundaries.StartTime = TimeOnly.Parse(callBackData);
            await _botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: markup);
            return;
        }

        private async Task HandleEndTimeInput<T>(ITelegramBotClient botClient, Update update, Result<T> result, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            await HandleFinalRequest(_botClient, update, result, "Success", cancellationToken);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: markup);
            UpdateUserRequests(chatId: chatId, baseMenuSection: _userRequests[chatId].BaseMenuSection, item: _userRequests[chatId].Item, items: _userRequests[chatId].Items);
            return;
        }

        private void UpdateUserRequests(long chatId, string baseMenuSection = null, string item = null, List<CommonProcessDto> items = null, RProcessDTO boundaries = null)
        {
            if (_userRequests.ContainsKey(chatId))
            {
                _userRequests[chatId].BaseMenuSection = baseMenuSection;
                _userRequests[chatId].Item = item;
                _userRequests[chatId].Items = items == null ? new List<CommonProcessDto>() : items;
                _userRequests[chatId].Boundaries = boundaries == null ? new RProcessDTO() : boundaries;
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