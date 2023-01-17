// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.UI;

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
                    // stable "magic ratio". see OsuPlayfieldAdjustmentContainer for full explanation.
                    value.ScaleAdjust *= 1.6f;
                base.Texture = value;
            }
        }
    }
}
