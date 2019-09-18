// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    public class InfoMessage : LocalMessage
    {
        private static int infoID = -1;

        public InfoMessage(string message)
            : base(infoID--)
        {
            Timestamp = DateTimeOffset.Now;
            Content = message;

            Sender = User.SYSTEM_USER;
        }
    }
}
