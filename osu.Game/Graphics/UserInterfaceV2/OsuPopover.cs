// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class OsuPopover : Popover, IKeyBindingHandler<GlobalAction>
    {
        private const float fade_duration = 250;
        private const double scale_duration = 500;

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
        private void load([CanBeNull] OverlayColourProvider colourProvider, OsuColour colours)
        {
            Background.Colour = Arrow.Colour = colourProvider?.Background4 ?? colours.GreySeaFoamDarker;
        }

        protected override Drawable CreateArrow() => Empty();

        protected override void PopIn()
        {
            this.ScaleTo(1, scale_duration, Easing.OutElasticHalf);
            this.FadeIn(fade_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.ScaleTo(0.7f, scale_duration, Easing.OutQuint);
            this.FadeOut(fade_duration, Easing.OutQuint);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            if (State.Value == Visibility.Hidden)
                return false;

            if (e.Action == GlobalAction.Back)
            {
                Hide();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
