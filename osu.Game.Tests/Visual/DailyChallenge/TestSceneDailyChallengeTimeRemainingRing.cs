// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.DailyChallenge;

namespace osu.Game.Tests.Visual.DailyChallenge
{
    public partial class TestSceneDailyChallengeTimeRemainingRing : OsuTestScene
    {
        private readonly Bindable<Room> room = new Bindable<Room>(new Room());

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) => new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent))
        {
            Model = { BindTarget = room }
        };

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [Test]
        public void TestBasicAppearance()
        {
            DailyChallengeTimeRemainingRing ring = null!;

            AddStep("create content", () => Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                ring = new DailyChallengeTimeRemainingRing
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
            AddSliderStep("adjust width", 0.1f, 1, 1, width =>
            {
                if (ring.IsNotNull())
                    ring.Width = width;
            });
            AddSliderStep("adjust height", 0.1f, 1, 1, height =>
            {
                if (ring.IsNotNull())
                    ring.Height = height;
            });
            AddStep("just started", () =>
            {
                room.Value.StartDate.Value = DateTimeOffset.Now.AddMinutes(-1);
                room.Value.EndDate.Value = room.Value.StartDate.Value.Value.AddDays(1);
            });
            AddStep("midway through", () =>
            {
                room.Value.StartDate.Value = DateTimeOffset.Now.AddHours(-12);
                room.Value.EndDate.Value = room.Value.StartDate.Value.Value.AddDays(1);
            });
            AddStep("nearing end", () =>
            {
                room.Value.StartDate.Value = DateTimeOffset.Now.AddDays(-1).AddMinutes(8);
                room.Value.EndDate.Value = room.Value.StartDate.Value.Value.AddDays(1);
            });
            AddStep("already ended", () =>
            {
                room.Value.StartDate.Value = DateTimeOffset.Now.AddDays(-2);
                room.Value.EndDate.Value = room.Value.StartDate.Value.Value.AddDays(1);
            });
            AddSliderStep("manual progress", 0f, 1f, 0f, progress =>
            {
                var startedTimeAgo = TimeSpan.FromHours(24) * progress;
                room.Value.StartDate.Value = DateTimeOffset.Now - startedTimeAgo;
                room.Value.EndDate.Value = room.Value.StartDate.Value.Value.AddDays(1);
            });
        }
    }
}
