using Application.DTOs;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.KeyboardHandlers;
using TGBot.Menu;
using TGBot.MenuHandlers.SubmenuHandlers.Rules.ItemHandlers;
using TGBot.Models;

namespace TGBot.MenuHandlers.Rules.SubmenuHandlers
{
    public static class RulesListHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;

            //View list item details
            if (userRequest.Items.Any(x => x.ProcessName.Equals(callBackData)))
            {
                response = "Choose an action towards the process";
                await KeyboardHandler.HandleDetailsRequest(botclient, update, userRequest, await mediator.Send(new Application.Rules.Details.Query { ProcessName = callBackData }), InlineKeyboards.RuleMenuKeyboard(), response, cancellationToken);
                return;
            }

            if (userRequest.Item != "")
            {
                RuleItemHandler.Handle(botclient, update, userRequest, mediator, response, cancellationToken);
            }

            //Refresh the list
            if (callBackData == CommonItems.Refresh)
            {
                await KeyboardHandler.HandleListRequest(botclient, update, userRequest, await mediator.Send(new Application.Rules.List.Query()), CommonItems.BackToRules, cancellationToken);
                return;
            }

            //Back to Rules menu
            if (callBackData == CommonItems.BackToRules)
            {
                await KeyboardHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                userRequest.SubMenu = "";
                userRequest.Items = new List<CommonDto>();
                return;
            }
        }
    }
}