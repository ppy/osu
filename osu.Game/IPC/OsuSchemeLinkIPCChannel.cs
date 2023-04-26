// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Platform;
using osu.Game.Online;

namespace osu.Game.IPC
{
    public class OsuSchemeLinkIPCChannel : IpcChannel<OsuSchemeLinkMessage>
    {
        private readonly ILinkHandler? linkHandler;

        public OsuSchemeLinkIPCChannel(IIpcHost host, ILinkHandler? linkHandler = null)
            : base(host)
        {
            this.linkHandler = linkHandler;

            MessageReceived += msg =>
            {
                Debug.Assert(linkHandler != null);
                linkHandler.HandleLink(msg.Link);
                return null;
            };
        }

        public async Task HandleLinkAsync(string url)
        {
            if (linkHandler == null)
            {
                await SendMessageAsync(new OsuSchemeLinkMessage(url)).ConfigureAwait(false);
                return;
            }

            linkHandler.HandleLink(url);
        }
    }

    public class OsuSchemeLinkMessage
    {
        public string Link { get; }

        public OsuSchemeLinkMessage(string link)
        {
            Link = link;
        }
    }
}
