// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapCarouselPanel : PoolableDrawable, ICarouselPanel
    {
        [Resolved]
        private BeatmapCarousel carousel { get; set; } = null!;

        public CarouselItem? Item
        {
            get => item;
            set
            {
                item = value;

                selected.UnbindBindings();

                if (item != null)
                    selected.BindTo(item.Selected);
            }
        }

        private readonly BindableBool selected = new BindableBool();
        private CarouselItem? item;

        [BackgroundDependencyLoader]
        private void load()
        {
            selected.BindValueChanged(value =>
            {
                if (value.NewValue)
                {
                    BorderThickness = 5;
                    BorderColour = Color4.Pink;
                }
                else
                {
                    BorderThickness = 0;
                }
            });
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();
            Item = null;
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            DrawYPosition = Item.CarouselYPosition;

            Size = new Vector2(500, Item.DrawHeight);
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = (Item.Model is BeatmapInfo ? Color4.Aqua : Color4.Yellow).Darken(5),
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuSpriteText
                {
                    Text = Item.ToString() ?? string.Empty,
                    Padding = new MarginPadding(5),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };

            this.FadeInFromZero(500, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            carousel.CurrentSelection = Item!.Model;
            return true;
        }

        public double DrawYPosition { get; set; }
    }
}
