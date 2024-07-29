using Application.DTOs;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.MessageContentHandlers;
using TGBot.Menu;
using TGBot.Models;

namespace TGBot.MenuHandlers.Rules.SubmenuHandlers
{
    public static class RulesAddHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;

            if (userRequest.Item != "")
            {
                if (userRequest.Boundaries.StartTime == TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                {
                    response = "Select blocker end time";
                    await MessageContentHandler.HandleStartTimeInput(botclient, update, userRequest, await InlineKeyboards.ListKeyboard(CommonItems.BackToRules), response, cancellationToken);
                    return;
                }

                if (userRequest.Boundaries.StartTime != TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
                {
                    userRequest.Boundaries.EndTime = TimeOnly.Parse(callBackData);
                    var Rule = new Domain.Rule()
                    {
                        ProcessName = userRequest.Item,
                        BlockStartTime = userRequest.Boundaries.StartTime,
                        BlockEndTime = userRequest.Boundaries.EndTime
                    };
                    await MessageContentHandler.HandleEndTimeInput(botclient, update, await mediator.Send(new Application.Rules.Add.Command { Process = Rule }), InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                    userRequest.SubMenu = "";
                    userRequest.Item = "";
                    userRequest.Boundaries = new RuleDto();
                    return;
                }

                if (callBackData == CommonItems.BackToRules)
                {
                    await MessageContentHandler.HandleSimpleMenuRequest(botclient, update, InlineKeyboards.RulesMenuKeyboard(), response, cancellationToken);
                    userRequest.SubMenu = "";
                    userRequest.Item = "";
                    userRequest.Boundaries = new RuleDto();
                    return;
                }
            }
        }
    }
}