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
using osu.Game.Users;
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

        private BeatmapSetInfo getUndownloadableBeatmapSet() => new BeatmapSetInfo
        {
            OnlineBeatmapSetID = 123,
            Metadata = new BeatmapMetadata
            {
                Title = "undownloadable beatmap",
                Artist = "test",
                Source = "more tests",
                Author = new User
                {
                    Username = "BanchoBot",
                    Id = 3,
                },
            },
            OnlineInfo = new BeatmapSetOnlineInfo
            {
                Availability = new BeatmapSetOnlineAvailability
                {
                    DownloadDisabled = true,
                },
                Preview = @"https://b.ppy.sh/preview/12345.mp3",
                PlayCount = 123,
                FavouriteCount = 456,
                BPM = 111,
                HasVideo = true,
                HasStoryboard = true,
                Covers = new BeatmapSetOnlineCovers(),
            },
            Beatmaps = new List<BeatmapInfo>
            {
                new BeatmapInfo
                {
                    Ruleset = Ruleset.Value,
                    Version = "Test",
                    StarDifficulty = 6.42,
                }
            }
        };

        private BeatmapSetInfo getManyDifficultiesBeatmapSet(RulesetStore rulesets)
        {
            var beatmaps = new List<BeatmapInfo>();

            for (int i = 0; i < 100; i++)
            {
                beatmaps.Add(new BeatmapInfo
                {
                    Ruleset = rulesets.GetRuleset(i % 4),
                    StarDifficulty = 2 + i % 4 * 2,
                    BaseDifficulty = new BeatmapDifficulty
                    {
                        OverallDifficulty = 3.5f,
                    }
                });
            }

            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = 1,
                Metadata = new BeatmapMetadata
                {
                    Title = "many difficulties beatmap",
                    Artist = "test",
                    Author = new User
                    {
                        Username = "BanchoBot",
                        Id = 3,
                    }
                },
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    HasVideo = true,
                    HasStoryboard = true,
                    Covers = new BeatmapSetOnlineCovers(),
                },
                Beatmaps = beatmaps,
            };
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            var normal = CreateWorkingBeatmap(Ruleset.Value).BeatmapSetInfo;
            normal.OnlineInfo.HasVideo = true;
            normal.OnlineInfo.HasStoryboard = true;

            var undownloadable = getUndownloadableBeatmapSet();
            var manyDifficulties = getManyDifficultiesBeatmapSet(rulesets);

            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Full,
                    Padding = new MarginPadding(20),
                    Spacing = new Vector2(5, 20),
                    Children = new Drawable[]
                    {
                        new DirectGridPanel(normal),
                        new DirectGridPanel(undownloadable),
                        new DirectGridPanel(manyDifficulties),
                        new DirectListPanel(normal),
                        new DirectListPanel(undownloadable),
                        new DirectListPanel(manyDifficulties),
                    },
                },
            };
        }
    }
}
