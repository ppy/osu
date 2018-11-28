// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Rulesets;

namespace osu.Game.Scoring
{
    public class ScoreManager : ArchiveModelManager<Score, ScoreFileInfo>
    {
        public override string[] HandledExtensions => new[] { ".osr" };

        protected override string ImportFromStablePath => "Replays";

        public ScoreManager(RulesetStore rulesets, BeatmapManager beatmaps, Storage storage, IDatabaseContextFactory contextFactory, IIpcHost importHost = null)
            : base(storage, contextFactory, new ScoreStore(contextFactory, storage), importHost)
        {
        }

        protected override Score CreateModel(ArchiveReader archive) => new Score();

        protected override void Populate(Score model, ArchiveReader archive)
        {
            if (archive == null)
                return;
        }
    }
}
