// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        }

        protected override void PopOut()
        {
            base.PopOut();
            Waves.Hide();
        }
    }
}
