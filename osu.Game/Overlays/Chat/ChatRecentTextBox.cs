// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osuTK.Input;

namespace osu.Game.Overlays.Chat
{
    public class ChatRecentTextBox : FocusedTextBox
    {
        private readonly List<string> messageHistory = new List<string>();

        private int messageIndex = -1;

        private string originalMessage = string.Empty;

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            /* Behavior:
             * add when on last element -> last element stays
             * subtract when on first element -> sets to original text
             * reset indexing when Text is set to Empty
             */

            switch (e.Key)
            {
                case Key.Up:
                    if (messageIndex == -1)
                        originalMessage = Text;

                    if (messageIndex == messageHistory.Count - 1)
                        return true;

                    Text = messageHistory[++messageIndex];

                    return true;

                case Key.Down:
                    if (messageIndex == -1)
                        return true;

                    if (messageIndex == 0)
                    {
                        messageIndex = -1;
                        Text = originalMessage;
                        return true;
                    }

                    Text = messageHistory[--messageIndex];

                    return true;
            }

            bool onKeyDown = base.OnKeyDown(e);

            if (string.IsNullOrEmpty(Text))
                messageIndex = -1;

            return onKeyDown;
        }

        protected override void Commit()
        {
            if (!string.IsNullOrEmpty(Text))
                messageHistory.Insert(0, Text);

            messageIndex = -1;

            base.Commit();
        }
    }
}
