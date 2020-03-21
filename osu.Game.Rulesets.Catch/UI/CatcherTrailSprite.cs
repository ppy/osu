// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherTrailSprite : Sprite
    {
        public CatcherTrailSprite(Texture texture)
        {
            Texture = texture;

            Size = new Vector2(CatcherArea.CATCHER_SIZE);

            // Sets the origin roughly to the centre of the catcher's plate to allow for correct scaling.
            OriginPosition = new Vector2(0.5f, 0.06f) * CatcherArea.CATCHER_SIZE;
        }
    }
}
