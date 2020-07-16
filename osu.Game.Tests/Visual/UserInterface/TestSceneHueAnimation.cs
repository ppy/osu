// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneHueAnimation : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            HueAnimation anim2;

            Add(anim2 = new HueAnimation
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Texture = textures.Get("Intro/Triangles/logo-highlight"),
                Colour = Colour4.White,
            });

            HueAnimation anim;

            Add(anim = new HueAnimation
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Texture = textures.Get("Intro/Triangles/logo-background"),
                Colour = OsuColour.Gray(0.6f),
            });

            AddSliderStep("Progress", 0f, 1f, 0f, newValue =>
            {
                anim2.AnimationProgress = newValue;
                anim.AnimationProgress = newValue;
            });
        }
    }
}
