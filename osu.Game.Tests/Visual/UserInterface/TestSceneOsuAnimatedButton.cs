// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneOsuAnimatedButton : OsuGridTestScene
    {
        public TestSceneOsuAnimatedButton()
            : base(3, 2)
        {
            Cell(0).Add(new BaseContainer("relative sized")
            {
                RelativeSizeAxes = Axes.Both,
            });

            Cell(1).Add(new BaseContainer("auto sized")
            {
                AutoSizeAxes = Axes.Both
            });

            Cell(2).Add(new BaseContainer("relative Y auto X")
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X
            });

            Cell(3).Add(new BaseContainer("relative X auto Y")
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y
            });

            Cell(4).Add(new BaseContainer("fixed")
            {
                Size = new Vector2(100),
            });

            Cell(5).Add(new BaseContainer("fixed")
            {
                Size = new Vector2(100, 50),
            });

            AddToggleStep("toggle enabled", toggle =>
            {
                for (int i = 0; i < 6; i++)
                    ((BaseContainer)Cell(i).Child).Action = toggle ? () => { } : (Action)null;
            });
        }

        public class BaseContainer : OsuAnimatedButton
        {
            public BaseContainer(string text)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Add(new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = text
                });
            }
        }
    }
}
