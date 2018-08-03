// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// A message which is generated and displayed locally.
    /// </summary>
    public class LocalMessage : Message
    {
        protected LocalMessage(long? id)
            : base(id)
        {
        }
    }
}
