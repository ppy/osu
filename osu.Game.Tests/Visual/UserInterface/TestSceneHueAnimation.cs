// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneHueAnimation : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            HueAnimation anim;

            Add(anim = new HueAnimation
            {
                Texture = textures.Get("Intro/Triangles/logo-triangles.png"),
                Colour = Colour4.White,
            });

            AddSliderStep("Progress", 0f, 1f, 0f, newValue => anim.AnimationProgress = newValue);
        }
    }
}
