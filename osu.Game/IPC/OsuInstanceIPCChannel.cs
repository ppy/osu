// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Platform;

namespace osu.Game.IPC
{
    public class OsuInstanceIPCChannel : IpcChannel<OsuInstanceIPCMessage>
    {
        public OsuInstanceIPCChannel(IIpcHost host)
            : base(host)
        {
        }

        public async Task<bool?> InstanceAsync(string cwd, string[] args)
        {
            OsuInstanceIPCMessage? response = await SendMessageWithResponseAsync(new OsuInstanceIPCMessage { Cwd = cwd, Args = args }).ConfigureAwait(false);
            return response?.Handled;
        }
    }

    public class OsuInstanceIPCMessage
    {
        public string Cwd = "";
        public string[]? Args;
        public bool Handled;
    }
}
