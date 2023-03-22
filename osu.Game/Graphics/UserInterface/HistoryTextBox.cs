// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Events;
using osu.Game.Utils;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A <see cref="FocusedTextBox"/> which additionally retains a history of text committed, up to a limit
    /// (100 by default, specified in constructor).
    /// The history of committed text can be navigated using up/down arrows.
    /// This resembles the operation of command-line terminals.
    /// </summary>
    public partial class HistoryTextBox : FocusedTextBox
    {
        private readonly LimitedCapacityQueue<string> messageHistory;

        public int HistoryCount => messageHistory.Count;

        private int selectedIndex;

        private string originalMessage = string.Empty;

        /// <summary>
        /// Creates a new <see cref="HistoryTextBox"/>.
        /// </summary>
        /// <param name="capacity">
        /// The maximum number of committed lines to keep in history.
        /// When exceeded, the oldest lines in history will be dropped to make space for new ones.
        /// </param>
        public HistoryTextBox(int capacity = 100)
        {
            messageHistory = new LimitedCapacityQueue<string>(capacity);

            Current.ValueChanged += text =>
            {
                if (selectedIndex != HistoryCount && text.NewValue != messageHistory[selectedIndex])
                {
                    selectedIndex = HistoryCount;
                }
            };
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.ControlPressed || e.AltPressed || e.SuperPressed || e.ShiftPressed)
                return false;

            switch (e.Key)
            {
                case Key.Up:
                    if (selectedIndex == 0)
                        return true;

                    if (selectedIndex == HistoryCount)
                        originalMessage = Text;

                    Text = messageHistory[--selectedIndex];

                    return true;

                case Key.Down:
                    if (selectedIndex == HistoryCount)
                        return true;

                    if (selectedIndex == HistoryCount - 1)
                    {
                        selectedIndex = HistoryCount;
                        Text = originalMessage;
                        return true;
                    }

                    Text = messageHistory[++selectedIndex];

                    return true;
            }

            return base.OnKeyDown(e);
        }

        protected override void Commit()
        {
            if (!string.IsNullOrEmpty(Text))
                messageHistory.Enqueue(Text);

            selectedIndex = HistoryCount;

            base.Commit();
        }
    }
}
