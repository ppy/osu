// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    public class HistoryTextBox : FocusedTextBox
    {
        private readonly int historyLimit;
        private readonly List<string> messageHistory;

        public IReadOnlyList<string> MessageHistory =>
            Enumerable.Range(0, HistoryLength).Select(GetOldMessage).ToList();

        public int HistoryLength => messageHistory.Count;

        private int startIndex;
        private int selectedIndex = -1;

        private string originalMessage = string.Empty;
        private bool everythingSelected;

        private int getNormalizedIndex(int index) =>
            (HistoryLength + startIndex - index - 1) % HistoryLength;

        public HistoryTextBox(int historyLimit = 100)
        {
            if (historyLimit <= 0)
                throw new ArgumentOutOfRangeException();

            this.historyLimit = historyLimit;
            messageHistory = new List<string>(historyLimit);

            Current.ValueChanged += text =>
            {
                if (string.IsNullOrEmpty(text.NewValue) || everythingSelected)
                {
                    selectedIndex = -1;
                    everythingSelected = false;
                }
            };
        }

        protected override void OnTextSelectionChanged(TextSelectionType selectionType)
        {
            everythingSelected = SelectedText == Text;

            base.OnTextSelectionChanged(selectionType);
        }

        public string GetOldMessage(int index)
        {
            if (index < 0 || index >= HistoryLength)
                throw new ArgumentOutOfRangeException();

            return HistoryLength == 0 ? string.Empty : messageHistory[getNormalizedIndex(index)];
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (selectedIndex == HistoryLength - 1)
                        return true;

                    if (selectedIndex == -1)
                        originalMessage = Text;

                    Text = messageHistory[getNormalizedIndex(++selectedIndex)];

                    return true;

                case Key.Down:
                    if (selectedIndex == -1)
                        return true;

                    if (selectedIndex == 0)
                    {
                        selectedIndex = -1;
                        Text = originalMessage;
                        return true;
                    }

                    Text = messageHistory[getNormalizedIndex(--selectedIndex)];

                    return true;
            }

            return base.OnKeyDown(e);
        }

        protected override void Commit()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                if (HistoryLength == historyLimit)
                {
                    messageHistory[startIndex++] = Text;
                    startIndex %= historyLimit;
                }
                else
                {
                    messageHistory.Add(Text);
                }
            }

            selectedIndex = -1;

            base.Commit();
        }
    }
}
