// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public partial class TestSceneStarFountain : OsuTestScene
    {
        [Test]
        public void TestMenu()
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

            AddRepeatStep("activate fountains sometimes", () =>
            {
                foreach (var fountain in Children.OfType<StarFountain>())
                {
                    if (RNG.NextSingle() > 0.8f)
                        fountain.Shoot(RNG.Next(-1, 2));
                }
            }, 150);
        }

        [Test]
        public void TestGameplay()
        {
            KiaiGameplayFountains fountains = null!;

            AddStep("make fountains", () =>
            {
                Children = new[]
                {
                    fountains = new KiaiGameplayFountains(),
                };
            });

            AddStep("activate fountains", () => fountains.Shoot());
        }

        [Test]
        public void TestGameplayStarFountainsSetting()
        {
            Bindable<bool> starFountainsEnabled = null!;

            AddStep("load configuration", () =>
            {
                var config = new OsuConfigManager(LocalStorage);
                starFountainsEnabled = config.GetBindable<bool>(OsuSetting.StarFountains);
            });

            AddStep("make fountains", () =>
            {
                Children = new Drawable[]
                {
                    new KiaiGameplayFountains.GameplayStarFountain
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        X = 75,
                    },
                    new KiaiGameplayFountains.GameplayStarFountain
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        X = -75,
                    },
                };
            });

            AddStep("enable KiaiStarEffects", () => starFountainsEnabled.Value = true);
            AddRepeatStep("activate fountains (enabled)", () =>
            {
                ((KiaiGameplayFountains.GameplayStarFountain)Children[0]).Shoot(1);
                ((KiaiGameplayFountains.GameplayStarFountain)Children[1]).Shoot(-1);
            }, 100);

            AddStep("disable KiaiStarEffects", () => starFountainsEnabled.Value = false);
            AddRepeatStep("attempt to activate fountains (disabled)", () =>
            {
                ((KiaiGameplayFountains.GameplayStarFountain)Children[0]).Shoot(1);
                ((KiaiGameplayFountains.GameplayStarFountain)Children[1]).Shoot(-1);
            }, 100);

            AddStep("re-enable KiaiStarEffects", () => starFountainsEnabled.Value = true);
            AddRepeatStep("activate fountains (re-enabled)", () =>
            {
                ((KiaiGameplayFountains.GameplayStarFountain)Children[0]).Shoot(1);
                ((KiaiGameplayFountains.GameplayStarFountain)Children[1]).Shoot(-1);
            }, 100);
        }
    }
}
