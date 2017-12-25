// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;

namespace osu.Game.Graphics.Sprites
{
    public class OsuLinkSpriteText : OsuSpriteText
    {
        protected override IEnumerable<Drawable> FlowingChildren => Children;

        protected override Container<Drawable> Content => content;

        private readonly Container content;

        public OsuLinkSpriteText()
        {
            AddInternal(content = new OsuHoverContainer
            {
                AutoSizeAxes = Axes.Both,
                Action = OnLinkClicked,
            });
        }

        private string url;

        public string Url
        {
            get => url;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    url = value;
            }
        }

        public ColourInfo TextColour
        {
            get => Content.Colour;
            set => Content.Colour = value;
        }

        protected virtual void OnLinkClicked() => Process.Start(Url);
    }
}
