// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays
{
    public abstract partial class WaveOverlayContainer : OsuFocusedOverlayContainer
    {
        protected readonly WaveContainer Waves;

        protected override bool BlockNonPositionalInput => true;
        protected override Container<Drawable> Content => Waves;

        public const float WIDTH_PADDING = 80;

        protected override bool StartHidden => true;

        // `WaveContainer` plays PopIn/PopOut samples, so we disable the overlay-level one as to not double-up sample playback.
        protected override string PopInSampleName => string.Empty;
        protected override string PopOutSampleName => string.Empty;

        public const float HORIZONTAL_PADDING = 50;

        protected WaveOverlayContainer()
        {
            AddInternal(Waves = new WaveContainer
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        protected override void PopIn()
        {
            Waves.Show();
            this.FadeIn(100, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            Waves.Hide();
            this.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.InQuint)
                // base call is responsible for stopping preview tracks.
                // delay it until the fade has concluded to ensure that nothing inside the overlay has triggered
                // another preview track playback in the meantime, leaving an "orphaned" preview playing.
                .OnComplete(_ => base.PopOut());
        }
    }
}
