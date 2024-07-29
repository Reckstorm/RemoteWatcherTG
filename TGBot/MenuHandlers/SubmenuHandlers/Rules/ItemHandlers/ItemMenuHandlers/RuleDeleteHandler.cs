using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.MessageContentHandlers;
using TGBot.Menu;
using TGBot.Models;

namespace TGBot.MenuHandlers.SubmenuHandlers.Rules.ItemHandlers.ItemMenuHandlers
{
    public static class RuleDeleteHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;

            if (callBackData == Confirmation.Yes)
            {
                response = "Rule have been deleted successfully";
                await MessageContentHandler.HandleFinalRequest(botclient, update, await mediator.Send(new Application.Rules.Delete.Command() { ProcessName = userRequest.Item }), response, cancellationToken);
                await MessageContentHandler.HandleListRequest(botclient, update, userRequest, await mediator.Send(new Application.Rules.List.Query()), CommonItems.BackToRules, cancellationToken);
                userRequest.ItemMenu = "";
                return;
            }
            
            if (callBackData == Confirmation.No)
            {
                await MessageContentHandler.HandleListRequest(botclient, update, userRequest, await mediator.Send(new Application.Rules.List.Query()), CommonItems.BackToRules, cancellationToken);
                userRequest.ItemMenu = "";
                return;
            }
        }
    }
}