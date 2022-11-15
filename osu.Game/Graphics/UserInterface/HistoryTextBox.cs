// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    public class HistoryTextBox : FocusedTextBox
    {
        private readonly int historyLimit;

        private readonly List<string> messageHistory;

        public IReadOnlyList<string> MessageHistory => messageHistory;

        private int startIndex;

        private string originalMessage = string.Empty;
        private int nullIndex => -1;
        private int historyIndex = -1;
        private int endIndex => (messageHistory.Count + startIndex - 1) % Math.Max(1, messageHistory.Count);

        public HistoryTextBox(int historyLimit = 100)
        {
            this.historyLimit = historyLimit;
            messageHistory = new List<string>(historyLimit);

            Current.ValueChanged += text =>
            {
                if (string.IsNullOrEmpty(text.NewValue))
                    historyIndex = nullIndex;
            };
        }

        public string GetOldMessage(int index)
        {
            if (index < 0 || index >= messageHistory.Count)
                throw new ArgumentOutOfRangeException();

            return messageHistory[(startIndex + index) % messageHistory.Count];
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (historyIndex == nullIndex)
                    {
                        historyIndex = endIndex;
                        originalMessage = Text;
                    }

                    if (historyIndex == startIndex)
                        return true;

                    historyIndex = (historyLimit + historyIndex - 1) % historyLimit;
                    Text = messageHistory[historyIndex];

                    return true;

                case Key.Down:
                    if (historyIndex == nullIndex)
                        return true;

                    if (historyIndex == endIndex)
                    {
                        historyIndex = nullIndex;
                        Text = originalMessage;
                        return true;
                    }

                    historyIndex = (historyIndex + 1) % historyLimit;
                    Text = messageHistory[historyIndex];

                    return true;
            }

            return base.OnKeyDown(e);
        }

        protected override void Commit()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                if (messageHistory.Count == historyLimit)
                {
                    messageHistory[startIndex++] = Text;
                }
                else
                {
                    messageHistory.Add(Text);
                }
            }

            historyIndex = nullIndex;

            base.Commit();
        }
    }
}
