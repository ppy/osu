// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;

namespace osu.Game.Graphics.Containers
{
    public class OsuHoverContainer : OsuClickableContainer, IHandleHover
    {
        private Color4 hoverColour;

        public override bool OnHover(InputState state)
        {
            this.FadeColour(hoverColour, 500, Easing.OutQuint);
            return base.OnHover(state);
        }

        public virtual void OnHoverLost(InputState state)
        {
            this.FadeColour(Color4.White, 500, Easing.OutQuint);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            hoverColour = colours.Yellow;
        }
    }
}
