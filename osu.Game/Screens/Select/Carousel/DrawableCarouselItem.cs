// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select.Carousel
{
    public abstract class DrawableCarouselItem : PoolableDrawable
    {
        public const float MAX_HEIGHT = 80;

        public override bool IsPresent => base.IsPresent || Item?.Visible == true;

        public CarouselItem Item
        {
            get => item;
            set
            {
                if (item == value)
                    return;

                if (item != null)
                {
                    Item.Filtered.ValueChanged -= onStateChange;
                    Item.State.ValueChanged -= onStateChange;

                    if (item is CarouselGroup group)
                    {
                        foreach (var c in group.Children)
                            c.Filtered.ValueChanged -= onStateChange;
                    }
                }

                item = value;

                if (IsLoaded)
                    UpdateItem();
            }
        }

        public virtual IEnumerable<DrawableCarouselItem> ChildItems => Enumerable.Empty<DrawableCarouselItem>();

        private readonly Container nestedContainer;

        protected readonly Container BorderContainer;

        private readonly Box hoverLayer;

        protected Container<Drawable> Content => nestedContainer;

        protected DrawableCarouselItem()
        {
            Height = MAX_HEIGHT;
            RelativeSizeAxes = Axes.X;
            Alpha = 0;

            InternalChild = BorderContainer = new Container
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
                        Blending = BlendingParameters.Additive,
                    },
                }
            };
        }

        private SampleChannel sampleHover;
        private CarouselItem item;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            sampleHover = audio.Samples.Get($@"SongSelect/song-ping-variation-{RNG.Next(1, 5)}");
            hoverLayer.Colour = colours.Blue.Opacity(0.1f);
        }

        protected override bool OnHover(HoverEvent e)
        {
            sampleHover?.Play();

            hoverLayer.FadeIn(100, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverLayer.FadeOut(1000, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        public void SetMultiplicativeAlpha(float alpha) => BorderContainer.Alpha = alpha;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            UpdateItem();
        }

        protected virtual void UpdateItem()
        {
            if (item == null)
                return;

            Scheduler.AddOnce(ApplyState);

            Item.Filtered.ValueChanged += onStateChange;
            Item.State.ValueChanged += onStateChange;

            if (Item is CarouselGroup group)
            {
                foreach (var c in group.Children)
                    c.Filtered.ValueChanged += onStateChange;
            }
        }

        private void onStateChange(ValueChangedEvent<CarouselItemState> obj) => Scheduler.AddOnce(ApplyState);

        private void onStateChange(ValueChangedEvent<bool> _) => Scheduler.AddOnce(ApplyState);

        protected virtual void ApplyState()
        {
            Debug.Assert(Item != null);

            switch (Item.State.Value)
            {
                case CarouselItemState.NotSelected:
                    Deselected();
                    break;

                case CarouselItemState.Selected:
                    Selected();
                    break;
            }

            if (!Item.Visible)
                this.FadeOut(300, Easing.OutQuint);
            else
                this.FadeIn(250);
        }

        protected virtual void Selected()
        {
            Debug.Assert(Item != null);

            Item.State.Value = CarouselItemState.Selected;

            BorderContainer.BorderThickness = 2.5f;
            BorderContainer.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = new Color4(130, 204, 255, 150),
                Radius = 20,
                Roundness = 10,
            };
        }

        protected virtual void Deselected()
        {
            Item.State.Value = CarouselItemState.NotSelected;

            BorderContainer.BorderThickness = 0;
            BorderContainer.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(1),
                Radius = 10,
                Colour = Color4.Black.Opacity(100),
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            Item.State.Value = CarouselItemState.Selected;
            return true;
        }
    }
}
