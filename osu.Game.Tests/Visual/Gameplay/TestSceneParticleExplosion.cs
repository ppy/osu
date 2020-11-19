// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneParticleExplosion : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            AddStep(@"display", () =>
            {
                Child = new ParticleExplosion(textures.Get("Cursor/cursortrail"), 150, 1200)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(200)
                };
            });
        }
    }
}
