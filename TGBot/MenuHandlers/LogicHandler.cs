using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.KeyboardHandlers;
using TGBot.Menu;
using TGBot.Models;

namespace TGBot.MenuHandlers
{
    public class LogicHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;

            if (callBackData == Logic.Start)
            {
                response = "Blocker logic has been started successfully";
                await KeyboardHandler.HandleFinalRequest(botclient, update, await mediator.Send(new Application.Logic.Start.Command()), response, cancellationToken);
                return;
            }

            if (callBackData == Logic.Stop)
            {
                response = "Blocker logic has been stopped successfully";
                await KeyboardHandler.HandleFinalRequest(botclient, update, await mediator.Send(new Application.Logic.Stop.Command()), response, cancellationToken);
                return;
            }

            if (callBackData == CommonItems.BackToMain)
            {
                await KeyboardHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.MainMenuKeyboard(), response, cancellationToken);
                userRequest.Menu = CommonItems.BackToMain;
                return;
            }
        }
    }
}