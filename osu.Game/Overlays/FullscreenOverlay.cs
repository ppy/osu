// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public abstract class FullscreenOverlay : WaveOverlayContainer, IOnlineComponent
    {
        [Resolved]
        protected IAPIProvider API { get; private set; }

        private readonly OverlayColourScheme colourScheme;

        protected FullscreenOverlay(OverlayColourScheme colourScheme)
        {
            this.colourScheme = colourScheme;

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

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            OverlayColourProvider colourProvider;
            dependencies.Cache(colourProvider = new OverlayColourProvider(colourScheme));

            Waves.FirstWaveColour = colourProvider.Highlight1;
            Waves.SecondWaveColour = colourProvider.Link4;
            Waves.ThirdWaveColour = colourProvider.Dark3;
            Waves.FourthWaveColour = colourProvider.Dark1;
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
