// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using System.Collections.Generic;
using System.Diagnostics;

namespace osu.Game.Graphics.Sprites
{
    public class OsuLinkSpriteText : OsuSpriteText
    {
        private readonly OsuHoverContainer content;

        public override bool HandleInput => content.Action != null;

        protected override Container<Drawable> Content => content ?? (Container<Drawable>)this;

        protected override IEnumerable<Drawable> FlowingChildren => Children;

        private string url;

        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    url = value;

                    content.Action = OnLinkClicked;
                }
            }
        }

        public OsuLinkSpriteText()
        {
            AddInternal(content = new OsuHoverContainer
            {
                AutoSizeAxes = Axes.Both,
            });
        }

        public ColourInfo TextColour
        {
            get { return Content.Colour; }
            set { Content.Colour = value; }
        }

        protected virtual void OnLinkClicked() => Process.Start(Url);
    }
}
