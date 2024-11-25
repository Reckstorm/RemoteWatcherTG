using Application.DTOs;
using Domain;
using Telegram.Bot.Types.ReplyMarkups;
using TGBot.Menu;

namespace TGBot
{
    public static class InlineKeyboards
    {
        public static InlineKeyboardMarkup MainMenuKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                    [
                        InlineKeyboardButton.WithCallbackData(MainMenu.Rule)
                    ],
                    [
                        InlineKeyboardButton.WithCallbackData(MainMenu.Process)
                    ],
                    [
                        InlineKeyboardButton.WithCallbackData(MainMenu.Logic)
                    ]
                    }
                );
        }

        public static InlineKeyboardMarkup LogicKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                        [
                            InlineKeyboardButton.WithCallbackData(Logic.Start),
                            InlineKeyboardButton.WithCallbackData(Logic.StopCompletely)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(Logic.StartNormalBlock),
                            InlineKeyboardButton.WithCallbackData(Logic.StopUntilStartTime)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(CommonItems.BackToMain)
                        ]
                    }
                );
        }

        public static InlineKeyboardMarkup ProcessesMenuKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                        [
                            InlineKeyboardButton.WithCallbackData(Processes.List)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(CommonItems.BackToMain)
                        ]
                    }
                );
        }

        public static InlineKeyboardMarkup RulesMenuKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                        [
                            InlineKeyboardButton.WithCallbackData(Rules.Add),
                            InlineKeyboardButton.WithCallbackData(Rules.List)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(Rules.EditAll),
                            InlineKeyboardButton.WithCallbackData(Rules.DeleteAll)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(CommonItems.BackToMain)
                        ]
                    }
                );
        }

        public static InlineKeyboardMarkup ProcessMenuKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                        [
                            InlineKeyboardButton.WithCallbackData(Process.Kill)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(Process.Add)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(CommonItems.BackToList)
                        ]
                    }
                );
        }

        public static InlineKeyboardMarkup RuleMenuKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                        [
                            InlineKeyboardButton.WithCallbackData(Menu.Rule.EditName),
                            InlineKeyboardButton.WithCallbackData(Menu.Rule.EditTime)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(Menu.Rule.Delete)
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(CommonItems.BackToList)
                        ]
                    }
                );
        }

        public static async Task<InlineKeyboardMarkup> ListKeyboard(List<CommonDto> items, string back)
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
                buttons.Add([InlineKeyboardButton.WithCallbackData(CommonItems.Refresh)]);
                buttons.Add([InlineKeyboardButton.WithCallbackData(back)]);
            });
            return new InlineKeyboardMarkup(buttons);
        }

        public static async Task<InlineKeyboardMarkup> ListKeyboard(string back)
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
                buttons.Add([InlineKeyboardButton.WithCallbackData(back)]);
            });
            return new InlineKeyboardMarkup(buttons);
        }

        public static InlineKeyboardMarkup ConfirmationKeyboard()
        {
            return new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][]{
                        [
                            InlineKeyboardButton.WithCallbackData(Confirmation.Yes),
                            InlineKeyboardButton.WithCallbackData(Confirmation.No)
                        ],
                    }
                );
        }
    }
}