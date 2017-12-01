// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                if (value != null)
                {
                    url = value;
                    content.Action = () => Process.Start(value);
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
    }
}
