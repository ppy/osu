// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.MathUtils;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class Panel : Container, IStateful<PanelSelectedState>
    {
        public const float MAX_HEIGHT = 80;

        public event Action<PanelSelectedState> StateChanged;

        public override bool RemoveWhenNotAlive => false;

        private readonly Container nestedContainer;

        private readonly Container borderContainer;

        private readonly Box hoverLayer;

        protected override Container<Drawable> Content => nestedContainer;

        protected Panel()
        {
            Height = MAX_HEIGHT;
            RelativeSizeAxes = Axes.X;

            AddInternal(borderContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 10,
                BorderColour = new Color4(221, 255, 255, 255),
                Children = new Drawable[]
                {
                    nestedContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    hoverLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Blending = BlendingMode.Additive,
                    },
                }
            });

            Alpha = 0;
        }

        private SampleChannel sampleHover;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            sampleHover = audio.Sample.Get($@"SongSelect/song-ping-variation-{RNG.Next(1, 5)}");
            hoverLayer.Colour = colours.Blue.Opacity(0.1f);
        }

        protected override bool OnHover(InputState state)
        {
            sampleHover?.Play();

            hoverLayer.FadeIn(100, Easing.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            hoverLayer.FadeOut(1000, Easing.OutQuint);
            base.OnHoverLost(state);
        }

        public void SetMultiplicativeAlpha(float alpha)
        {
            borderContainer.Alpha = alpha;
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
            borderContainer.BorderThickness = 2.5f;
            borderContainer.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = new Color4(130, 204, 255, 150),
                Radius = 20,
                Roundness = 10,
            };
        }

        protected virtual void Deselected()
        {
            borderContainer.BorderThickness = 0;
            borderContainer.EdgeEffect = new EdgeEffectParameters
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
