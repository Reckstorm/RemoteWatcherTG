using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBot.MessageContentHandlers;
using TGBot.Menu;
using TGBot.MenuHandlers.SubmenuHandlers.Processes.ItemHandlers.ItemMenuHandlers;
using TGBot.Models;

namespace TGBot.MenuHandlers.SubmenuHandlers.Processes.ItemHandlers
{
    public static class ProcessItemHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;

            //Specific process actions
            if (callBackData == Process.Kill)
            {
                await MessageContentHandler.HandleFinalRequest(botclient, update, await mediator.Send(new Application.Processes.Kill.Command { ProcessName = userRequest.Item }), "Process successfully killed", cancellationToken);
                await MessageContentHandler.HandleListRequest(botclient, update, userRequest, await mediator.Send(new Application.Processes.List.Query()), CommonItems.BackToProcesses, cancellationToken);
                userRequest.Item = "";
                return;
            }

            //Add a rule
            if (callBackData == Process.Add)
            {
                response = $"Current name: <b>{userRequest.Item}</b>\nSelect blocker start time";
                await botclient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: await InlineKeyboards.ListKeyboard(CommonItems.BackToDetails), parseMode: ParseMode.Html);
                userRequest.ItemMenu = Process.Add;
                return;
            }

            if (userRequest.ItemMenu == Process.Add)
            {
                RuleAddHandler.Handle(botclient, update, userRequest, mediator, response, cancellationToken);
            }

            //Back to Processes list
            if (callBackData == CommonItems.BackToList)
            {
                await MessageContentHandler.HandleListRequest(botclient, update, userRequest, await mediator.Send(new Application.Processes.List.Query()), CommonItems.BackToProcesses, cancellationToken);
                userRequest.Item = "";
                return;
            }
        }
    }
}