// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;

namespace osu.Game.Graphics.Sprites
{
    public class OsuLinkSpriteText : OsuSpriteText
    {
        protected override IEnumerable<Drawable> FlowingChildren => Children;

        protected override bool OnClick(InputState state)
        {
            OnLinkClicked();
            return true;
        }

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
                    url = value;
            }
        }

        public ColourInfo TextColour
        {
            get { return Content.Colour; }
            set { Content.Colour = value; }
        }

        protected virtual void OnLinkClicked() => Process.Start(Url);
    }
}
