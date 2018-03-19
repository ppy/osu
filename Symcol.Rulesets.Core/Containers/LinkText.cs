using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using System.Collections.Generic;
using System.Diagnostics;

namespace Symcol.Rulesets.Core.Containers
{
    public class LinkText : OsuSpriteText, IHasTooltip
    {
        public string TooltipText => Tooltip;

        public virtual string Tooltip => "";

        private readonly OsuHoverContainer content;

        public override bool HandleKeyboardInput => content.Action != null;
        public override bool HandleMouseInput => content.Action != null;

        protected override Container<Drawable> Content => content ?? (Container<Drawable>)this;

        public override IEnumerable<Drawable> FlowingChildren => Children;

        public string Url
        {
            set
            {
                if (value != null)
                    content.Action = () => Process.Start(value);
            }
        }

        public LinkText()
        {
            AddInternal(content = new OsuHoverContainer
            {
                AutoSizeAxes = Axes.Both,
            });
        }
    }
}
