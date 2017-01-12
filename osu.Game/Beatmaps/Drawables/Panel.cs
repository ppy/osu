//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    class Panel : Container, IStateful<PanelSelectedState>
    {
        public const float MAX_HEIGHT = 80;

        public override bool RemoveWhenNotAlive => false;

        public bool IsOnScreen;

        public override bool IsAlive => IsOnScreen && base.IsAlive;

        private Container nestedContainer;

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
        }

        public void SetMultiplicativeAlpha(float alpha)
        {
            nestedContainer.Alpha = alpha;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            applyState();
        }

        private void applyState()
        {
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

        private PanelSelectedState state = PanelSelectedState.NotSelected;

        public PanelSelectedState State
        {
            get { return state; }

            set
            {
                if (state == value) return;
                state = value;

                applyState();
            }
        }

        protected virtual void Selected()
        {
            nestedContainer.BorderThickness = 2.5f;
            nestedContainer.EdgeEffect = new EdgeEffect
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
            nestedContainer.EdgeEffect = new EdgeEffect
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

    enum PanelSelectedState
    {
        NotSelected,
        Selected
    }
}
