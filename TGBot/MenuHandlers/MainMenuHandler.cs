using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.KeyboardHandlers;
using TGBot.Menu;
using TGBot.Models;

namespace TGBot.MenuHandlers
{
    public static class MainMenuHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;

            if (callBackData == MainMenu.Rule)
            {
                await KeyboardHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                userRequest.Menu = MainMenu.Rule;
                return;
            }

            if (callBackData == MainMenu.Process)
            {
                await KeyboardHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.ProcessesMenuKeyboard(), response, cancellationToken);
                userRequest.Menu = MainMenu.Process;
                return;
            }

            if (callBackData == MainMenu.Logic)
            {
                await KeyboardHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.LogicKeyboard(), response, cancellationToken);
                userRequest.Menu = MainMenu.Logic;
                return;
            }
        }
    }
}