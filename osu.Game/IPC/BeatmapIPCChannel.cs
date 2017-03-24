// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.IPC
{
    public class BeatmapIPCChannel : IpcChannel<BeatmapImportMessage>
    {
        private readonly BeatmapDatabase beatmaps;

        public BeatmapIPCChannel(IIpcHost host, BeatmapDatabase beatmaps = null)
            : base(host)
        {
            this.beatmaps = beatmaps;
            MessageReceived += msg =>
            {
                Debug.Assert(beatmaps != null);
                ImportAsync(msg.Path).ContinueWith(t =>
                {
                    if (t.Exception != null) throw t.Exception;
                }, TaskContinuationOptions.OnlyOnFaulted);
            };
        }

        public async Task ImportAsync(string path)
        {
            if (beatmaps == null)
            {
                //we want to contact a remote osu! to handle the import.
                await SendMessageAsync(new BeatmapImportMessage { Path = path });
                return;
            }

            beatmaps.Import(path);
        }
    }

    public class BeatmapImportMessage
    {
        public string Path;
    }
}
