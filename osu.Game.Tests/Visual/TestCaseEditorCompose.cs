// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit.Screens.Compose;

namespace osu.Game.Tests.Visual
{
    public class TestCaseEditorCompose : OsuTestCase
    {
        private readonly Random random;
        private readonly Compose compose;

        public TestCaseEditorCompose()
        {
            random = new Random(1337);

            Add(compose = new Compose());
            AddStep("Next beatmap", nextBeatmap);
        }

        private OsuGameBase osuGame;
        private BeatmapManager beatmaps;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame, BeatmapManager beatmaps)
        {
            this.osuGame = osuGame;
            this.beatmaps = beatmaps;

            compose.Beatmap.BindTo(osuGame.Beatmap);
        }

        private void nextBeatmap()
        {
            var sets = beatmaps.GetAllUsableBeatmapSets();
            if (sets.Count == 0)
                return;

            var b = sets[random.Next(0, sets.Count)].Beatmaps[0];
            osuGame.Beatmap.Value = beatmaps.GetWorkingBeatmap(b);
        }
    }
}
