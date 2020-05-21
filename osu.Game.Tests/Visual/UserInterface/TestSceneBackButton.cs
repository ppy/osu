// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneBackButton : OsuTestScene
    {
        public TestSceneBackButton()
        {
            BackButton button;
            BackButton.Receptor receptor = new BackButton.Receptor();

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(300),
                Masking = true,
                Children = new Drawable[]
                {
                    receptor,
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.SlateGray
                    },
                    button = new BackButton(receptor)
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                    }
                }
            };

            button.Action = () => button.Hide();

            AddStep("show button", () => button.Show());
            AddStep("hide button", () => button.Hide());
        }
    }
}
