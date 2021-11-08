// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneUserPanel : OsuTestScene
    {
        private readonly Bindable<UserActivity> activity = new Bindable<UserActivity>();
        private readonly Bindable<UserStatus> status = new Bindable<UserStatus>();

        private UserGridPanel peppy;
        private TestUserListPanel evast;

        [Resolved]
        private RulesetStore rulesetStore { get; set; }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            UserGridPanel flyte;

            activity.Value = null;
            status.Value = null;

            Child = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Spacing = new Vector2(10f),
                Children = new Drawable[]
                {
                    new UserBrickPanel(new APIUser
                    {
                        Username = @"flyte",
                        Id = 3103765,
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
                    }),
                    new UserBrickPanel(new APIUser
                    {
                        Username = @"peppy",
                        Id = 2,
                        Colour = "99EB47",
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    }),
                    flyte = new UserGridPanel(new APIUser
                    {
                        Username = @"flyte",
                        Id = 3103765,
                        Country = new Country { FlagName = @"JP" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
                    }) { Width = 300 },
                    peppy = new UserGridPanel(new APIUser
                    {
                        Username = @"peppy",
                        Id = 2,
                        Country = new Country { FlagName = @"AU" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                        IsSupporter = true,
                        SupportLevel = 3,
                    }) { Width = 300 },
                    evast = new TestUserListPanel(new APIUser
                    {
                        Username = @"Evast",
                        Id = 8195163,
                        Country = new Country { FlagName = @"BY" },
                        CoverUrl = @"https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                        IsOnline = false,
                        LastVisit = DateTimeOffset.Now
                    })
                },
            };

            flyte.Status.Value = new UserStatusOnline();

            peppy.Status.BindTo(status);
            peppy.Activity.BindTo(activity);

            evast.Status.BindTo(status);
            evast.Activity.BindTo(activity);
        });

        [Test]
        public void TestUserStatus()
        {
            AddStep("online", () => status.Value = new UserStatusOnline());
            AddStep("do not disturb", () => status.Value = new UserStatusDoNotDisturb());
            AddStep("offline", () => status.Value = new UserStatusOffline());
            AddStep("null status", () => status.Value = null);
        }

        [Test]
        public void TestUserActivity()
        {
            AddStep("set online status", () => status.Value = new UserStatusOnline());

            AddStep("idle", () => activity.Value = null);
            AddStep("spectating", () => activity.Value = new UserActivity.Spectating());
            AddStep("solo (osu!)", () => activity.Value = soloGameStatusForRuleset(0));
            AddStep("solo (osu!taiko)", () => activity.Value = soloGameStatusForRuleset(1));
            AddStep("solo (osu!catch)", () => activity.Value = soloGameStatusForRuleset(2));
            AddStep("solo (osu!mania)", () => activity.Value = soloGameStatusForRuleset(3));
            AddStep("choosing", () => activity.Value = new UserActivity.ChoosingBeatmap());
            AddStep("editing", () => activity.Value = new UserActivity.Editing(null));
            AddStep("modding", () => activity.Value = new UserActivity.Modding());
        }

        [Test]
        public void TestUserActivityChange()
        {
            AddAssert("visit message is visible", () => evast.LastVisitMessage.IsPresent);
            AddStep("set online status", () => status.Value = new UserStatusOnline());
            AddAssert("visit message is not visible", () => !evast.LastVisitMessage.IsPresent);
            AddStep("set choosing activity", () => activity.Value = new UserActivity.ChoosingBeatmap());
            AddStep("set offline status", () => status.Value = new UserStatusOffline());
            AddAssert("visit message is visible", () => evast.LastVisitMessage.IsPresent);
            AddStep("set online status", () => status.Value = new UserStatusOnline());
            AddAssert("visit message is not visible", () => !evast.LastVisitMessage.IsPresent);
        }

        private UserActivity soloGameStatusForRuleset(int rulesetId) => new UserActivity.InSoloGame(null, rulesetStore.GetRuleset(rulesetId));

        private class TestUserListPanel : UserListPanel
        {
            public TestUserListPanel(APIUser user)
                : base(user)
            {
            }

            public new TextFlowContainer LastVisitMessage => base.LastVisitMessage;
        }
    }
}
