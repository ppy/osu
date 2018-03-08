// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile.Sections;
using osu.Game.Overlays.Profile.Sections.Recent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseUserProfileRecentSection : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RecentSection),
            typeof(DrawableRecentActivity),
            typeof(PaginatedRecentActivityContainer),
            typeof(MedalIcon)
        };

        public TestCaseUserProfileRecentSection()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f)
                },
                new ScrollContainer
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

        private IEnumerable<RecentActivity> createDummyActivities()
        {
            var dummyBeatmap = new RecentActivity.RecentActivityBeatmap
            {
                Title = @"Dummy beatmap",
                Url = "/b/1337",
            };

            var dummyUser = new RecentActivity.RecentActivityUser
            {
                Username = "DummyReborn",
                Url = "/u/666",
                PreviousUsername = "Dummy",
            };

            return new[]
            {
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.Achievement,
                    Achievement = new RecentActivity.RecentActivityAchievement
                    {
                        Name = @"Feelin' It",
                        Slug = @"all-secret-feelinit",
                    },
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapPlaycount,
                    Count = 1337,
                    Beatmap = dummyBeatmap,
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapsetApprove,
                    Approval = BeatmapApproval.Qualified,
                    Beatmapset = dummyBeatmap,
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapsetDelete,
                    Beatmapset = dummyBeatmap,
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapsetRevive,
                    Beatmapset = dummyBeatmap,
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapsetRevive,
                    Beatmapset = dummyBeatmap,
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapsetUpdate,
                    Beatmapset = dummyBeatmap,
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.BeatmapsetUpload,
                    Beatmapset = dummyBeatmap,
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.Rank,
                    Rank = 1,
                    Mode = "osu!",
                    Beatmap = dummyBeatmap,
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.RankLost,
                    Mode = "osu!",
                    Beatmap = dummyBeatmap,
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.UsernameChange,
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.UserSupportAgain,
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.UserSupportFirst,
                },
                new RecentActivity
                {
                    User = dummyUser,
                    Type = RecentActivityType.UserSupportGift,
                },
            };
        }
    }
}
