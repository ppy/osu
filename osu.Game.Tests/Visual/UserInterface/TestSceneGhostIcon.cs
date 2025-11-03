// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneGhostIcon : OsuTestScene
    {
        public TestSceneGhostIcon()
        {
            Add(new GhostIcon
            {
                Size = new Vector2(64),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }
    }
}
