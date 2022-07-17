// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneUserProfileScores : OsuTestScene
    {
        public TestSceneUserProfileScores()
        {
            var firstScore = new SoloScoreInfo
            {
                PP = 1047.21,
                Rank = ScoreRank.SH,
                BeatmapID = 2058788,
                EndedAt = DateTimeOffset.Now,
                Mods = new[]
                {
                    new APIMod { Acronym = new OsuModHidden().Acronym },
                    new APIMod { Acronym = new OsuModHardRock().Acronym },
                    new APIMod { Acronym = new OsuModDoubleTime().Acronym },
                },
                Accuracy = 0.9813
            };
            var firstBeatmap = new APIBeatmap
            {
                BeatmapSet = new APIBeatmapSet()
                {
                    Title = "JUSTadICE (TV Size)",
                    Artist = "Oomori Seiko",
                },
                DifficultyName = "Extreme"
            };

            var secondScore = new SoloScoreInfo
            {
                PP = 134.32,
                Rank = ScoreRank.A,
                BeatmapID = 767046,
                EndedAt = DateTimeOffset.Now,
                Mods = new[]
                {
                    new APIMod { Acronym = new OsuModHardRock().Acronym },
                    new APIMod { Acronym = new OsuModDoubleTime().Acronym },
                },
                Accuracy = 0.998546
            };
            var secondBeatmap = new APIBeatmap
            {
                BeatmapSet = new APIBeatmapSet()
                {
                    Title = "Triumph & Regret",
                    Artist = "typeMARS",
                },
                DifficultyName = "[4K] Regret"
            };

            var thirdScore = new SoloScoreInfo
            {
                PP = 96.83,
                Rank = ScoreRank.S,
                BeatmapID = 2134713,
                EndedAt = DateTimeOffset.Now,
                Accuracy = 0.9726
            };
            var thirdBeatmap = new APIBeatmap
            {
                BeatmapSet = new APIBeatmapSet()
                {
                    Title = "Idolize",
                    Artist = "Creo",
                },
                DifficultyName = "Insane"
            };

            var noPPScore = new SoloScoreInfo
            {
                Rank = ScoreRank.B,
                BeatmapID = 992512,
                EndedAt = DateTimeOffset.Now,
                Accuracy = 0.55879
            };
            var noPPBeatmap = new APIBeatmap
            {
                BeatmapSet = new APIBeatmapSet()
                {
                    Title = "Galaxy Collapse",
                    Artist = "Kurokotei",
                },
                DifficultyName = "[4K] Cataclysmic Hypernova"
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
                    new ColourProvidedContainer(OverlayColourScheme.Green, new DrawableProfileScore(firstScore, firstBeatmap)),
                    new ColourProvidedContainer(OverlayColourScheme.Green, new DrawableProfileScore(secondScore, secondBeatmap)),
                    new ColourProvidedContainer(OverlayColourScheme.Pink, new DrawableProfileScore(noPPScore, noPPBeatmap)),
                    new ColourProvidedContainer(OverlayColourScheme.Pink, new DrawableProfileWeightedScore(firstScore, firstBeatmap, 0.97)),
                    new ColourProvidedContainer(OverlayColourScheme.Pink, new DrawableProfileWeightedScore(secondScore, secondBeatmap, 0.85)),
                    new ColourProvidedContainer(OverlayColourScheme.Pink, new DrawableProfileWeightedScore(thirdScore, thirdBeatmap, 0.66)),
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
