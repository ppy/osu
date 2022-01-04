// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            var firstScore = new APIScore
            {
                PP = 1047.21,
                Rank = ScoreRank.SH,
                Beatmap = new APIBeatmap
                {
                    BeatmapSet = new APIBeatmapSet
                    {
                        Title = "JUSTadICE (TV Size)",
                        Artist = "Oomori Seiko",
                    },
                    DifficultyName = "Extreme"
                },
                Date = DateTimeOffset.Now,
                Mods = new[]
                {
                    new APIMod { Acronym = new OsuModHidden().Acronym },
                    new APIMod { Acronym = new OsuModHardRock().Acronym },
                    new APIMod { Acronym = new OsuModDoubleTime().Acronym },
                },
                Accuracy = 0.9813
            };

            var secondScore = new APIScore
            {
                PP = 134.32,
                Rank = ScoreRank.A,
                Beatmap = new APIBeatmap
                {
                    BeatmapSet = new APIBeatmapSet
                    {
                        Title = "Triumph & Regret",
                        Artist = "typeMARS",
                    },
                    DifficultyName = "[4K] Regret"
                },
                Date = DateTimeOffset.Now,
                Mods = new[]
                {
                    new APIMod { Acronym = new OsuModHardRock().Acronym },
                    new APIMod { Acronym = new OsuModDoubleTime().Acronym },
                },
                Accuracy = 0.998546
            };

            var thirdScore = new APIScore
            {
                PP = 96.83,
                Rank = ScoreRank.S,
                Beatmap = new APIBeatmap
                {
                    BeatmapSet = new APIBeatmapSet
                    {
                        Title = "Idolize",
                        Artist = "Creo",
                    },
                    DifficultyName = "Insane"
                },
                Date = DateTimeOffset.Now,
                Accuracy = 0.9726
            };

            var noPPScore = new APIScore
            {
                Rank = ScoreRank.B,
                Beatmap = new APIBeatmap
                {
                    BeatmapSet = new APIBeatmapSet
                    {
                        Title = "C18H27NO3(extend)",
                        Artist = "Team Grimoire",
                    },
                    DifficultyName = "[4K] Cataclysmic Hypernova"
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
