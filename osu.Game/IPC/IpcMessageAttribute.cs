// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.IPC
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IpcMessageAttribute : Attribute
    {
        public Type ChannelType { get; }

        public IpcMessageAttribute(Type channelType)
        {
            ChannelType = channelType;
        }
    }
}
