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
                            InlineKeyboardButton.WithCallbackData(BaseMenu.Back)
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
                            InlineKeyboardButton.WithCallbackData(BaseMenu.Back)
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
                            InlineKeyboardButton.WithCallbackData(BaseMenu.Back)
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
                            InlineKeyboardButton.WithCallbackData(BaseMenu.Back)
                        ]
                    }
                );
        }

        public static async Task<InlineKeyboardMarkup> ListKeyboard<T>(List<T> values) where T : INamedProcess
        {
            List<List<InlineKeyboardButton>> buttons = [];
            await Task.Run(() =>
            {
                for (int i = 0, j = 0; i < values.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        buttons.Add([InlineKeyboardButton.WithCallbackData(values[i].ProcessName)]);
                    }
                    else
                    {
                        buttons[j].Add(InlineKeyboardButton.WithCallbackData(values[i].ProcessName));
                        j++;
                    }
                }

                buttons.Add([InlineKeyboardButton.WithCallbackData(BaseMenu.Back)]);
            });
            return new InlineKeyboardMarkup(buttons);
        }
    }
}