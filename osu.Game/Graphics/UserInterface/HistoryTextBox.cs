// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    public class HistoryTextBox : FocusedTextBox
    {
        private readonly List<string> messageHistory = new List<string>();

        public IReadOnlyList<string> MessageHistory => messageHistory;

        private int historyIndex = -1;

        private string originalMessage = string.Empty;

        public HistoryTextBox()
        {
            Current.ValueChanged += text =>
            {
                if (string.IsNullOrEmpty(text.NewValue))
                    historyIndex = -1;
            };
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (historyIndex == -1)
                        originalMessage = Text;

                    if (historyIndex == messageHistory.Count - 1)
                        return true;

                    Text = messageHistory[++historyIndex];

                    return true;

                case Key.Down:
                    if (historyIndex == -1)
                        return true;

                    if (historyIndex == 0)
                    {
                        historyIndex = -1;
                        Text = originalMessage;
                        return true;
                    }

                    Text = messageHistory[--historyIndex];

                    return true;
            }

            return base.OnKeyDown(e);
        }

        protected override void Commit()
        {
            if (!string.IsNullOrEmpty(Text))
                messageHistory.Insert(0, Text);

            historyIndex = -1;

            base.Commit();
        }
    }
}
