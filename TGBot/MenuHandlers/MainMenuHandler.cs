using Application.Logic;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.MessageContentHandlers;
using TGBot.Menu;
using TGBot.Models;

namespace TGBot.MenuHandlers
{
    public static class MainMenuHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, IMediator mediator, UserRequest userRequest, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;

            if (callBackData == MainMenu.Rule)
            {
                await MessageContentHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                userRequest.Menu = MainMenu.Rule;
                return;
            }

            if (callBackData == MainMenu.Process)
            {
                await MessageContentHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.ProcessesMenuKeyboard(), response, cancellationToken);
                userRequest.Menu = MainMenu.Process;
                return;
            }

            if (callBackData == MainMenu.Logic)
            {
                await MessageContentHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.LogicKeyboard(), await MessageContentHandler.HandleLogicStatusRequest(mediator, response), cancellationToken);
                userRequest.Menu = MainMenu.Logic;
                return;
            }
        }
    }
}