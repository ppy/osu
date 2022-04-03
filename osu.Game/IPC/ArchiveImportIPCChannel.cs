// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Platform;

#nullable enable

namespace osu.Game.IPC.Channels
{
    public class ArchiveImportIPCChannel : LoadableIpcChannel<ArchiveImportMessage>
    {
        [Resolved(CanBeNull = true)]
        private OsuGame? game { get; set; }

        protected override IpcMessage? HandleMessage(ArchiveImportMessage msg)
        {
            Debug.Assert(game != null);

            ImportAsync(msg.Path).ContinueWith(t =>
            {
                if (t.Exception != null) throw t.Exception;
            }, TaskContinuationOptions.OnlyOnFaulted);

            return null;
        }

        public async Task ImportAsync(string path)
        {
            if (game == null)
            {
                // we want to contact a remote osu! to handle the import.
                await Ipc.SendMessageAsync(new ArchiveImportMessage { Path = path }).ConfigureAwait(false);
                return;
            }

            if (game.HandledExtensions.Contains(Path.GetExtension(path)?.ToLowerInvariant()))
                await game.Import(path).ConfigureAwait(false);
        }
    }

    [IpcMessage(typeof(ArchiveImportIPCChannel))]
    public class ArchiveImportMessage
    {
        public string Path = null!;
    }
}
