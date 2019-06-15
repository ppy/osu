// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public abstract class FullscreenOverlay : WaveOverlayContainer, IOnlineComponent
    {
        [Resolved]
        protected IAPIProvider API { get; private set; }

        protected FullscreenOverlay()
        {
            Waves.FirstWaveColour = OsuColour.Gray(0.4f);
            Waves.SecondWaveColour = OsuColour.Gray(0.3f);
            Waves.ThirdWaveColour = OsuColour.Gray(0.2f);
            Waves.FourthWaveColour = OsuColour.Gray(0.1f);

            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
            Width = 0.85f;
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            Masking = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0),
                Type = EdgeEffectType.Shadow,
                Radius = 10
            };
        }

        public override void Show()
        {
            if (State.Value == Visibility.Visible)
            {
                // re-trigger the state changed so we can potentially surface to front
                State.TriggerChange();
            }
            else
            {
                base.Show();
            }
        }

        protected override void PopIn()
        {
            base.PopIn();
            FadeEdgeEffectTo(0.4f, WaveContainer.APPEAR_DURATION, Easing.Out);
        }

        protected override void PopOut()
        {
            base.PopOut();
            FadeEdgeEffectTo(0, WaveContainer.DISAPPEAR_DURATION, Easing.In).OnComplete(_ => PopOutComplete());
        }

        protected virtual void PopOutComplete()
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            API.Register(this);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            API?.Unregister(this);
        }

        public virtual void APIStateChanged(IAPIProvider api, APIState state)
        {
        }
    }
}
