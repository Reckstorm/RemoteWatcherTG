using Application.DTOComparers;
using Application.DTOs;
using Application.Logic;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TGBot.Menu;
using TGBot.Models;

namespace TGBot.MessageContentHandlers
{
    public static class MessageContentHandler
    {
        public static async Task HandleMenuRequestWithTextResponse(ITelegramBotClient botClient, Update update, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId,
                text: response, parseMode: ParseMode.Html);
            return;
        }

        public static async Task HandleSimpleMenuRequest(ITelegramBotClient botClient, Update update, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: markup, parseMode: ParseMode.Html);
            return;
        }

        public static async Task HandleConfirmationRequest(ITelegramBotClient botClient, Update update, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: InlineKeyboards.ConfirmationKeyboard());
            return;
        }

        public static async Task HandleFinalRequest<T>(ITelegramBotClient botClient, Update update, Result<T> result, string response, CancellationToken cancellationToken)
        {
            long chatId;
            if (update.Type == UpdateType.Message)
            {
                chatId = update.Message.Chat.Id;
                if (result == null)
                {
                    await botClient.SendTextMessageAsync(chatId, "Failed to process command");
                    return;
                }
                if (!result.IsSuccess && result.Error != null)
                {
                    await botClient.SendTextMessageAsync(chatId, result.Error);
                    return;
                }

                await botClient.SendTextMessageAsync(chatId, response);
            }
            else
            {
                var callbackQueryId = update.CallbackQuery.Id;
                if (result == null)
                {
                    await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Failed to process command");
                    return;
                }
                if (!result.IsSuccess && result.Error != null)
                {
                    await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, result.Error);
                    return;
                }
                await botClient.AnswerCallbackQueryAsync(callbackQueryId, response);
            }
            return;
        }

        public static async Task HandleListRequest<T>(ITelegramBotClient botClient, Update update, UserRequest userRequest, Result<T> result, string back, CancellationToken cancellationToken) where T : List<CommonDto>
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var callbackQueryId = update.CallbackQuery.Id;
            if (result == null)
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Failed to process command");
                return;
            }
            if (!result.IsSuccess && result.Error != null)
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, result.Error);
                return;
            }

            bool res = await EqualListsComparer(userRequest.Items, result.Value, new CommonDtoEqualityComparer());

            if (!res || userRequest.Item != "")
            {
                userRequest.Items = result.Value;
                await botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: update.CallbackQuery.Message.MessageId,
                    text: "Choose a process:",
                    replyMarkup: await InlineKeyboards.ListKeyboard(result.Value, back));
            }

            await botClient.AnswerCallbackQueryAsync(callbackQueryId);
            return;
        }

        public static async Task HandleDetailsRequest(ITelegramBotClient botClient, Update update, UserRequest userRequest, Result<CommonDto> result, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
        {
            long chatId;

            string text = $"Currently chosen process:\nName: <b>{result.Value.ProcessName}</b>";
            if (result.Value.StartTime != TimeOnly.MaxValue && result.Value.EndTime != TimeOnly.MaxValue)
                text += $"\nProcess is blocked from <b>{result.Value.StartTime}</b> to <b>{result.Value.EndTime}</b>";
            text += $"\n{response}";

            if (update.Type == UpdateType.Message)
            {
                chatId = update.Message.Chat.Id;
                if (result == null)
                {
                    await botClient.SendTextMessageAsync(chatId, "Failed to process command");
                    return;
                }
                if (!result.IsSuccess && result.Error != null)
                {
                    await botClient.SendTextMessageAsync(chatId, result.Error);
                    return;
                }
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    replyMarkup: markup,
                    parseMode: ParseMode.Html);
            }
            else
            {
                chatId = update.CallbackQuery.Message.Chat.Id;
                var callbackQueryId = update.CallbackQuery.Id;
                if (result == null)
                {
                    await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Failed to process command");
                    return;
                }
                if (!result.IsSuccess && result.Error != null)
                {
                    await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, result.Error);
                    return;
                }
                await botClient.AnswerCallbackQueryAsync(callbackQueryId);

                await botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: update.CallbackQuery.Message.MessageId,
                    text: text,
                    replyMarkup: markup,
                    parseMode: ParseMode.Html);
            }

            userRequest.Item = TimeMenu.TimeOptions.Any(x => x.Equals(update.CallbackQuery.Data)) || update.CallbackQuery.Data == CommonItems.BackToDetails ?
            userRequest.Item : update.CallbackQuery.Data;

            return;
        }

        public static async Task HandleStartTimeInput(ITelegramBotClient botClient, Update update, UserRequest userRequest, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var callBackData = update.CallbackQuery.Data;
            userRequest.Boundaries.StartTime = TimeOnly.Parse(callBackData);
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: markup);
            return;
        }

        public static async Task HandleEndTimeInput<T>(ITelegramBotClient botClient, Update update, Result<T> result, InlineKeyboardMarkup markup, string response, CancellationToken cancellationToken)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            await HandleFinalRequest(botClient, update, result, "Success", cancellationToken);
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: update.CallbackQuery.Message.MessageId, text: response, replyMarkup: markup);
            return;
        }

        public static async Task<string> HandleLogicStatusRequest(IMediator mediator, string response)
        {
            var result = await mediator.Send(new Status.Query());
            string logicStatus, unblockStatus;

            if (result == null)
            {
                logicStatus = "Error getting status";
                unblockStatus = "Error getting status";
            }
            else if (result != null && result.Error != null)
            {
                logicStatus = result.Error;
                unblockStatus = result.Error;
            }
            else
            {
                logicStatus = result.Value.LogicStatus ? "Running" : "Not running";
                unblockStatus = result.Value.StoppedUntilStartTimeStatus ? "Running" : "Not running";
            }

            return $"Current logic status: <b>{logicStatus}</b>\nCurrent unblock status: <b>{unblockStatus}</b>\n{response}";
        }

        public async static Task<bool> EqualListsComparer<T>(IEnumerable<T> list1, IEnumerable<T> list2, IEqualityComparer<T> comparer)
        {
            bool result = false;

            if (list1.Count() == 0 || list2.Count() == 0) return result;
            if (list1.Count() != list2.Count()) return result;

            await Task.Run(() =>
            {
                var cnt = new Dictionary<T, int>(comparer);
                foreach (T s in list1)
                {
                    if (cnt.ContainsKey(s))
                    {
                        cnt[s]++;
                    }
                    else
                    {
                        cnt.Add(s, 1);
                    }
                }
                foreach (T s in list2)
                {
                    if (cnt.ContainsKey(s))
                    {
                        cnt[s]--;
                    }
                    else
                    {
                        break;
                    }
                }
                result = cnt.Values.All(c => c == 0);
            });
            return result;
        }
    }
}