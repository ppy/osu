// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osuTK;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class OsuPopover : Popover, IKeyBindingHandler<GlobalAction>
    {
        private const float fade_duration = 250;
        private const double scale_duration = 500;

        private Sample? samplePopIn;
        private Sample? samplePopOut;
        protected virtual string PopInSampleName => "UI/overlay-pop-in";
        protected virtual string PopOutSampleName => "UI/overlay-pop-out";

        // required due to LoadAsyncComplete() in `VisibilityContainer` calling PopOut() during load - similar workaround to `OsuDropdownMenu`
        private bool wasOpened;

        public OsuPopover(bool withPadding = true)
        {
            Content.Padding = withPadding ? new MarginPadding(20) : new MarginPadding();

            Body.Masking = true;
            Body.CornerRadius = 10;
            Body.Margin = new MarginPadding(10);
            Body.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(0, 2),
                Radius = 5,
                Colour = Colour4.Black.Opacity(0.3f)
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? colourProvider, OsuColour colours, AudioManager audio)
        {
            Background.Colour = Arrow.Colour = colourProvider?.Background4 ?? colours.GreySeaFoamDarker;
            samplePopIn = audio.Samples.Get(PopInSampleName);
            samplePopOut = audio.Samples.Get(PopOutSampleName);
        }

        protected override Drawable CreateArrow() => Empty();

        protected override void PopIn()
        {
            this.ScaleTo(1, scale_duration, Easing.OutElasticHalf);
            this.FadeIn(fade_duration, Easing.OutQuint);

            samplePopIn?.Play();
            wasOpened = true;
        }

        protected override void PopOut()
        {
            this.ScaleTo(0.7f, scale_duration, Easing.OutQuint);
            this.FadeOut(fade_duration, Easing.OutQuint);

            if (wasOpened)
                samplePopOut?.Play();
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == Key.Escape)
                return false; // disable the framework-level handling of escape key for conformity (we use GlobalAction.Back).

            return base.OnKeyDown(e);
        }

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            if (State.Value == Visibility.Hidden)
                return false;

            if (e.Action == GlobalAction.Back)
            {
                this.HidePopover();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
