// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning
{
    /// <summary>
    /// A sprite which is displayed within the playfield, but historically was not considered part of the playfield.
    /// Performs scale adjustment to undo the scale applied by <see cref="PlayfieldAdjustmentContainer"/> (osu! ruleset specifically).
    /// </summary>
    public partial class NonPlayfieldSprite : Sprite
    {
        public override Texture? Texture
        {
            get => base.Texture;
            set
            {
                if (value != null)
                    value.ScaleAdjust *= LegacySkin.POSITION_SCALE_FACTOR;
                base.Texture = value;
            }
        }
    }
}
