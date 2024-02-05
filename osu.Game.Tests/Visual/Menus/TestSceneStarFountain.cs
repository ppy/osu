// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public partial class TestSceneStarFountain : OsuTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("make fountains", () =>
            {
                Children = new[]
                {
                    new StarFountain
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        X = 200,
                    },
                    new StarFountain
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        X = -200,
                    },
                };
            });
        }

        [Test]
        public void TestPew()
        {
            AddRepeatStep("activate fountains sometimes", () =>
            {
                foreach (var fountain in Children.OfType<StarFountain>())
                {
                    if (RNG.NextSingle() > 0.8f)
                        fountain.Shoot(RNG.Next(-1, 2));
                }
            }, 150);
        }
    }
}
