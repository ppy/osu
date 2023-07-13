// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneRevertToDefaultButton : OsuTestScene
    {
        [Resolved]
        private OsuColour colours { get; set; }

        private float scale = 1;

        private readonly Bindable<float> current = new Bindable<float>
        {
            Default = default,
            Value = 1,
        };

        [Test]
        public void TestBasic()
        {
            RevertToDefaultButton<float> revertToDefaultButton = null;

            AddStep("create button", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.GreySeaFoam
                    },
                    revertToDefaultButton = new RevertToDefaultButton<float>
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(scale),
                        Current = current,
                    }
                }
            });
            AddSliderStep("set scale", 1, 4, 1, scale =>
            {
                this.scale = scale;
                if (revertToDefaultButton != null)
                    revertToDefaultButton.Scale = new Vector2(scale);
            });
            AddToggleStep("toggle default state", state => current.Value = state ? default : 1);
            AddToggleStep("toggle disabled state", state => current.Disabled = state);
        }
    }
}
