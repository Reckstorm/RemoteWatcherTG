using System.Globalization;
using System.Text.RegularExpressions;
using Application.DTOs;
using Domain;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TGBot.Menu;
using TGBot.Models;

namespace TGBot.Services
{
    public class BotService : BackgroundService
    {
        private readonly IMediator _mediator;
        private readonly TelegramBotClient _botClient;
        private readonly ReceiverOptions _receiverOptions;
        private readonly UserRequest _userRequest;
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
            _userRequest = new UserRequest();
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
            if (_userRequest.Menu == CommonItems.BackToMain)
            {
                if (callBackData == MainMenu.Rule)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                    _userRequest.Menu = MainMenu.Rule;
                    return;
                }

                if (callBackData == MainMenu.Process)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.ProcessesMenuKeyboard(), response, cancellationToken);
                    _userRequest.Menu = MainMenu.Process;
                    return;
                }

                if (callBackData == MainMenu.Logic)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.LogicKeyboard(), response, cancellationToken);
                    _userRequest.Menu = MainMenu.Logic;
                    return;
                }
            }

            //Logic menu actions
            if (_userRequest.Menu == MainMenu.Logic)
            {
                if (callBackData == Logic.Start)
                {
                    response = "Blocker logic has been started successfully";
                    await HandleFinalRequest(botClient, update, await _mediator.Send(new Application.Logic.Start.Command()), response, cancellationToken);
                    return;
                }

                if (callBackData == Logic.Stop)
                {
                    response = "Blocker logic has been stopped successfully";
                    await HandleFinalRequest(botClient, update, await _mediator.Send(new Application.Logic.Stop.Command()), response, cancellationToken);
                    return;
                }

                if (callBackData == CommonItems.BackToMain)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.MainMenuKeyboard(), response, cancellationToken);
                    _userRequest.Menu = CommonItems.BackToMain;
                    return;
                }
            }

            //Processes menu actions
            if (_userRequest.Menu == MainMenu.Process)
            {
                //Send a list of processes
                if (callBackData == Processes.List)
                {
                    await HandleListRequest(botClient, update, await _mediator.Send(new Application.Processes.List.Query()), CommonItems.BackToProcesses, cancellationToken);
                    _userRequest.SubMenu = Processes.List;
                    return;
                }

                //Handle requests to the items list
                if (_userRequest.SubMenu == Processes.List)
                {
                    //Process menu actions
                    if (_userRequest.Items.Any(x => x.ProcessName.Equals(callBackData)))
                    {
                        response = "You can either kill this process with the relevant button below or go back";
                        await HandleDetailsRequest(botClient, update, await _mediator.Send(new Application.Processes.Details.Query { ProcessName = callBackData }), InlineKeyboards.ProcessMenuKeyboard(), response, cancellationToken);
                        return;
                    }

                    //Handle requests about selected item
                    if (_userRequest.Item != "")
                    {
                        //Specific process actions
                        if (callBackData == Process.Kill)
                        {
                            await HandleFinalRequest(botClient, update, await _mediator.Send(new Application.Processes.Kill.Command { ProcessName = _userRequest.Item }), "Process successfully killed", cancellationToken);
                            return;
                        }

                        //Add a rule
                        if (callBackData == Process.Add)
                        {
                            response = $"Current name: <b>{_userRequest.Item}</b>\nSelect blocker start time";
                            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: await InlineKeyboards.ListKeyboard(CommonItems.BackToDetails), parseMode: ParseMode.Html);
                            _userRequest.ItemMenu = Process.Add;
                            return;
                        }

                        if (_userRequest.ItemMenu == Process.Add)
                        {
                            if (_userRequest.Boundaries.StartTime == TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                            {
                                response = "Select blocker end time";
                                await HandleStartTimeInput(botClient, update, await InlineKeyboards.ListKeyboard(CommonItems.BackToDetails), response, cancellationToken);
                                return;
                            }

                            if (_userRequest.Boundaries.StartTime != TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                            {
                                _userRequest.Boundaries.EndTime = TimeOnly.Parse(callBackData);

                                var rule = new Domain.Rule()
                                {
                                    ProcessName = _userRequest.Item,
                                    BlockStartTime = _userRequest.Boundaries.StartTime,
                                    BlockEndTime = _userRequest.Boundaries.EndTime
                                };

                                await HandleFinalRequest(_botClient, update, await _mediator.Send(new Application.Rules.Add.Command { Process = rule }), "Success", cancellationToken);
                                response = "Choose an action towards the process";
                                await HandleDetailsRequest(botClient, update, await _mediator.Send(new Application.Processes.Details.Query { ProcessName = rule.ProcessName }), InlineKeyboards.ProcessMenuKeyboard(), response, cancellationToken);
                                _userRequest.Boundaries = new RuleDto();
                                _userRequest.ItemMenu = "";
                                return;
                            }

                            if (callBackData == CommonItems.BackToDetails)
                            {
                                await HandleDetailsRequest(botClient, update, await _mediator.Send(new Application.Processes.Details.Query { ProcessName = _userRequest.Item }), InlineKeyboards.ProcessMenuKeyboard(), response, cancellationToken);
                                _userRequest.Boundaries = new RuleDto();
                                _userRequest.ItemMenu = "";
                                return;
                            }
                        }

                        //Back to Processes list
                        if (callBackData == CommonItems.BackToList)
                        {
                            await HandleListRequest(botClient, update, await _mediator.Send(new Application.Processes.List.Query()), CommonItems.BackToProcesses, cancellationToken);
                            _userRequest.Item = "";
                            return;
                        }
                    }

                    //Refresh the list
                    if (callBackData == CommonItems.Refresh)
                    {
                        await HandleListRequest(botClient, update, await _mediator.Send(new Application.Processes.List.Query()), CommonItems.BackToProcesses, cancellationToken);
                        return;
                    }

                    //Back to Processes menu
                    if (callBackData == CommonItems.BackToProcesses)
                    {
                        await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.ProcessesMenuKeyboard(), response, cancellationToken);
                        _userRequest.SubMenu = "";
                        return;
                    }
                }

                //Back to base menu
                if (callBackData == CommonItems.BackToMain)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.MainMenuKeyboard(), response, cancellationToken);
                    _userRequest.Menu = CommonItems.BackToMain;
                    return;
                }
            }

            //Rules menu actions
            if (_userRequest.Menu == MainMenu.Rule)
            {
                //Add new Rule
                if (callBackData == Rules.Add)
                {
                    response = "Provide a process name in a form of \"Name: ProcessName\"\nWithout quotes!\nIn case you want to cancell simply send /menu to start the flow over";
                    _userRequest.SubMenu = Rules.Add;
                    await HandleMenuRequestWithTextResponse(botClient, update, response, cancellationToken);
                    return;
                }

                //Handle work with Add rules submenu
                if (_userRequest.SubMenu == Rules.Add)
                {
                    if (_userRequest.Item != "")
                    {
                        if (_userRequest.Boundaries.StartTime == TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                        {
                            response = "Select blocker end time";
                            await HandleStartTimeInput(botClient, update, await InlineKeyboards.ListKeyboard(CommonItems.BackToRules), response, cancellationToken);
                            return;
                        }

                        if (_userRequest.Boundaries.StartTime != TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                        {
                            _userRequest.Boundaries.EndTime = TimeOnly.Parse(callBackData);
                            var Rule = new Domain.Rule()
                            {
                                ProcessName = _userRequest.Item,
                                BlockStartTime = _userRequest.Boundaries.StartTime,
                                BlockEndTime = _userRequest.Boundaries.EndTime
                            };
                            await HandleEndTimeInput(botClient, update, await _mediator.Send(new Application.Rules.Add.Command { Process = Rule }), InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                            _userRequest.SubMenu = "";
                            _userRequest.Item = "";
                            _userRequest.Boundaries = new RuleDto();
                            return;
                        }

                        if (callBackData == CommonItems.BackToRules)
                        {
                            await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                            _userRequest.SubMenu = "";
                            _userRequest.Item = "";
                            _userRequest.Boundaries = new RuleDto();
                            return;
                        }
                    }
                }

                //Edit all Rules
                if (callBackData == Rules.EditAll)
                {
                    response = "Select blocker start time";
                    _userRequest.SubMenu = Rules.EditAll;
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: await InlineKeyboards.ListKeyboard(CommonItems.BackToRules));
                    return;
                }

                //Handle work with EditAll rules submenu
                if (_userRequest.SubMenu == Rules.EditAll)
                {
                    if (_userRequest.Boundaries.StartTime == TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                    {
                        response = "Select blocker end time";
                        await HandleStartTimeInput(botClient, update, await InlineKeyboards.ListKeyboard(CommonItems.BackToRules), response, cancellationToken);
                        return;
                    }

                    if (_userRequest.Boundaries.StartTime != TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                    {
                        _userRequest.Boundaries.EndTime = TimeOnly.Parse(callBackData);
                        await HandleEndTimeInput(botClient, update, await _mediator.Send(new Application.Rules.EditAll.Command { Boundaries = _userRequest.Boundaries }), InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                        _userRequest.SubMenu = "";
                        _userRequest.Boundaries = new RuleDto();
                        return;
                    }

                    if (callBackData == CommonItems.BackToRules)
                    {
                        await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                        _userRequest.SubMenu = "";
                        _userRequest.Boundaries = new RuleDto();
                        return;
                    }
                }

                //Delete all Rulees
                if (callBackData == Rules.DeleteAll)
                {
                    response = "Are you sure you want to delete all rules?";
                    _userRequest.SubMenu = Rules.DeleteAll;
                    await HandleConfirmationRequest(botClient, update, response, cancellationToken);
                    return;
                }

                //Handle work with DeleteAll rules submenu
                if (_userRequest.SubMenu == Rules.DeleteAll)
                {
                    if (callBackData == Confirmation.Yes)
                    {
                        response = "All have been rules successfully deleted";
                        await HandleFinalRequest(botClient, update, await _mediator.Send(new Application.Rules.DeleteAll.Command()), response, cancellationToken);
                        await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                        _userRequest.SubMenu = "";
                        return;
                    }
                    if (callBackData == Confirmation.No)
                    {
                        await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                        _userRequest.SubMenu = "";
                        return;
                    }
                }

                //List all Rules
                if (callBackData == Rules.List)
                {
                    await HandleListRequest(botClient, update, await _mediator.Send(new Application.Rules.List.Query()), CommonItems.BackToRules, cancellationToken);
                    _userRequest.SubMenu = Rules.List;
                    return;
                }

                if (_userRequest.SubMenu == Rules.List)
                {
                    //View list item details
                    if (_userRequest.Items.Any(x => x.ProcessName.Equals(callBackData)))
                    {
                        response = "Choose an action towards the process";
                        await HandleDetailsRequest(botClient, update, await _mediator.Send(new Application.Rules.Details.Query { ProcessName = callBackData }), InlineKeyboards.RuleMenuKeyboard(), response, cancellationToken);
                        return;
                    }

                    if (_userRequest.Item != "")
                    {
                        if (callBackData == Menu.Rule.EditName)
                        {
                            response = $"Current name: <b>{_userRequest.Item}</b>\nProvide a new process name in a form of \"Name: ProcessName\"\nWithout quotes!\nIn case you want to cancell simply send /menu to start the flow over";
                            await HandleMenuRequestWithTextResponse(botClient, update, response, cancellationToken);
                            return;
                        }

                        //Edit Rule time boundaries
                        if (callBackData == Menu.Rule.EditTime)
                        {
                            response = $"Current name: <b>{_userRequest.Item}</b>\nSelect new blocker start time";
                            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: await InlineKeyboards.ListKeyboard(CommonItems.BackToDetails), parseMode: ParseMode.Html);
                            _userRequest.ItemMenu = Menu.Rule.EditTime;
                            return;
                        }

                        if (_userRequest.ItemMenu == Menu.Rule.EditTime)
                        {
                            if (_userRequest.Boundaries.StartTime == TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                            {
                                response = "Select blocker end time";
                                await HandleStartTimeInput(botClient, update, await InlineKeyboards.ListKeyboard(CommonItems.BackToDetails), response, cancellationToken);
                                return;
                            }

                            if (_userRequest.Boundaries.StartTime != TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                            {
                                _userRequest.Boundaries.EndTime = TimeOnly.Parse(callBackData);

                                var rule = new Domain.Rule()
                                {
                                    ProcessName = _userRequest.Item,
                                    BlockStartTime = _userRequest.Boundaries.StartTime,
                                    BlockEndTime = _userRequest.Boundaries.EndTime
                                };

                                await HandleFinalRequest(_botClient, update, await _mediator.Send(new Application.Rules.Edit.Command { ProcessName = _userRequest.Item, Process = rule }), "Success", cancellationToken);
                                response = "Choose an action towards the process";
                                await HandleDetailsRequest(botClient, update, await _mediator.Send(new Application.Rules.Details.Query { ProcessName = rule.ProcessName }), InlineKeyboards.RuleMenuKeyboard(), response, cancellationToken);
                                _userRequest.Boundaries = new RuleDto();
                                _userRequest.ItemMenu = "";
                                return;
                            }

                            if (callBackData == CommonItems.BackToDetails)
                            {
                                await HandleDetailsRequest(botClient, update, await _mediator.Send(new Application.Rules.Details.Query { ProcessName = _userRequest.Item }), InlineKeyboards.RuleMenuKeyboard(), response, cancellationToken);
                                _userRequest.Boundaries = new RuleDto();
                                _userRequest.ItemMenu = "";
                                return;
                            }
                        }

                        //Delete rule
                        if (callBackData == Menu.Rule.Delete)
                        {
                            response = $"Are you sure you want to delete rule {_userRequest.Item}?";
                            await HandleConfirmationRequest(botClient, update, response, cancellationToken);
                            _userRequest.ItemMenu = Menu.Rule.Delete;
                            return;
                        }

                        if (_userRequest.ItemMenu == Menu.Rule.Delete)
                        {
                            if (callBackData == Confirmation.Yes)
                            {
                                response = "Rule have been deleted successfully";
                                await HandleFinalRequest(botClient, update, await _mediator.Send(new Application.Rules.Delete.Command() { ProcessName = _userRequest.Item }), response, cancellationToken);
                                await HandleListRequest(botClient, update, await _mediator.Send(new Application.Rules.List.Query()), CommonItems.BackToRules, cancellationToken);
                                _userRequest.ItemMenu = "";
                                return;
                            }
                            if (callBackData == Confirmation.No)
                            {
                                await HandleListRequest(botClient, update, await _mediator.Send(new Application.Rules.List.Query()), CommonItems.BackToRules, cancellationToken);
                                _userRequest.ItemMenu = "";
                                return;
                            }
                        }

                        //Back to list
                        if (callBackData == CommonItems.BackToList)
                        {
                            await HandleListRequest(botClient, update, await _mediator.Send(new Application.Rules.List.Query()), CommonItems.BackToRules, cancellationToken);
                            _userRequest.Item = "";
                            return;
                        }
                    }

                    //Refresh the list
                    if (callBackData == CommonItems.Refresh)
                    {
                        await HandleListRequest(botClient, update, await _mediator.Send(new Application.Rules.List.Query()), CommonItems.BackToRules, cancellationToken);
                        return;
                    }

                    //Back to Rules menu
                    if (callBackData == CommonItems.BackToRules)
                    {
                        await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                        _userRequest.SubMenu = "";
                        return;
                    }
                }

                //Back to main
                if (callBackData == CommonItems.BackToMain)
                {
                    await HandleSimpleMenuRequest(botClient, update, InlineKeyboards.MainMenuKeyboard(), response, cancellationToken);
                    _userRequest.Menu = CommonItems.BackToMain;
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
                _userRequest.Menu = CommonItems.BackToMain;
                await botClient.SendTextMessageAsync(chatId: chatId, text: response, replyMarkup: InlineKeyboards.MainMenuKeyboard(), parseMode: ParseMode.Html);
                return;
            }

            else if (_userRequest.Menu == MainMenu.Rule && _userRequest.Item == "")
            {
                if (CheckName(messageText))
                {
                    response = "Select blocker start time";
                    var name = messageText.Split(' ')[1];
                    _userRequest.Item = name;
                    await botClient.SendTextMessageAsync(chatId: chatId,
                        text: response, replyMarkup: await InlineKeyboards.ListKeyboard(CommonItems.BackToRules));
                    return;
                }
                await botClient.SendTextMessageAsync(chatId: chatId,
                        text: "Failed to add data.\nTry again according to the provided formats");
                return;
            }

            else if (_userRequest.Menu == MainMenu.Rule && _userRequest.Item != "")
            {
                if (CheckName(messageText))
                {
                    var name = messageText.Split(' ')[1];
                    var i = _userRequest.Items.FindIndex(x => x.ProcessName == _userRequest.Item);
                    var updatedRule = new Domain.Rule { ProcessName = name, BlockEndTime = _userRequest.Items[i].EndTime, BlockStartTime = _userRequest.Items[i].StartTime };

                    response = "Process name successfully updated";
                    await HandleFinalRequest(botClient, update, await _mediator.Send(new Application.Rules.Edit.Command { ProcessName = _userRequest.Item, Process = updatedRule }), response, cancellationToken);
                    _userRequest.Items[i].ProcessName = name;
                    _userRequest.Item = name;

                    response = "Choose an action towards the process";
                    await HandleDetailsRequest(botClient, update, await _mediator.Send(new Application.Rules.Details.Query { ProcessName = name }), InlineKeyboards.RuleMenuKeyboard(), response, cancellationToken);
                    return;
                }
                await botClient.SendTextMessageAsync(chatId: chatId,
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
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId,
                text: response, parseMode: ParseMode.Html);
            return;
        }

        private async Task HandleSimpleMenuRequest(ITelegramBotClient botClient, Update update, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: markup);
            return;
        }

        private async Task HandleConfirmationRequest(ITelegramBotClient botClient, Update update, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: InlineKeyboards.ConfirmationKeyboard());
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
            return;
        }

        private async Task HandleListRequest<T>(ITelegramBotClient botClient, Update update, Result<T> result, string back, CancellationToken cancellationToken) where T : List<CommonDto>
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
            
            _userRequest.Items = result.Value;

            await botClient.AnswerCallbackQueryAsync(callbackQueryId);
            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: update.CallbackQuery.Message.MessageId,
                text: "Choose a process:",
                replyMarkup: await InlineKeyboards.ListKeyboard(result.Value, back));
            return;
        }

        private async Task HandleDetailsRequest(ITelegramBotClient botClient, Update update, Result<CommonDto> result, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
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

            _userRequest.Item = TimeMenu.TimeOptions.Any(x => x.Equals(update.CallbackQuery.Data)) || update.CallbackQuery.Data == CommonItems.BackToDetails ?
            _userRequest.Item : update.CallbackQuery.Data;
            return;
        }

        private async Task HandleStartTimeInput(ITelegramBotClient botClient, Update update, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var callBackData = update.CallbackQuery.Data;
            _userRequest.Boundaries.StartTime = TimeOnly.Parse(callBackData);
            await _botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: markup);
            return;
        }

        private async Task HandleEndTimeInput<T>(ITelegramBotClient botClient, Update update, Result<T> result, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            await HandleFinalRequest(_botClient, update, result, "Success", cancellationToken);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: markup);
            return;
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