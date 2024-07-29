using System.Globalization;
using System.Text.RegularExpressions;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBot.KeyboardHandlers;
using TGBot.Menu;
using TGBot.MenuHandlers;
using TGBot.Models;

namespace TGBot.Services
{
    public class BotService : BackgroundService
    {
        private readonly IMediator _mediator;
        private readonly TelegramBotClient _botClient;
        private readonly ReceiverOptions _receiverOptions;
        private UserRequest _userRequest;
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

            _botClient.SendTextMessageAsync(_configuration["BotAdminChatId"], $"Bot started {DateTime.Now.ToShortTimeString()}");
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
                MainMenuHandler.Handle(botClient, update, _userRequest, response, cancellationToken);
            }

            //Logic menu actions
            if (_userRequest.Menu == MainMenu.Logic)
            {
                LogicHandler.Handle(botClient, update, _userRequest, _mediator, response, cancellationToken);
            }

            //Processes menu actions
            if (_userRequest.Menu == MainMenu.Process)
            {
                ProcessesHandler.Handle(botClient, update, _userRequest, _mediator, response, cancellationToken);
            }

            //Rules menu actions
            if (_userRequest.Menu == MainMenu.Rule)
            {
                RulesHandler.Handle(botClient, update, _userRequest, _mediator, response, cancellationToken);
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
                _userRequest = new() { Menu = CommonItems.BackToMain };
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
                    await KeyboardHandler.HandleFinalRequest(botClient, update, await _mediator.Send(new Application.Rules.Edit.Command { ProcessName = _userRequest.Item, Process = updatedRule }), response, cancellationToken);
                    _userRequest.Items[i].ProcessName = name;
                    _userRequest.Item = name;

                    response = "Choose an action towards the process";
                    await KeyboardHandler.HandleDetailsRequest(botClient, update, _userRequest, await _mediator.Send(new Application.Rules.Details.Query { ProcessName = name }), InlineKeyboards.RuleMenuKeyboard(), response, cancellationToken);
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