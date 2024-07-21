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
                for (int i = 0; i < items.Count; i++)
                {
                    if (i == 0 || i % rowItemsCount == 0)
                    {
                        buttons.Add([InlineKeyboardButton.WithCallbackData(items[i].ProcessName)]);
                    }
                    else
                    {
                        buttons.Last().Add(InlineKeyboardButton.WithCallbackData(items[i].ProcessName));
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
                for (int i = 0; i < TimeMenu.TimeOptions.Count; i++)
                {
                    if (i == 0 || i % rowItemsCount == 0)
                    {
                        buttons.Add([InlineKeyboardButton.WithCallbackData(TimeMenu.TimeOptions[i])]);
                    }
                    else
                    {
                        buttons.Last().Add(InlineKeyboardButton.WithCallbackData(TimeMenu.TimeOptions[i]));
                    }
                }
                buttons.Add([InlineKeyboardButton.WithCallbackData(CommonMenuItems.BackToProcesses)]);
            });
            return new InlineKeyboardMarkup(buttons);
        }
    }
}