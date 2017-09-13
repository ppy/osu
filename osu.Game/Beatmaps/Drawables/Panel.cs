// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Beatmaps.Drawables
{
    public class Panel : Container, IStateful<PanelSelectedState>
    {
        public const float MAX_HEIGHT = 80;

        public event Action<PanelSelectedState> StateChanged;

        public override bool RemoveWhenNotAlive => false;

        private readonly Container nestedContainer;

        protected override Container<Drawable> Content => nestedContainer;

        protected Panel()
        {
            Height = MAX_HEIGHT;
            RelativeSizeAxes = Axes.X;

            AddInternal(nestedContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 10,
                BorderColour = new Color4(221, 255, 255, 255),
            });

            Alpha = 0;
        }

        public void SetMultiplicativeAlpha(float alpha)
        {
            nestedContainer.Alpha = alpha;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ApplyState();
        }

        protected virtual void ApplyState(PanelSelectedState last = PanelSelectedState.Hidden)
        {
            if (!IsLoaded) return;

            switch (state)
            {
                case PanelSelectedState.Hidden:
                case PanelSelectedState.NotSelected:
                    Deselected();
                    break;
                case PanelSelectedState.Selected:
                    Selected();
                    break;
            }

            if (state == PanelSelectedState.Hidden)
                this.FadeOut(300, Easing.OutQuint);
            else
                this.FadeIn(250);
        }

        private PanelSelectedState state = PanelSelectedState.NotSelected;

        public PanelSelectedState State
        {
            get { return state; }

            set
            {
                if (state == value)
                    return;

                var last = state;
                state = value;

                ApplyState(last);

                StateChanged?.Invoke(State);
            }
        }

        protected virtual void Selected()
        {
            nestedContainer.BorderThickness = 2.5f;
            nestedContainer.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = new Color4(130, 204, 255, 150),
                Radius = 20,
                Roundness = 10,
            };
        }

        protected virtual void Deselected()
        {
            nestedContainer.BorderThickness = 0;
            nestedContainer.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(1),
                Radius = 10,
                Colour = Color4.Black.Opacity(100),
            };
        }

        protected override bool OnClick(InputState state)
        {
            State = PanelSelectedState.Selected;
            return true;
        }
    }

    public enum PanelSelectedState
    {
        Hidden,
        NotSelected,
        Selected
    }
}
