// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Screens.Footer;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class FooterButtonFreestyleV2 : ScreenFooterButton
    {
        private const float bar_height = 30f;

        public readonly Bindable<bool> Freestyle = new Bindable<bool>();

        public new Action Action
        {
            set => throw new NotSupportedException("The click action is handled by the button itself.");
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private Drawable statusBackground = null!;
        private OsuSpriteText statusText = null!;

        public FooterButtonFreestyleV2()
        {
            // Overwrite any external behaviour as we delegate the main toggle action to a sub-button.
            base.Action = () => Freestyle.Value = !Freestyle.Value;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Text = "Freestyle";
            Icon = FontAwesome.Solid.ExchangeAlt;
            AccentColour = colours.Lime1;

            AddRange(new[]
            {
                new Container
                {
                    Y = -5f,
                    Depth = float.MaxValue,
                    Origin = Anchor.BottomLeft,
                    Shear = OsuGame.SHEAR,
                    CornerRadius = CORNER_RADIUS,
                    Size = new Vector2(BUTTON_WIDTH, bar_height),
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 4,
                        // Figma says 50% opacity, but it does not match up visually if taken at face value, and looks bad.
                        Colour = Colour4.Black.Opacity(0.25f),
                        Offset = new Vector2(0, 2),
                    },
                    Children = new[]
                    {
                        statusBackground = new Box
                        {
                            Colour = colourProvider.Background3,
                            RelativeSizeAxes = Axes.Both,
                        },
                        statusText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Torus.With(size: 14f, weight: FontWeight.Bold),
                            Shear = -OsuGame.SHEAR,
                        },
                    }
                },
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Freestyle.BindValueChanged(v =>
            {
                if (v.NewValue)
                {
                    statusBackground.Colour = colours.Yellow;
                    statusText.Text = "ON";
                    statusText.Colour = Color4.Black;
                }
                else
                {
                    statusBackground.Colour = colourProvider.Background3;
                    statusText.Text = "OFF";
                    statusText.Colour = Color4.White;
                }
            }, true);
        }
    }
}
