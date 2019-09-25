// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A skin configuration pre-populated with sane defaults.
    /// </summary>
    public class DefaultSkinConfiguration : SkinConfiguration
    {
        public DefaultSkinConfiguration()
        {
            ComboColours.AddRange(new[]
            {
                new Color4(17, 136, 170, 255),
                new Color4(102, 136, 0, 255),
                new Color4(204, 102, 0, 255),
                new Color4(121, 9, 13, 255)
            });
        }
    }
}
