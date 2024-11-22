// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.DailyChallenge;
using osu.Game.Screens.OnlinePlay.DailyChallenge.Events;

namespace osu.Game.Tests.Visual.DailyChallenge
{
    public partial class TestSceneDailyChallengeCarousel : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        private readonly Bindable<Room> room = new Bindable<Room>(new Room());

        [Test]
        public void TestBasicAppearance()
        {
            DailyChallengeCarousel carousel = null!;

            AddStep("create content", () => Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                carousel = new DailyChallengeCarousel
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
            AddSliderStep("adjust width", 0.1f, 1, 1, width =>
            {
                if (carousel.IsNotNull())
                    carousel.Width = width;
            });
            AddSliderStep("adjust height", 0.1f, 1, 1, height =>
            {
                if (carousel.IsNotNull())
                    carousel.Height = height;
            });
            AddRepeatStep("add content", () => carousel.Add(new FakeContent()), 3);
        }

        [Test]
        public void TestIntegration()
        {
            GridContainer grid = null!;
            DailyChallengeEventFeed feed = null!;
            DailyChallengeScoreBreakdown breakdown = null!;

            AddStep("create content", () => Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                grid = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RowDimensions =
                    [
                        new Dimension(),
                        new Dimension()
                    ],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new DailyChallengeCarousel
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Children = new Drawable[]
                                {
                                    new DailyChallengeTimeRemainingRing(room.Value),
                                    breakdown = new DailyChallengeScoreBreakdown(),
                                }
                            }
                        },
                        [
                            feed = new DailyChallengeEventFeed
                            {
                                RelativeSizeAxes = Axes.Both,
                            }
                        ],
                    }
                },
            });
            AddSliderStep("adjust width", 0.1f, 1, 1, width =>
            {
                if (grid.IsNotNull())
                    grid.Width = width;
            });
            AddSliderStep("adjust height", 0.1f, 1, 1, height =>
            {
                if (grid.IsNotNull())
                    grid.Height = height;
            });
            AddSliderStep("update time remaining", 0f, 1f, 0f, progress =>
            {
                var startedTimeAgo = TimeSpan.FromHours(24) * progress;
                room.Value.StartDate = DateTimeOffset.Now - startedTimeAgo;
                room.Value.EndDate = room.Value.StartDate.Value.AddDays(1);
            });
            AddStep("add normal score", () =>
            {
                var ev = new NewScoreEvent(1, new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                    CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                }, RNG.Next(1_000_000), null);

                feed.AddNewScore(ev);
                breakdown.AddNewScore(ev);
            });
            AddStep("add new user best", () =>
            {
                var ev = new NewScoreEvent(1, new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                    CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                }, RNG.Next(1_000_000), RNG.Next(1, 1000));

                feed.AddNewScore(ev);
                breakdown.AddNewScore(ev);
            });
        }

        private partial class FakeContent : CompositeDrawable
        {
            private OsuSpriteText text = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Colour4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1),
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "Fake Content " + (char)('A' + RNG.Next(26)),
                    },
                };

                text.FadeOut(500, Easing.OutQuint)
                    .Then().FadeIn(500, Easing.OutQuint)
                    .Loop();
            }
        }
    }
}
