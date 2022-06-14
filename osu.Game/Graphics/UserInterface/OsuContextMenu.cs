// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuContextMenu : OsuMenu
    {
        private const int fade_duration = 250;

        [Resolved]
        private OsuContextMenuSamples samples { get; set; }

        // todo: this shouldn't be required after https://github.com/ppy/osu-framework/issues/4519 is fixed.
        private bool wasOpened;
        private readonly bool playClickSample;

        public OsuContextMenu(bool playClickSample = false)
            : base(Direction.Vertical)
        {
            MaskingContainer.CornerRadius = 5;
            MaskingContainer.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.1f),
                Radius = 4,
            };

            ItemsContainer.Padding = new MarginPadding { Vertical = DrawableOsuMenuItem.MARGIN_VERTICAL };

            MaxHeight = 250;

            this.playClickSample = playClickSample;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.ContextMenuGray;
        }

        protected override void AnimateOpen()
        {
            this.FadeIn(fade_duration, Easing.OutQuint);

            if (playClickSample)
                samples.PlayClickSample();

            if (!wasOpened)
                samples.PlayOpenSample();

            wasOpened = true;
        }

        protected override void AnimateClose()
        {
            this.FadeOut(fade_duration, Easing.OutQuint);

            if (wasOpened)
                samples.PlayCloseSample();

            wasOpened = false;
        }

        protected override Menu CreateSubMenu() => new OsuContextMenu();
    }
}
