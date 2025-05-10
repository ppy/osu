// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneParticleExplosion : OsuTestScene
    {
        private ParticleExplosion explosion;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            AddStep("create initial", () =>
            {
                Child = explosion = new ParticleExplosion(textures.Get("Cursor/cursortrail"), 150, 1200)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(400)
                };
            });

            AddWaitStep("wait for playback", 5);

            AddRepeatStep(@"restart animation", () =>
            {
                explosion.Restart();
            }, 10);
        }
    }
}
