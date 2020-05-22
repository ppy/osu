// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;

namespace osu.Game.Online.Chat
{
    [Cached(typeof(IChannelPostTarget))]
    public interface IChannelPostTarget
    {
        /// <summary>
        /// Posts a message to the currently opened channel.
        /// </summary>
        /// <param name="text">The message text that is going to be posted</param>
        /// <param name="isAction">Is true if the message is an action, e.g.: user is currently eating </param>
        /// <param name="target">An optional target channel. If null, <see cref="ChannelManager.CurrentChannel"/> will be used.</param>
        void PostMessage(string text, bool isAction = false, Channel target = null);
    }
}
