using Application.DTOs;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using TGBot.KeyboardHandlers;
using TGBot.Menu;
using TGBot.Models;

namespace TGBot.MenuHandlers.SubmenuHandlers.Processes.ItemHandlers.ItemMenuHandlers
{
    public static class RuleAddHandler
    {
        public static async void Handle(ITelegramBotClient botclient, Update update, UserRequest userRequest, IMediator mediator, string response, CancellationToken cancellationToken)
        {
            var callBackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;

            if (userRequest.Boundaries.StartTime == TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
            {
                response = "Select blocker end time";
                await KeyboardHandler.HandleStartTimeInput(botclient, update, userRequest, await InlineKeyboards.ListKeyboard(CommonItems.BackToDetails), response, cancellationToken);
                return;
            }

            if (userRequest.Boundaries.StartTime != TimeOnly.MaxValue && TimeMenu.TimeOptions.Any(x => x.Equals(callBackData)))
            {
                userRequest.Boundaries.EndTime = TimeOnly.Parse(callBackData);

                var rule = new Domain.Rule()
                {
                    ProcessName = userRequest.Item,
                    BlockStartTime = userRequest.Boundaries.StartTime,
                    BlockEndTime = userRequest.Boundaries.EndTime
                };

                await KeyboardHandler.HandleFinalRequest(botclient, update, await mediator.Send(new Application.Rules.Add.Command { Process = rule }), "Success", cancellationToken);
                response = "Choose an action towards the process";
                await KeyboardHandler.HandleDetailsRequest(botclient, update, userRequest, await mediator.Send(new Application.Processes.Details.Query { ProcessName = rule.ProcessName }), InlineKeyboards.ProcessMenuKeyboard(), response, cancellationToken);
                userRequest.Boundaries = new RuleDto();
                userRequest.ItemMenu = "";
                return;
            }

            if (callBackData == CommonItems.BackToDetails)
            {
                await KeyboardHandler.HandleDetailsRequest(botclient, update, userRequest, await mediator.Send(new Application.Processes.Details.Query { ProcessName = userRequest.Item }), InlineKeyboards.ProcessMenuKeyboard(), response, cancellationToken);
                userRequest.Boundaries = new RuleDto();
                userRequest.ItemMenu = "";
                return;
            }
        }
    }
}