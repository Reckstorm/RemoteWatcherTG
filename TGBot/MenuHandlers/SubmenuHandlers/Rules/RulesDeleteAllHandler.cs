using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.MessageContentHandlers;
using TGBot.Menu;
using TGBot.Models;

namespace TGBot.MenuHandlers.Rules.SubmenuHandlers
{
    public static class RulesDeleteAllHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;

            if (callBackData == Confirmation.Yes)
            {
                response = "All have been rules successfully deleted";
                await MessageContentHandler.HandleFinalRequest(botclient, update, await mediator.Send(new Application.Rules.DeleteAll.Command()), response, cancellationToken);
                await MessageContentHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                userRequest.SubMenu = "";
                return;
            }
            
            if (callBackData == Confirmation.No)
            {
                await MessageContentHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                userRequest.SubMenu = "";
                return;
            }
        }
    }
}