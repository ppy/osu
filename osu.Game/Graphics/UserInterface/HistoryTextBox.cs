// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Difficulty.Utils;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    public class HistoryTextBox : FocusedTextBox
    {
        private readonly LimitedCapacityQueue<string> messageHistory;

        public IEnumerable<string> GetMessageHistory()
        {
            if (HistoryCount == 0)
                yield break;

            for (int i = HistoryCount - 1; i >= 0; i--)
                yield return messageHistory[i];
        }

        public int HistoryCount => messageHistory.Count;

        private int selectedIndex;

        private string originalMessage = string.Empty;
        private bool everythingSelected;

        public string GetOldMessage(int index) => messageHistory[HistoryCount - index - 1];

        public HistoryTextBox(int capacity = 100)
        {
            messageHistory = new LimitedCapacityQueue<string>(capacity);

            Current.ValueChanged += text =>
            {
                if (string.IsNullOrEmpty(text.NewValue) || everythingSelected)
                {
                    selectedIndex = HistoryCount;
                    everythingSelected = false;
                }
            };
        }

        protected override void OnTextSelectionChanged(TextSelectionType selectionType)
        {
            everythingSelected = SelectedText == Text;

            base.OnTextSelectionChanged(selectionType);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
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
