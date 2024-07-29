using Application.DTOs;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.MessageContentHandlers;
using TGBot.Menu;
using TGBot.MenuHandlers.SubmenuHandlers.Processes.ItemHandlers;
using TGBot.Models;

namespace TGBot.MenuHandlers.Processes.SubmenuHandlers
{
    public static class ProcessesListHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;
            
            //Process menu actions
            if (userRequest.Items.Any(x => x.ProcessName.Equals(callBackData)))
            {
                response = "You can either kill this process with the relevant button below or go back";
                await MessageContentHandler.HandleDetailsRequest(botclient, update, userRequest, await mediator.Send(new Application.Processes.Details.Query { ProcessName = callBackData }), InlineKeyboards.ProcessMenuKeyboard(), response, cancellationToken);
                return;
            }

            //Handle requests about selected item
            if (userRequest.Item != "")
            {
                ProcessItemHandler.Handle(botclient, update, userRequest, mediator, response, cancellationToken);
            }

            //Refresh the list
            if (callBackData == CommonItems.Refresh)
            {
                await MessageContentHandler.HandleListRequest(botclient, update, userRequest, await mediator.Send(new Application.Processes.List.Query()), CommonItems.BackToProcesses, cancellationToken);
                return;
            }

            //Back to Processes menu
            if (callBackData == CommonItems.BackToProcesses)
            {
                await MessageContentHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.ProcessesMenuKeyboard(), response, cancellationToken);
                userRequest.SubMenu = "";
                userRequest.Items = new List<CommonDto>();
                return;
            }
        }
    }
}