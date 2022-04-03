// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Platform;

#nullable enable

namespace osu.Game.IPC.Channels
{
    public class OsuLinkIPCChannel : LoadableIpcChannel<OsuOpenLinkMessage>
    {
        [Resolved(CanBeNull = true)]
        private OsuGame? handler { get; set; }

        protected override IpcMessage? HandleMessage(OsuOpenLinkMessage msg)
        {
            Debug.Assert(handler != null);

            HandleLinkAsync(msg.Link).ContinueWith(t =>
            {
                if (t.Exception != null) throw t.Exception;
            }, TaskContinuationOptions.OnlyOnFaulted);

            return null;
        }

        public async Task HandleLinkAsync(string link)
        {
            if (handler == null)
            {
                // we want to contact a remote osu! to handle the link.
                await Ipc.SendMessageAsync(new OsuOpenLinkMessage { Link = link }).ConfigureAwait(false);
                return;
            }

            handler.HandleLink(link);
        }
    }

    [IpcMessage(typeof(OsuLinkIPCChannel))]
    public class OsuOpenLinkMessage
    {
        public string Link = null!;
    }
}
