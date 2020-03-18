// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile.Sections;
using osu.Game.Overlays.Profile.Sections.Recent;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneUserProfileRecentSection : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RecentSection),
            typeof(DrawableRecentActivity),
            typeof(PaginatedRecentActivityContainer),
            typeof(MedalIcon)
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        public TestSceneUserProfileRecentSection()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f)
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new FillFlowContainer<DrawableRecentActivity>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        ChildrenEnumerable = createDummyActivities().Select(a => new DrawableRecentActivity(a))
                    },
                }
            };
        }

        private IEnumerable<APIRecentActivity> createDummyActivities()
        {
            var dummyBeatmap = new APIRecentActivity.RecentActivityBeatmap
            {
                Title = @"Dummy beatmap",
                Url = "/b/1337",
            };

            var dummyUser = new APIRecentActivity.RecentActivityUser
            {
                Username = "DummyReborn",
                Url = "/u/666",
                PreviousUsername = "Dummy",
            };

            return new[]
            {
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.Achievement,
                    Achievement = new APIRecentActivity.RecentActivityAchievement
                    {
                        Name = @"Feelin' It",
                        Slug = @"all-secret-feelinit",
                    },
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapPlaycount,
                    Count = 1337,
                    Beatmap = dummyBeatmap,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapsetApprove,
                    Approval = BeatmapApproval.Qualified,
                    Beatmapset = dummyBeatmap,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapsetDelete,
                    Beatmapset = dummyBeatmap,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapsetRevive,
                    Beatmapset = dummyBeatmap,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapsetRevive,
                    Beatmapset = dummyBeatmap,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapsetUpdate,
                    Beatmapset = dummyBeatmap,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapsetUpload,
                    Beatmapset = dummyBeatmap,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.Rank,
                    Rank = 1,
                    Mode = "osu!",
                    Beatmap = dummyBeatmap,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.Rank,
                    Rank = 1,
                    Mode = "vitaru",
                    Beatmap = dummyBeatmap,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.Rank,
                    Rank = 1,
                    Mode = "fruits",
                    Beatmap = dummyBeatmap,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.RankLost,
                    Mode = "osu!",
                    Beatmap = dummyBeatmap,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.UsernameChange,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.UserSupportAgain,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.UserSupportFirst,
                },
                new APIRecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.UserSupportGift,
                },
            };
        }
    }
}
