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
using osu.Game.Overlays;
using osu.Framework.Allocation;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneUserProfileScores : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableProfileScore),
            typeof(DrawableProfileWeightedScore),
            typeof(ProfileItemContainer),
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

            var secondScore = new ScoreInfo
            {
                PP = 96.83,
                Rank = ScoreRank.S,
                Beatmap = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = "Idolize",
                        Artist = "Creo"
                    },
                    Version = "Insane"
                },
                Date = DateTimeOffset.Now,
                Accuracy = 0.9726
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
                    new ColourProvidedContainer(OverlayColourScheme.Green, new DrawableProfileScore(score)),
                    new ColourProvidedContainer(OverlayColourScheme.Pink, new DrawableProfileScore(noPPScore)),
                    new ColourProvidedContainer(OverlayColourScheme.Pink, new DrawableProfileWeightedScore(score, 0.85)),
                    new ColourProvidedContainer(OverlayColourScheme.Pink, new DrawableProfileWeightedScore(secondScore, 0.66)),
                }
            });
        }

        private class ColourProvidedContainer : Container
        {
            [Cached]
            private readonly OverlayColourProvider colourProvider;

            public ColourProvidedContainer(OverlayColourScheme colourScheme, DrawableProfileScore score)
            {
                colourProvider = new OverlayColourProvider(colourScheme);

                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
                Add(score);
            }
        }
    }
}
