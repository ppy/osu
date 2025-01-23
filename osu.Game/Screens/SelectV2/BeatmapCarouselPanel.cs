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

        private Box activationFlash = null!;
        private Box background = null!;
        private OsuSpriteText text = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    Alpha = 0.8f,
                    RelativeSizeAxes = Axes.Both,
                },
                activationFlash = new Box
                {
                    Colour = Color4.White,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                },
                text = new OsuSpriteText
                {
                    Padding = new MarginPadding(5),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };

            Selected.BindValueChanged(value =>
            {
                activationFlash.FadeTo(value.NewValue ? 0.2f : 0, 500, Easing.OutQuint);
            });

            KeyboardSelected.BindValueChanged(value =>
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
            Selected.Value = false;
            KeyboardSelected.Value = false;
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            DrawYPosition = Item.CarouselYPosition;

            Size = new Vector2(500, Item.DrawHeight);
            Masking = true;

            background.Colour = (Item.Model is BeatmapInfo ? Color4.Aqua : Color4.Yellow).Darken(5);
            text.Text = getTextFor(Item.Model);

            this.FadeInFromZero(500, Easing.OutQuint);
        }

        private string getTextFor(object item)
        {
            switch (item)
            {
                case BeatmapInfo bi:
                    return $"Difficulty: {bi.DifficultyName} ({bi.StarRating:N1}*)";

                case BeatmapSetInfo si:
                    return $"{si.Metadata}";
            }

            return "unknown";
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (carousel.CurrentSelection == Item!.Model)
                carousel.TryActivateSelection();
            else
                carousel.CurrentSelection = Item!.Model;
            return true;
        }

        public CarouselItem? Item { get; set; }
        public BindableBool Selected { get; } = new BindableBool();
        public BindableBool KeyboardSelected { get; } = new BindableBool();

        public double DrawYPosition { get; set; }

        public void FlashFromActivation()
        {
            activationFlash.FadeOutFromOne(500, Easing.OutQuint);
        }
    }
}
