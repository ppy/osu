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
using OpenTK;
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
            BorderColour = new Color4(221, 255, 255, 255);

            RelativeSizeAxes = Axes.X;

            Deselected();
        }

        private PanelSelectedState state = PanelSelectedState.NotSelected;

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
            BorderThickness = 2.5f;

            EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Glow,
                Colour = new Color4(130, 204, 255, 150),
                Radius = 20,
                Roundness = 10,
            };
        }

        protected virtual void Deselected()
        {
            BorderThickness = 0;

            EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(1),
                Radius = 10,
                Colour = new Color4(0, 0, 0, 100),
            };
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
