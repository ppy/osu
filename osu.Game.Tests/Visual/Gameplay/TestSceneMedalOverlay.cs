// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.Notifications.WebSocket;
using osu.Game.Online.Notifications.WebSocket.Events;
using osu.Game.Overlays;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneMedalOverlay : OsuManualInputManagerTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private MedalOverlay overlay = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create overlay", () => Child = overlay = new MedalOverlay());
        }

        [Test]
        public void TestBasicAward()
        {
            awardMedal(new UserAchievementUnlock
            {
                Title = "Time And A Half",
                Description = "Having a right ol' time. One and a half of them, almost.",
                Slug = @"all-intro-doubletime"
            });
            AddUntilStep("overlay shown", () => overlay.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddRepeatStep("dismiss", () => InputManager.Key(Key.Escape), 2);
            AddUntilStep("overlay hidden", () => overlay.State.Value, () => Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void TestMultipleMedalsInQuickSuccession()
        {
            awardMedal(new UserAchievementUnlock
            {
                Title = "Time And A Half",
                Description = "Having a right ol' time. One and a half of them, almost.",
                Slug = @"all-intro-doubletime"
            });
            awardMedal(new UserAchievementUnlock
            {
                Title = "S-Ranker",
                Description = "Accuracy is really underrated.",
                Slug = @"all-secret-rank-s"
            });
            awardMedal(new UserAchievementUnlock
            {
                Title = "500 Combo",
                Description = "500 big ones! You're moving up in the world!",
                Slug = @"osu-combo-500"
            });
        }

        private void awardMedal(UserAchievementUnlock unlock) => AddStep("award medal", () => dummyAPI.NotificationsClient.Receive(new SocketMessage
        {
            Event = @"new",
            Data = JObject.FromObject(new NewPrivateNotificationEvent
            {
                Name = @"user_achievement_unlock",
                Details = JObject.FromObject(unlock)
            })
        }));
    }
}
