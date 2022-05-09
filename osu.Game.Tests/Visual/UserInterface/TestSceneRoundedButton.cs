// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneRoundedButton : OsuTestScene
    {
        [Test]
        public void TestBasic()
        {
            RoundedButton button = null;

            AddStep("create button", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    button = new RoundedButton
                    {
                        Width = 400,
                        Text = "Test Button",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Action = () => { }
                    }
                }
            });

            AddToggleStep("toggle disabled", disabled => button.Action = disabled ? (Action)null : () => { });
        }

        [Test]
        public void TestOverlay()
        {
            IEnumerable<OverlayColourScheme> schemes = Enum.GetValues(typeof(OverlayColourScheme)).Cast<OverlayColourScheme>();

            AddStep("create buttons", () =>
            {
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5f),
                    ChildrenEnumerable = schemes.Select(c => new DependencyProvidingContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[] { (typeof(OverlayColourProvider), new OverlayColourProvider(c)) },
                        Child = new RoundedButton
                        {
                            Width = 400,
                            Text = $"Test {c}",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Action = () => { },
                        }
                    }),
                };
            });

            AddAssert("first button has correct colour", () => this.ChildrenOfType<RoundedButton>().First().BackgroundColour == new OverlayColourProvider(schemes.First()).Highlight1);
            AddToggleStep("toggle disabled", disabled => this.ChildrenOfType<RoundedButton>().ForEach(b => b.Action = disabled ? (Action)null : () => { }));
        }
    }
}
