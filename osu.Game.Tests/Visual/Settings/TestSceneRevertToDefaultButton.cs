// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneRevertToDefaultButton : ThemeComparisonTestScene
    {
        private float scale = 1;

        private readonly Bindable<float> current = new Bindable<float>
        {
            Default = default,
            Value = 1,
        };

        protected override Drawable CreateContent() => new Container
        {
            AutoSizeAxes = Axes.Both,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Child = new RevertToDefaultButton<float>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(scale),
                Current = current,
            }
        };

        [Test]
        public void TestStates()
        {
            AddStep("create content", () => CreateThemedContent(OverlayColourScheme.Purple));
            AddSliderStep("set scale", 1, 4, 1, scale =>
            {
                this.scale = scale;
                foreach (var revertToDefaultButton in this.ChildrenOfType<RevertToDefaultButton<float>>())
                    revertToDefaultButton.Parent!.Scale = new Vector2(scale);
            });
            AddToggleStep("toggle default state", state => current.Value = state ? default : 1);
            AddToggleStep("toggle disabled state", state => current.Disabled = state);
        }
    }
}
