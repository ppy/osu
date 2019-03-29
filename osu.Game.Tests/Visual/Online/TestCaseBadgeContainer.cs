// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestCaseBadgeContainer : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(BadgeContainer) };

        public TestCaseBadgeContainer()
        {
            BadgeContainer badgeContainer;

            Child = badgeContainer = new BadgeContainer
            {
                RelativeSizeAxes = Axes.Both
            };

            AddStep("Show 1 badge", () => badgeContainer.ShowBadges(new[]
            {
                new Badge
                {
                    AwardedAt = DateTimeOffset.Now,
                    Description = "Appreciates compasses",
                    ImageUrl = "https://assets.ppy.sh/profile-badges/mg2018-1star.png",
                }
            }));

            AddStep("Show 2 badges", () => badgeContainer.ShowBadges(new[]
            {
                new Badge
                {
                    AwardedAt = DateTimeOffset.Now,
                    Description = "Contributed to osu!lazer testing",
                    ImageUrl = "https://assets.ppy.sh/profile-badges/contributor.png",
                },
                new Badge
                {
                    AwardedAt = DateTimeOffset.Now,
                    Description = "Appreciates compasses",
                    ImageUrl = "https://assets.ppy.sh/profile-badges/mg2018-1star.png",
                }
            }));

            AddStep("Show many badges", () => badgeContainer.ShowBadges(Enumerable.Range(1, 20).Select(i => new Badge
            {
                AwardedAt = DateTimeOffset.Now,
                Description = $"Contributed to osu!lazer testing {i} times",
                ImageUrl = "https://assets.ppy.sh/profile-badges/contributor.jpg",
            }).ToArray()));
        }
    }
}
