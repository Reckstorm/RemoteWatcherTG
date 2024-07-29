using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBot.KeyboardHandlers;
using TGBot.Menu;
using TGBot.MenuHandlers.SubmenuHandlers.Rules.ItemHandlers.ItemMenuHandlers;
using TGBot.Models;

namespace TGBot.MenuHandlers.SubmenuHandlers.Rules.ItemHandlers
{
    public static class RuleItemHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;

            if (callBackData == Rule.EditName)
            {
                response = $"Current name: <b>{userRequest.Item}</b>\nProvide a new process name in a form of \"Name: ProcessName\"\nWithout quotes!\nIn case you want to cancell simply send /menu to start the flow over";
                await KeyboardHandler.HandleMenuRequestWithTextResponse(botclient, update, response, cancellationToken);
                return;
            }

            //Edit Rule time boundaries
            if (callBackData == Rule.EditTime)
            {
                response = $"Current name: <b>{userRequest.Item}</b>\nSelect new blocker start time";
                await botclient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: await InlineKeyboards.ListKeyboard(CommonItems.BackToDetails), parseMode: ParseMode.Html);
                userRequest.ItemMenu = Rule.EditTime;
                return;
            }

            if (userRequest.ItemMenu == Rule.EditTime)
            {
                RuleEditTimeHandler.Handle(botclient, update, userRequest, mediator, response, cancellationToken);
            }

            //Delete rule
            if (callBackData == Rule.Delete)
            {
                response = $"Are you sure you want to delete rule {userRequest.Item}?";
                await KeyboardHandler.HandleConfirmationRequest(botclient, update, response, cancellationToken);
                userRequest.ItemMenu = Rule.Delete;
                return;
            }

            if (userRequest.ItemMenu == Rule.Delete)
            {
                RuleDeleteHandler.Handle(botclient, update, userRequest, mediator, response, cancellationToken);
            }

            //Back to list
            if (callBackData == CommonItems.BackToList)
            {
                await KeyboardHandler.HandleListRequest(botclient, update, userRequest, await mediator.Send(new Application.Rules.List.Query()), CommonItems.BackToRules, cancellationToken);
                userRequest.Item = "";
                return;
            }
        }
    }
}