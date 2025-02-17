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
    public partial class BeatmapPanel : PoolableDrawable, ICarouselPanel
    {
        [Resolved]
        private BeatmapCarousel carousel { get; set; } = null!;

        private Box activationFlash = null!;
        private OsuSpriteText text = null!;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            var inputRectangle = DrawRectangle;

            // Cover the gaps introduced by the spacing between BeatmapPanels so that clicks will not fall through the carousel.
            //
            // Caveat is that for simplicity, we are covering the full spacing, so panels with frontmost depth will have a slightly
            // larger hit target.
            inputRectangle = inputRectangle.Inflate(new MarginPadding { Vertical = BeatmapCarousel.SPACING });

            return inputRectangle.Contains(ToLocalSpace(screenSpacePos));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(500, CarouselItem.DEFAULT_HEIGHT);
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Aqua.Darken(5),
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

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);
            var beatmap = (BeatmapInfo)Item.Model;

            text.Text = $"Difficulty: {beatmap.DifficultyName} ({beatmap.StarRating:N1}*)";

            this.FadeInFromZero(500, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            carousel.Activate(Item!);
            return true;
        }

        #region ICarouselPanel

        public CarouselItem? Item { get; set; }
        public BindableBool Selected { get; } = new BindableBool();
        public BindableBool Expanded { get; } = new BindableBool();
        public BindableBool KeyboardSelected { get; } = new BindableBool();

        public double DrawYPosition { get; set; }

        public void Activated() => activationFlash.FadeOutFromOne(500, Easing.OutQuint);

        #endregion
    }
}
