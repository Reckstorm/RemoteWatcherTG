using Application.DTOs;
using Domain;
using Telegram.Bot.Types.ReplyMarkups;
using TGBot.Menus;

namespace TGBot
{
    public static class InlineKeyboards
    {
        public static InlineKeyboardMarkup BaseMenuKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                    [
                        InlineKeyboardButton.WithCallbackData(BaseMenu.Logic),
                        InlineKeyboardButton.WithCallbackData(BaseMenu.RProcess)
                    ],
                    [
                        InlineKeyboardButton.WithCallbackData(BaseMenu.Process)
                    ]
                    }
                );
        }

        public static InlineKeyboardMarkup LogicMenuKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                        [
                            InlineKeyboardButton.WithCallbackData(LogicMenu.Start),
                            InlineKeyboardButton.WithCallbackData(LogicMenu.Stop)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(CommonMenuItems.BackToBase)
                        ]
                    }
                );
        }

        public static InlineKeyboardMarkup ProcessesMenuKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                        [
                            InlineKeyboardButton.WithCallbackData(ProcessesMenu.List)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(CommonMenuItems.BackToBase)
                        ]
                    }
                );
        }

        public static InlineKeyboardMarkup RProcessesMenuKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                        [
                            InlineKeyboardButton.WithCallbackData(RProcessesMenu.Add),
                            InlineKeyboardButton.WithCallbackData(RProcessesMenu.List)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(RProcessesMenu.EditAll),
                            InlineKeyboardButton.WithCallbackData(RProcessesMenu.DeleteAll)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(CommonMenuItems.BackToBase)
                        ]
                    }
                );
        }

        public static InlineKeyboardMarkup ProcessMenuKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                        [
                            InlineKeyboardButton.WithCallbackData(ProcessMenu.Kill)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(CommonMenuItems.BackToList)
                        ]
                    }
                );
        }

        public static InlineKeyboardMarkup RProcessMenuKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                        [
                            InlineKeyboardButton.WithCallbackData(RProcessMenu.EditName),
                            InlineKeyboardButton.WithCallbackData(RProcessMenu.EditTime)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(RProcessMenu.Delete)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(CommonMenuItems.BackToList)
                        ]
                    }
                );
        }

        public static async Task<InlineKeyboardMarkup> ListKeyboard(List<CommonProcessDto> items)
        {
            int rowItemsCount = 4;
            List<List<InlineKeyboardButton>> buttons = [];
            await Task.Run(() =>
            {
                foreach (var item in items)
                {
                    int i = items.IndexOf(item);
                    if (i == 0 || i % rowItemsCount == 0)
                    {
                        buttons.Add([InlineKeyboardButton.WithCallbackData(item.ProcessName)]);
                    }
                    else
                    {
                        buttons.Last().Add(InlineKeyboardButton.WithCallbackData(item.ProcessName));
                    }
                }
                buttons.Add([InlineKeyboardButton.WithCallbackData(CommonMenuItems.BackToProcesses)]);
            });
            return new InlineKeyboardMarkup(buttons);
        }

        public static async Task<InlineKeyboardMarkup> ListKeyboard()
        {
            int rowItemsCount = 4;
            List<List<InlineKeyboardButton>> buttons = [];
            await Task.Run(() =>
            {
                foreach (var item in TimeMenu.TimeOptions)
                {
                    int i = TimeMenu.TimeOptions.IndexOf(item);
                    if (i == 0 || i % rowItemsCount == 0)
                    {
                        buttons.Add([InlineKeyboardButton.WithCallbackData(item)]);
                    }
                    else
                    {
                        buttons.Last().Add(InlineKeyboardButton.WithCallbackData(item));
                    }
                }
                buttons.Add([InlineKeyboardButton.WithCallbackData(CommonMenuItems.BackToProcesses)]);
            });
            return new InlineKeyboardMarkup(buttons);
        }
    }
}