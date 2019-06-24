// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneUserPanel : OsuTestScene
    {
        private readonly UserPanel peppy;

        public TestSceneUserPanel()
        {
            UserPanel flyte;

            Add(new FillFlowContainer
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
            });

            flyte.Status.Value = new UserStatusOnline();
            peppy.Status.Value = null;
        }

        [Test]
        public void UserStatusesTests()
        {
            AddStep("online", () => { peppy.Status.Value = new UserStatusOnline(); });
            AddStep(@"do not disturb", () => { peppy.Status.Value = new UserStatusDoNotDisturb(); });
            AddStep(@"offline", () => { peppy.Status.Value = new UserStatusOffline(); });
            AddStep(@"null status", () => { peppy.Status.Value = null; });
        }

        [Test]
        public void UserActivitiesTests()
        {
            Bindable<UserActivity> activity = new Bindable<UserActivity>();

            peppy.Activity.BindTo(activity);

            AddStep("idle", () => { activity.Value = null; });
            AddStep("spectating", () => { activity.Value = new UserActivity.Spectating(); });
            AddStep("solo", () => { activity.Value = new UserActivity.SoloGame(null, null); });
            AddStep("choosing", () => { activity.Value = new UserActivity.ChoosingBeatmap(); });
            AddStep("editing", () => { activity.Value = new UserActivity.Editing(null); });
            AddStep("modding", () => { activity.Value = new UserActivity.Modding(); });
        }
    }
}
