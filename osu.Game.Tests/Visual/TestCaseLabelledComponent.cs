// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseLabelledComponent : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            var labelledComponent = new TestLabelledComponent { LabelText = "label text" };

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 250,
                AutoSizeAxes = Axes.Y,
                Child = labelledComponent
            };

            AddStep("set bottom text", () => labelledComponent.BottomLabelText = "bottom text");
            AddStep("remove bottom text", () => labelledComponent.BottomLabelText = string.Empty);
        }

        private class TestLabelledComponent : LabelledComponent
        {
            protected override Drawable CreateComponent() => new TestComponent();
        }

        private class TestComponent : CompositeDrawable
        {
            public TestComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new OsuSpriteText
                {
                    Colour = Color4.Red,
                    Text = @"\\ Component //"
                };
            }
        }
    }
}
