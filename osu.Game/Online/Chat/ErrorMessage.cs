// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.Chat
{
    public class ErrorMessage : InfoMessage
    {
        public ErrorMessage(string message)
            : base(message)
        {
            Sender.Colour = @"ff0000";
        }
    }
}
