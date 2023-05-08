﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.Chat
{
    public class InfoMessage : LocalMessage
    {
        public InfoMessage(string message)
            : base(null)
        {
            Content = message;

            Sender = APIUser.SYSTEM_USER;
        }
    }
}
