using Application.Logic;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBot.MessageContentHandlers;
using TGBot.Menu;
using TGBot.Models;

namespace TGBot.MenuHandlers
{
    public class LogicHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;

            try
            {
                if (callBackData == Logic.Start)
                {
                    response = "Blocker logic has been started successfully";
                    await MessageContentHandler.HandleFinalRequest(botclient, update, await mediator.Send(new Start.Command()), response, cancellationToken);

                    await botclient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId,
                        text: await MessageContentHandler.HandleLogicStatusRequest(mediator, "Choose an action"), replyMarkup: InlineKeyboards.LogicKeyboard(), parseMode: ParseMode.Html);
                    return;
                }

                if (callBackData == Logic.StopCompletely)
                {
                    response = "Blocker logic has been stopped successfully";
                    await MessageContentHandler.HandleFinalRequest(botclient, update, await mediator.Send(new Stop.Command()), response, cancellationToken);

                    await botclient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId,
                        text: await MessageContentHandler.HandleLogicStatusRequest(mediator, "Choose an action"), replyMarkup: InlineKeyboards.LogicKeyboard(), parseMode: ParseMode.Html);
                    return;
                }

                if (callBackData == Logic.StartNormalBlock)
                {
                    response = "Blocker logic has been reset to normal";
                    await MessageContentHandler.HandleFinalRequest(botclient, update, await mediator.Send(new StartNormalBlocking.Command()), response, cancellationToken);

                    await botclient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId,
                        text: await MessageContentHandler.HandleLogicStatusRequest(mediator, "Choose an action"), replyMarkup: InlineKeyboards.LogicKeyboard(), parseMode: ParseMode.Html);
                    return;
                }

                if (callBackData == Logic.StopUntilStartTime)
                {
                    response = "Blocker logic has been stopped successfully until the start time";
                    await MessageContentHandler.HandleFinalRequest(botclient, update, await mediator.Send(new StopUntilStartTime.Command()), response, cancellationToken);

                    await botclient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId,
                        text: await MessageContentHandler.HandleLogicStatusRequest(mediator, "Choose an action"), replyMarkup: InlineKeyboards.LogicKeyboard(), parseMode: ParseMode.Html);
                    return;
                }

                if (callBackData == CommonItems.BackToMain)
                {
                    await MessageContentHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.MainMenuKeyboard(), response, cancellationToken);
                    userRequest.Menu = CommonItems.BackToMain;
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            };

        }
    }
}