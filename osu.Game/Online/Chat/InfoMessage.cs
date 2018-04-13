﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    public class InfoMessage : Message
    {
        private static int infoID = -1;

        public InfoMessage(string message) : base(infoID--)
        {
            Timestamp = DateTimeOffset.Now;
            Content = message;

            Sender = new User
            {
                Username = @"system",
                Colour = @"0000ff",
            };
        }
    }
}
