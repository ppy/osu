// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IPC;
using osu.Game.Rulesets.Scoring.Legacy;

namespace osu.Game.Rulesets.Scoring
{
    public class ScoreStore : DatabaseBackedStore, ICanAcceptFiles
    {
        private readonly BeatmapManager beatmaps;
        private readonly RulesetStore rulesets;

        private const string replay_folder = @"replays";

        public event Action<Score> ScoreImported;

        // ReSharper disable once NotAccessedField.Local (we should keep a reference to this so it is not finalised)
        private ScoreIPCChannel ipc;

        public ScoreStore(DatabaseContextFactory factory, IIpcHost importHost = null, BeatmapManager beatmaps = null, RulesetStore rulesets = null) : base(factory)
        {
            this.beatmaps = beatmaps;
            this.rulesets = rulesets;

            if (importHost != null)
                ipc = new ScoreIPCChannel(importHost, this);
        }

        public string[] HandledExtensions => new[] { ".osr" };

        public void Import(params string[] paths)
        {
            foreach (var path in paths)
            {
                var score = ReadReplayFile(path);
                if (score != null)
                    ScoreImported?.Invoke(score);
            }
        }

        public Score ReadReplayFile(string replayFilename)
        {
            if (File.Exists(replayFilename))
            {
                using (var stream = File.OpenRead(replayFilename))
                    return new DatabasedLegacyScoreParser(rulesets, beatmaps).Parse(stream);
            }

            Logger.Log($"Replay file {replayFilename} cannot be found", LoggingTarget.Information, LogLevel.Error);
            return null;
        }
    }
}
