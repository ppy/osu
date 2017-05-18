// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Direct;

namespace osu.Desktop.VisualTests.Tests
{
    public class TestCaseDirect : TestCase
    {
        public override string Description => @"osu!direct overlay";

        private DirectOverlay direct;
        private RulesetDatabase rulesets;

        public override void Reset()
        {
            base.Reset();

            Add(direct = new DirectOverlay());
            newBeatmaps();
            direct.ResultCounts = new ResultCounts(1, 432, 3);

            AddStep(@"Toggle", direct.ToggleVisibility);
        }

        [BackgroundDependencyLoader]
        private void load(RulesetDatabase rulesets)
        {
            this.rulesets = rulesets;
        }

        private void newBeatmaps()
        {
            var setInfo = new BeatmapSetInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Title = @"Platina",
                    Artist = @"Maaya Sakamoto",
                    Author = @"TicClick",
                    Source = @"Cardcaptor Sakura",
                },
                Beatmaps = new List<BeatmapInfo>(),
            };
            
            for (int i = 0; i < 4; i++)
            {
                setInfo.Beatmaps.Add(new BeatmapInfo
                {
                    Ruleset = rulesets.GetRuleset(i),
                    StarDifficulty = i + 1,
                });
            }

            var s = new List<BeatmapSetInfo>();
            for (int i = 0; i < 10; i++)
                s.Add(setInfo);

            direct.BeatmapSets = s;
        }
    }
}
