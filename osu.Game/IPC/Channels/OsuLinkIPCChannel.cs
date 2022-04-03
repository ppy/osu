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
        [Resolved]
        private OsuGame handler { get; set; } = null!;

        protected override IpcMessage? HandleMessage(OsuOpenLinkMessage msg)
        {
            Debug.Assert(handler != null);

            handler.HandleLink(msg.Link);

            return null;
        }
    }

    [IpcMessage(typeof(OsuLinkIPCChannel))]
    public class OsuOpenLinkMessage
    {
        public string Link = null!;
    }
}
