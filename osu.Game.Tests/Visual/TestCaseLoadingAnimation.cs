﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseLoadingAnimation : GridTestCase
    {
        public TestCaseLoadingAnimation()
            : base(2, 2)
        {
            LoadingAnimation loading;

            Cell(0).AddRange(new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both
                },
                loading = new LoadingAnimation()
            });

            loading.Show();

            Cell(1).AddRange(new Drawable[]
            {
                new Box
                {
                    Colour = Color4.White,
                    RelativeSizeAxes = Axes.Both
                },
                loading = new LoadingAnimation()
            });

            loading.Show();

            Cell(2).AddRange(new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Gray,
                    RelativeSizeAxes = Axes.Both
                },
                loading = new LoadingAnimation()
            });

            loading.Show();

            Cell(3).AddRange(new Drawable[]
            {
                loading = new LoadingAnimation()
            });

            Scheduler.AddDelayed(() => loading.ToggleVisibility(), 200, true);
        }
    }
}
