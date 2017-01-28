//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Configuration;
using osu.Game.Online.API;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    class ToolbarOverlayToggleButton : ToolbarButton
    {
        private Box StateBackground;

        private OverlayContainer stateContainer;

        public OverlayContainer StateContainer
        {
            get { return stateContainer; }
            set
            {
                stateContainer = value;
                stateContainer.StateChanged += stateChanged;
            }
        }

        public ToolbarOverlayToggleButton()
        {
            Add(StateBackground = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(150).Opacity(180),
                BlendingMode = BlendingMode.Additive,
                Depth = 2,
                Alpha = 0,
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (stateContainer != null)
                stateContainer.StateChanged -= stateChanged;
        }

        private void stateChanged(OverlayContainer c, Visibility state)
        {
            switch (state)
            {
                case Visibility.Hidden:
                    StateBackground.FadeOut(200);
                    break;
                case Visibility.Visible:
                    StateBackground.FadeIn(200);
                    break;
            }
        }
    }
}
