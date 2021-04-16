using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;

namespace Mvis.Plugin.CollectionSupport.Sidebar
{
    public class TooltipContainer : Container, IHasTooltip
    {
        public string TooltipText { get; set; }
    }
}
