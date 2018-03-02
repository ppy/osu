// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Screens;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseEditor : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(Editor), typeof(EditorScreen) };

        private readonly Random rng;

        private BeatmapManager beatmaps;
        private OsuGameBase osuGame;

        public TestCaseEditor()
        {
            rng = new Random(1337);

            Add(new Editor());
            AddStep("Next beatmap", nextBeatmap);
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame, BeatmapManager beatmaps)
        {
            this.osuGame = osuGame;
            this.beatmaps = beatmaps;
        }

        private void nextBeatmap()
        {
            var sets = beatmaps.GetAllUsableBeatmapSets();
            if (sets.Count == 0)
                return;

            BeatmapInfo info = sets[rng.Next(0, sets.Count)].Beatmaps[0];
            osuGame.Beatmap.Value = beatmaps.GetWorkingBeatmap(info);
        }
    }
}
