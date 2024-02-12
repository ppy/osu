using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace osu.Game.Graphics.Sprites
{
    /// <summary>
    /// A <see cref="SpriteIcon"/> with a publicly settable tooltip text.
    /// </summary>
    public partial class SpriteIconWithTooltip : SpriteIcon, IHasTooltip
    {
        public LocalisableString TooltipText { get; set; }
    }
}
