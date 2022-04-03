// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;

#nullable enable

namespace osu.Game.IPC
{
    public interface ILoadableIpcChannel
    {
        IpcMessage? HandleMessage(object message);
    }

    public abstract class LoadableIpcChannel<TMessage> : Component, ILoadableIpcChannel
        where TMessage : class
    {
        [Resolved]
        protected IpcServer Ipc { get; private set; } = null!;

        IpcMessage? ILoadableIpcChannel.HandleMessage(object message) => HandleMessage((TMessage)message);

        /// <remarks>
        /// Right now the return value isn't used because the class may be constructed asynchronously.
        /// Unless there exists a channel that generates a response message, there's no need to add the extra logic.
        /// </remarks>
        protected abstract IpcMessage? HandleMessage(TMessage message);
    }
}
