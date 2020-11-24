using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;

namespace osu.Game.Screens.Mvis.Collections.Interface
{
    public class TooltipContainer : Container, IHasTooltip
    {
        public string TooltipText { get; set; }
    }
}
