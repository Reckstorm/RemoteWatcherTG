using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.KeyboardHandlers;
using TGBot.Menu;
using TGBot.MenuHandlers.Rules.SubmenuHandlers;
using TGBot.Models;

namespace TGBot.MenuHandlers
{
    public static class RulesHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;

            //Add new Rule
            if (callBackData == Menu.Rules.Add)
            {
                response = "Provide a process name in a form of \"Name: ProcessName\"\nWithout quotes!\nIn case you want to cancell simply send /menu to start the flow over";
                userRequest.SubMenu = Menu.Rules.Add;
                await KeyboardHandler.HandleMenuRequestWithTextResponse(botclient, update, response, cancellationToken);
                return;
            }

            //Handle work with Add rules submenu
            if (userRequest.SubMenu == Menu.Rules.Add)
            {
                RulesAddHandler.Handle(botclient, update, userRequest, mediator, response, cancellationToken);
            }

            //Edit all Rules
            if (callBackData == Menu.Rules.EditAll)
            {
                response = "Select blocker start time";
                userRequest.SubMenu = Menu.Rules.EditAll;
                await botclient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: await InlineKeyboards.ListKeyboard(CommonItems.BackToRules));
                return;
            }

            //Handle work with EditAll rules submenu
            if (userRequest.SubMenu == Menu.Rules.EditAll)
            {
                RulesEditAllHandler.Handle(botclient, update, userRequest, mediator, response, cancellationToken);
            }

            //Delete all Rulees
            if (callBackData == Menu.Rules.DeleteAll)
            {
                response = "Are you sure you want to delete all rules?";
                userRequest.SubMenu = Menu.Rules.DeleteAll;
                await KeyboardHandler.HandleConfirmationRequest(botclient, update, response, cancellationToken);
                return;
            }

            //Handle work with DeleteAll rules submenu
            if (userRequest.SubMenu == Menu.Rules.DeleteAll)
            {
                RulesDeleteAllHandler.Handle(botclient, update, userRequest, mediator, response, cancellationToken);
            }

            //List all Rules
            if (callBackData == Menu.Rules.List)
            {
                await KeyboardHandler.HandleListRequest(botclient, update, userRequest, await mediator.Send(new Application.Rules.List.Query()), CommonItems.BackToRules, cancellationToken);
                userRequest.SubMenu = Menu.Rules.List;
                return;
            }

            //Handle work with List submenu
            if (userRequest.SubMenu == Menu.Rules.List)
            {
                RulesListHandler.Handle(botclient, update, userRequest, mediator, response, cancellationToken);
            }

            //Back to main
            if (callBackData == CommonItems.BackToMain)
            {
                await KeyboardHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.MainMenuKeyboard(), response, cancellationToken);
                userRequest.Menu = CommonItems.BackToMain;
                return;
            }
        }
    }
}