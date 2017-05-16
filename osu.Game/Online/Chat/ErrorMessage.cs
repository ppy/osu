// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    public class ErrorMessage : Message
    {
        private static int errorId = -1;

        public ErrorMessage(string message) : base(errorId--)
        {
            Timestamp = DateTime.Now;
            Content = message;

            Sender = new User
            {
                Username = @"system",
                Colour = @"ff0000",
            };
        }
    }
}