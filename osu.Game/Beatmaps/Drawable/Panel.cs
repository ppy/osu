//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Drawable
{
    class Panel : Container, IStateful<PanelSelectedState>
    {
        public Panel()
        {
            Height = 80;

            Masking = true;
            CornerRadius = 10;
            BorderColour = new Color4(221, 255, 255, 0);
            GlowColour = new Color4(102, 204, 255, 100);

            RelativeSizeAxes = Axes.X;
        }

        private PanelSelectedState state;

        public PanelSelectedState State
        {
            get { return state; }

            set
            {
                if (state == value) return;
                state = value;

                switch (state)
                {
                    case PanelSelectedState.NotSelected:
                        Deselected();
                        break;
                    case PanelSelectedState.Selected:
                        Selected();
                        break;
                }
            }
        }

        protected virtual void Selected()
        {
            BorderColour = new Color4(BorderColour.R, BorderColour.G, BorderColour.B, 1f);
            GlowRadius = 10;
            BorderThickness = 2.5f;
        }

        protected virtual void Deselected()
        {
            BorderColour = new Color4(BorderColour.R, BorderColour.G, BorderColour.B, 0);
            GlowRadius = 0;
            BorderThickness = 0;
        }

        protected override bool OnClick(InputState state)
        {
            State = PanelSelectedState.Selected;
            return true;
        }
    }

    enum PanelSelectedState
    {
        NotSelected,
        Selected
    }
}
