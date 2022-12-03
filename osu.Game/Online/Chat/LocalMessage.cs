// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

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
            Timestamp = DateTimeOffset.Now;
        }
    }
}
