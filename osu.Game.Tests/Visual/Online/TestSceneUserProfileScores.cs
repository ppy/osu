// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        public TestSceneUserProfileScores()
        {
            var firstScore = new ScoreInfo
            {
                PP = 1047.21,
                Rank = ScoreRank.SH,
                Beatmap = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = "JUSTadICE (TV Size)",
                        Artist = "Oomori Seiko"
                    },
                    Version = "Extreme"
                },
                Date = DateTimeOffset.Now,
                Mods = new Mod[]
                {
                    new OsuModHidden(),
                    new OsuModHardRock(),
                    new OsuModDoubleTime()
                },
                Accuracy = 0.9813
            };

            var secondScore = new ScoreInfo
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

            var thirdScore = new ScoreInfo
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
                    new ColourProvidedContainer(OverlayColourScheme.Green, new DrawableProfileScore(firstScore)),
                    new ColourProvidedContainer(OverlayColourScheme.Green, new DrawableProfileScore(secondScore)),
                    new ColourProvidedContainer(OverlayColourScheme.Pink, new DrawableProfileScore(noPPScore)),
                    new ColourProvidedContainer(OverlayColourScheme.Pink, new DrawableProfileWeightedScore(firstScore, 0.97)),
                    new ColourProvidedContainer(OverlayColourScheme.Pink, new DrawableProfileWeightedScore(secondScore, 0.85)),
                    new ColourProvidedContainer(OverlayColourScheme.Pink, new DrawableProfileWeightedScore(thirdScore, 0.66)),
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
