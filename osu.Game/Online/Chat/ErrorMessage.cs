// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Online.Chat
{
    public class ErrorMessage : InfoMessage
    {
        public ErrorMessage(string message) : base(message)
        {
            Sender.Colour = @"ff0000";
        }
    }
}
