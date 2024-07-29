using Application.DTOs;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.MessageContentHandlers;
using TGBot.Menu;
using TGBot.Models;

namespace TGBot.MenuHandlers.Rules.SubmenuHandlers
{
    public static class RulesEditAllHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;

            if (userRequest.Boundaries.StartTime == TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
            {
                response = "Select blocker end time";
                await MessageContentHandler.HandleStartTimeInput(botclient, update, userRequest, await InlineKeyboards.ListKeyboard(CommonItems.BackToRules), response, cancellationToken);
                return;
            }

            if (userRequest.Boundaries.StartTime != TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
            {
                userRequest.Boundaries.EndTime = TimeOnly.Parse(callBackData);
                await MessageContentHandler.HandleEndTimeInput(botclient, update, await mediator.Send(new Application.Rules.EditAll.Command { Boundaries = userRequest.Boundaries }), InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                userRequest.SubMenu = "";
                userRequest.Boundaries = new RuleDto();
                return;
            }

            if (callBackData == CommonItems.BackToRules)
            {
                await MessageContentHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                userRequest.SubMenu = "";
                userRequest.Boundaries = new RuleDto();
                return;
            }
        }
    }
}