using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.KeyboardHandlers;
using TGBot.Menu;
using TGBot.MenuHandlers.Processes.SubmenuHandlers;
using TGBot.Models;

namespace TGBot.MenuHandlers
{
    public static class ProcessesHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;

            //Send a list of processes
            if (callBackData == Menu.Processes.List)
            {
                await KeyboardHandler.HandleListRequest(botclient, update, userRequest, await mediator.Send(new Application.Processes.List.Query()), CommonItems.BackToProcesses, cancellationToken);
                userRequest.SubMenu = Menu.Processes.List;
                return;
            }

            //Handle requests to the items list
            if (userRequest.SubMenu == Menu.Processes.List)
            {
                ProcessesListHandler.Handle(botclient, update, userRequest, mediator, response, cancellationToken);
            }

            //Back to base menu
            if (callBackData == CommonItems.BackToMain)
            {
                await KeyboardHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.MainMenuKeyboard(), response, cancellationToken);
                userRequest.Menu = CommonItems.BackToMain;
                return;
            }
        }
    }
}