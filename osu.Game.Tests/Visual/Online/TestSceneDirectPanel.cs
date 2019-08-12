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

        private BeatmapSetInfo getUndownloadableBeatmapSet(RulesetInfo ruleset) => new BeatmapSetInfo
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
                    Ruleset = ruleset,
                    Version = "Test",
                    StarDifficulty = 6.42,
                }
            }
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            var ruleset = new OsuRuleset().RulesetInfo;

            var normal = CreateWorkingBeatmap(ruleset).BeatmapSetInfo;
            normal.OnlineInfo.HasVideo = true;
            normal.OnlineInfo.HasStoryboard = true;

            var undownloadable = getUndownloadableBeatmapSet(ruleset);

            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(20),
                    Spacing = new Vector2(0, 20),
                    Children = new Drawable[]
                    {
                        new DirectGridPanel(normal),
                        new DirectListPanel(normal),
                        new DirectGridPanel(undownloadable),
                        new DirectListPanel(undownloadable),
                    },
                },
            };
        }
    }
}
