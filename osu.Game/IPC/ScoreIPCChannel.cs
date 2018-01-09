// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Platform;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.IPC
{
    public class ScoreIPCChannel : IpcChannel<ScoreImportMessage>
    {
        private readonly ScoreStore scores;

        public ScoreIPCChannel(IIpcHost host, ScoreStore scores = null)
            : base(host)
        {
            this.scores = scores;
            MessageReceived += msg =>
            {
                Debug.Assert(scores != null);
                ImportAsync(msg.Path).ContinueWith(t =>
                {
                    if (t.Exception != null) throw t.Exception;
                }, TaskContinuationOptions.OnlyOnFaulted);
            };
        }

        public async Task ImportAsync(string path)
        {
            if (scores == null)
            {
                //we want to contact a remote osu! to handle the import.
                await SendMessageAsync(new ScoreImportMessage { Path = path });
                return;
            }

            scores.ReadReplayFile(path);
        }
    }

    public class ScoreImportMessage
    {
        public string Path;
    }
}
