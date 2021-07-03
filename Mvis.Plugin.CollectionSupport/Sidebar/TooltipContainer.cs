using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;

namespace Mvis.Plugin.CollectionSupport.Sidebar
{
    public class TooltipContainer : Container, IHasTooltip
    {
        public LocalisableString TooltipText { get; set; }
    }
}
