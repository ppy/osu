// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneDirectPanel : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DirectGridPanel),
            typeof(DirectListPanel),
            typeof(IconPill)
        };

        private RulesetStore rulesets;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;

            var beatmap = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
            beatmap.BeatmapSetInfo.OnlineInfo.HasVideo = true;
            beatmap.BeatmapSetInfo.OnlineInfo.HasStoryboard = true;

            var manydiffBeatmap = createTestBeatmapSetWithManyDifficulties();

            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Full,
                    Padding = new MarginPadding(20),
                    Spacing = new Vector2(0, 20),
                    Children = new Drawable[]
                    {
                        new DirectGridPanel(beatmap.BeatmapSetInfo),
                        new DirectGridPanel(manydiffBeatmap),
                        new DirectListPanel(beatmap.BeatmapSetInfo),
                        new DirectListPanel(manydiffBeatmap),
                    }
                },
            };
        }

        private BeatmapSetInfo createTestBeatmapSetWithManyDifficulties()
        {
            var toReturn = new BeatmapSetInfo
            {
                OnlineBeatmapSetID = 1,
                Metadata = new BeatmapMetadata
                {
                    Artist = "peppy",
                    Title = "test set!",
                    AuthorString = "peppy",
                },
                OnlineInfo = new BeatmapSetOnlineInfo { Covers = new BeatmapSetOnlineCovers { Cover = "" }, },
                Beatmaps = new List<BeatmapInfo>(),
            };

            for (int b = 1; b < 101; b++)
            {
                toReturn.Beatmaps.Add(new BeatmapInfo
                {
                    OnlineBeatmapID = b * 10,
                    Path = $"extra{b}.osu",
                    Version = $"Extra {b}",
                    Ruleset = rulesets.GetRuleset(b % 4),
                    StarDifficulty = 2 + b % 4 * 2,
                    BaseDifficulty = new BeatmapDifficulty
                    {
                        OverallDifficulty = 3.5f,
                    }
                });
            }

            return toReturn;
        }
    }
}
