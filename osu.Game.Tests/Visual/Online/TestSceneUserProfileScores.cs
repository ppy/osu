// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Overlays.Profile.Sections;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Framework.Graphics;
using osu.Game.Scoring;
using osu.Framework.Graphics.Containers;
using osuTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneUserProfileScores : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ProfileScore),
            typeof(ProfileWeightedScore),
            typeof(ProfileItemBackground),
        };

        public TestSceneUserProfileScores()
        {
            var score = new ScoreInfo
            {
                PP = 134.32,
                Rank = ScoreRank.A,
                Beatmap = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = "Triumph & Regret",
                        Artist = "typeMARS"
                    },
                    Version = "[4K] Regret"
                },
                Date = DateTimeOffset.Now,
                Mods = new Mod[]
                {
                    new OsuModHardRock(),
                    new OsuModDoubleTime(),
                },
                Accuracy = 0.998546
            };

            var noPPScore = new ScoreInfo
            {
                Rank = ScoreRank.B,
                Beatmap = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = "C18H27NO3(extend)",
                        Artist = "Team Grimoire"
                    },
                    Version = "[4K] Cataclysmic Hypernova"
                },
                Date = DateTimeOffset.Now,
                Accuracy = 0.55879
            };

            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new[]
                {
                    new ProfileScore(score),
                    new ProfileScore(noPPScore),
                    new ProfileWeightedScore(score, 0.85),
                }
            });
        }
    }
}
