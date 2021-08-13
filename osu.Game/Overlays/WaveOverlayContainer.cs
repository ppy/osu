// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays
{
    public abstract class WaveOverlayContainer : OsuFocusedOverlayContainer
    {
        protected readonly WaveContainer Waves;

        protected override bool BlockNonPositionalInput => true;
        protected override Container<Drawable> Content => Waves;

        public const float WIDTH_PADDING = 80;

        protected override bool StartHidden => true;

        protected override string PopInSampleName => "UI/wave-pop-in";

        protected WaveOverlayContainer()
        {
            AddInternal(Waves = new WaveContainer
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        protected override void PopIn()
        {
            base.PopIn();

            Waves.Show();
            this.FadeIn(100, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            Waves.Hide();
            this.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.InQuint);
        }
    }
}
