// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public abstract partial class FullscreenOverlay<T> : WaveOverlayContainer, INamedOverlayComponent
        where T : OverlayHeader
    {
        public virtual IconUsage Icon => Header.Title.Icon;
        public virtual LocalisableString Title => Header.Title.Title;
        public virtual LocalisableString Description => Header.Title.Description;

        public T Header { get; }

        protected virtual Color4 BackgroundColour => ColourProvider.Background5;

        [Resolved]
        protected IAPIProvider API { get; private set; } = null!;

        [Cached]
        protected readonly OverlayColourProvider ColourProvider;

        protected override Container<Drawable> Content => content;

        private readonly Container content;

        protected FullscreenOverlay(OverlayColourScheme colourScheme)
        {
            Header = CreateHeader();

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
                Hollow = true,
                Radius = 10
            };

            base.Content.AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = BackgroundColour
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Waves.FirstWaveColour = ColourProvider.Light4;
            Waves.SecondWaveColour = ColourProvider.Light3;
            Waves.ThirdWaveColour = ColourProvider.Dark4;
            Waves.FourthWaveColour = ColourProvider.Dark3;
        }

        protected abstract T CreateHeader();

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
            FadeEdgeEffectTo(WaveContainer.SHADOW_OPACITY, WaveContainer.APPEAR_DURATION, Easing.Out);
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
