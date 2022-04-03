// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;

#nullable enable

namespace osu.Game.IPC
{
    /// <summary>
    /// Provides IPC between osu! instances.
    /// </summary>
    [Cached]
    public class IpcServer : CompositeDrawable
    {
        private readonly IIpcHost host;
        private readonly Dictionary<string, ILoadableIpcChannel> channels = new Dictionary<string, ILoadableIpcChannel>();

        public IpcServer(IIpcHost host)
        {
            this.host = host;
            this.host.MessageReceived += handleMessage;
        }

        public Task SendMessageAsync<T>(T message) => host.SendMessageAsync(new IpcMessage
        {
            Type = typeof(T).AssemblyQualifiedName,
            Value = message,
        });

        public IpcServer PrepareSingleUse<TChannel>(out TChannel chan)
            where TChannel : Component, ILoadableIpcChannel, new()
        {
            chan = new TChannel();
            CreateChildDependencies(null).Inject(chan);

            return this;
        }

        private IpcMessage? handleMessage(IpcMessage message)
        {
            Schedule(() =>
            {
                if (!channels.TryGetValue(message.Type, out var chan))
                    chan = tryLoadChannelByType(message.Value.GetType());

                if (chan == null)
                    return;

                // Can't send a response because this may load components
                chan.HandleMessage(message.Value);
            });

            return null;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (!IsDisposed)
                host.MessageReceived -= handleMessage;

            base.Dispose(isDisposing);
        }

        private ILoadableIpcChannel? tryLoadChannelByType(Type messageType)
        {
            var attribute = messageType.GetCustomAttribute<IpcMessageAttribute>();
            if (attribute == null)
                return null;

            var chanType = attribute.ChannelType;
            Debug.Assert(typeof(ILoadableIpcChannel).IsAssignableFrom(chanType));

            ILoadableIpcChannel channel = (ILoadableIpcChannel)Activator.CreateInstance(chanType);
            LoadComponent((Component)channel);
            AddInternal((Component)channel);
            return channel;
        }
    }
}
