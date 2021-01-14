// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Screens.OnlinePlay;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public abstract class FullscreenOverlay<T> : WaveOverlayContainer, INamedOverlayComponent
        where T : OverlayHeader
    {
        public virtual string IconTexture => Header?.Title.IconTexture ?? string.Empty;
        public virtual string Title => Header?.Title.Title ?? string.Empty;
        public virtual string Description => Header?.Title.Description ?? string.Empty;

        public T Header { get; }

        [Resolved]
        protected IAPIProvider API { get; private set; }

        [Cached]
        protected readonly OverlayColourProvider ColourProvider;

        [Cached]
        protected OngoingOperationTracker OngoingOperationTracker { get; private set; }

        protected override Container<Drawable> Content => content;

        private readonly LoadingLayer loadingLayer;
        private readonly Container content;

        protected FullscreenOverlay(OverlayColourScheme colourScheme, T header)
        {
            Header = header;
            ColourProvider = new OverlayColourProvider(colourScheme);

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
            base.Content.AddRange(new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
                loadingLayer = new LoadingLayer(true),
            });

            AddInternal(OngoingOperationTracker = new OngoingOperationTracker());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Waves.FirstWaveColour = ColourProvider.Light4;
            Waves.SecondWaveColour = ColourProvider.Light3;
            Waves.ThirdWaveColour = ColourProvider.Dark4;
            Waves.FourthWaveColour = ColourProvider.Dark3;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            OngoingOperationTracker.InProgress.BindValueChanged(isInProgress =>
            {
                if (isInProgress.NewValue)
                    loadingLayer.Show();
                else
                    loadingLayer.Hide();
            }, true);
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
    }
}
