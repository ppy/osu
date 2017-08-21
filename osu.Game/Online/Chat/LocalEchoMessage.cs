// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Online.Chat
{
    public class LocalEchoMessage : Message
    {
        public override bool Equals(Message other) => other is LocalEchoMessage ? this == other : base.Equals(other);
    }
}
