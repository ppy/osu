// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// Interface for consuming online chat.
    /// </summary>
    public interface IChatClient : IDisposable
    {
        /// <summary>
        /// Fired when a <see cref="Channel"/> has been joined.
        /// </summary>
        event Action<Channel>? ChannelJoined;

        /// <summary>
        /// Fired when a <see cref="Channel"/> has been parted.
        /// </summary>
        event Action<Channel>? ChannelParted;

        /// <summary>
        /// Fired when new <see cref="Message"/>s have arrived from the server.
        /// </summary>
        event Action<List<Message>>? NewMessages;

        /// <summary>
        /// Requests presence information from the server.
        /// </summary>
        void RequestPresence();

        /// <summary>
        /// Fired when the initial user presence information has been received.
        /// </summary>
        event Action? PresenceReceived;
    }
}
