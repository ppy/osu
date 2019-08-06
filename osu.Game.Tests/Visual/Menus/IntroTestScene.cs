// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public abstract class IntroTestScene : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(StartupScreen),
            typeof(IntroScreen),
            typeof(OsuScreen),
            typeof(IntroTestScene),
        };

        [Cached]
        private OsuLogo logo;

        protected IntroTestScene()
        {
            Drawable introStack = null;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                    Colour = Color4.Black,
                },
                logo = new OsuLogo
                {
                    Alpha = 0,
                    RelativePositionAxes = Axes.Both,
                    Depth = float.MinValue,
                    Position = new Vector2(0.5f),
                }
            };

            AddStep("restart sequence", () =>
            {
                logo.FinishTransforms();
                logo.IsTracking = false;

                introStack?.Expire();

                Add(introStack = new OsuScreenStack(CreateScreen())
                {
                    RelativeSizeAxes = Axes.Both,
                });
            });
        }

        protected abstract IScreen CreateScreen();
    }
}
