// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneUserPanel : OsuTestScene
    {
        private readonly Bindable<UserActivity> activity = new Bindable<UserActivity>();

        private UserPanel peppy;

        [Resolved]
        private RulesetStore rulesetStore { get; set; }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            UserPanel flyte;

            Child = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(10f),
                Children = new[]
                {
                    flyte = new UserPanel(new User
                    {
                        Username = @"flyte",
                        Id = 3103765,
                        Country = new Country { FlagName = @"JP" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
                    }) { Width = 300 },
                    peppy = new UserPanel(new User
                    {
                        Username = @"peppy",
                        Id = 2,
                        Country = new Country { FlagName = @"AU" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                        IsSupporter = true,
                        SupportLevel = 3,
                    }) { Width = 300 },
                },
            };

            flyte.Status.Value = new UserStatusOnline();
            peppy.Status.Value = null;
            peppy.Activity.BindTo(activity);
        });

        [Test]
        public void TestUserStatus()
        {
            AddStep("online", () => peppy.Status.Value = new UserStatusOnline());
            AddStep("do not disturb", () => peppy.Status.Value = new UserStatusDoNotDisturb());
            AddStep("offline", () => peppy.Status.Value = new UserStatusOffline());
            AddStep("null status", () => peppy.Status.Value = null);
        }

        [Test]
        public void TestUserActivity()
        {
            AddStep("set online status", () => peppy.Status.Value = new UserStatusOnline());

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

        private UserActivity soloGameStatusForRuleset(int rulesetId) => new UserActivity.SoloGame(null, rulesetStore.GetRuleset(rulesetId));
    }
}
