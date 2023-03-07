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

        public async Task<bool?> SendAsync(string cwd, string[] args)
        {
            OsuInstanceIPCMessage? response = await SendMessageWithResponseAsync(new OsuInstanceIPCMessage(cwd, args)).ConfigureAwait(false);
            return response?.Handled;
        }
    }

    public class OsuInstanceIPCMessage
    {
        public readonly string Cwd;
        public readonly string[] Args;
        public readonly bool Handled;

        public OsuInstanceIPCMessage(string cwd, string[] args)
        {
            Cwd = cwd;
            Args = args;
        }

        public OsuInstanceIPCMessage(bool handled)
        {
            Cwd = null!;
            Args = null!;
            Handled = handled;
        }
    }
}
