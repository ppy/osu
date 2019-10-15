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
                new Color4(255, 192, 0, 255),
                new Color4(0, 202, 0, 255),
                new Color4(18, 124, 255, 255),
                new Color4(242, 24, 57, 255),
            });
        }
    }
}
