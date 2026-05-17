// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osuTK.Graphics;

namespace osu.Game.Overlays.Dialog
{
    public abstract partial class PopupDialog : VisibilityContainer
    {
        public const int DEFAULT_WIDTH = 500;
        public const float ENTER_DURATION = 500;
        public const float EXIT_DURATION = 500;

        protected Container Dialog { get; }

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        protected PopupDialog()
        {
            Width = DEFAULT_WIDTH;
            AutoSizeAxes = Axes.Y;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChild = Dialog = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0f,
                Masking = true,
                CornerRadius = 20,
                CornerExponent = 2.5f,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.2f),
                    Radius = 14,
                },
                Child = content = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                }
            };

            // It's important we start in a visible state so our state fires on hide, even before load.
            // This is used by the dialog overlay to know when the dialog was dismissed.
            Show();
        }

        protected override void PopIn()
        {
            // Reset various animations but only if the dialog animation fully completed
            if (Dialog.Alpha == 0)
                Dialog.ScaleTo(0.7f);

            Dialog
                .ScaleTo(1, 750, Easing.OutElasticHalf)
                .FadeIn(ENTER_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            Dialog
                .ScaleTo(0.7f, EXIT_DURATION, Easing.Out)
                .FadeOut(EXIT_DURATION, Easing.OutQuint);
        }
    }
}
