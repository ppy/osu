using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;

namespace osu.Game.Screens.Mvis.Modules.v2
{
    public class TooltipContainer : Container, IHasTooltip
    {
        public string TooltipText { get; set; }
    }
}