using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;

namespace osu.Game.Graphics.Sprites
{
    /// <summary>
    /// An <see cref="OsuSpriteText"/> with a publicly settable tooltip text.
    /// </summary>
    internal partial class SpriteTextWithTooltip : OsuSpriteText, IHasTooltip
    {
        public LocalisableString TooltipText { get; set; }
    }
}
