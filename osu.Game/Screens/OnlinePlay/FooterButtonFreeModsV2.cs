// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.SelectV2;
using osuTK;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class FooterButtonFreeModsV2 : ScreenFooterButton
    {
        public readonly Bindable<IReadOnlyList<Mod>> FreeMods = new Bindable<IReadOnlyList<Mod>>([]);
        public readonly Bindable<bool> Freestyle = new Bindable<bool>();

        public new Action Action
        {
            set => throw new NotSupportedException("The click action is handled by the button itself.");
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private Container modsWedge = null!;
        private ModDisplay modDisplay = null!;
        private Container modContainer = null!;
        private FooterButtonMods.ModCountText overflowModCountDisplay = null!;

        public FooterButtonFreeModsV2(ModSelectOverlay overlay)
            : base(overlay)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Text = OnlinePlayStrings.FooterButtonFreemods;
            Icon = FontAwesome.Solid.ExchangeAlt;
            AccentColour = colours.Lime1;

            Add(modsWedge = new InputBlockingContainer
            {
                Y = -5f,
                Depth = float.MaxValue,
                Origin = Anchor.BottomLeft,
                Shear = OsuGame.SHEAR,
                CornerRadius = CORNER_RADIUS,
                Size = new Vector2(BUTTON_WIDTH, FooterButtonMods.BAR_HEIGHT),
                Masking = true,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 4,
                    // Figma says 50% opacity, but it does not match up visually if taken at face value, and looks bad.
                    Colour = Colour4.Black.Opacity(0.25f),
                    Offset = new Vector2(0, 2),
                },
                Alpha = 0,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = colourProvider.Background4,
                        RelativeSizeAxes = Axes.Both,
                    },
                    modContainer = new Container
                    {
                        CornerRadius = CORNER_RADIUS,
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            modDisplay = new ModDisplay(showExtendedInformation: true)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Shear = -OsuGame.SHEAR,
                                Scale = new Vector2(0.5f),
                                Current = { BindTarget = FreeMods },
                                ExpansionMode = ExpansionMode.AlwaysContracted,
                            },
                            overflowModCountDisplay = new FooterButtonMods.ModCountText
                            {
                                Mods = { BindTarget = FreeMods },
                            },
                        }
                    },
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Freestyle.BindValueChanged(f =>
            {
                Enabled.Value = !f.NewValue;
                overflowModCountDisplay.CustomText = f.NewValue ? ModSelectOverlayStrings.AllMods.ToUpper() : (LocalisableString?)null;
            }, true);
            FreeMods.BindValueChanged(m =>
            {
                if (m.NewValue.Count == 0 && !Freestyle.Value)
                    modsWedge.FadeOut(300, Easing.OutExpo);
                else
                    modsWedge.FadeIn(300, Easing.OutExpo);
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            // If there are freemods selected but the display has no width, it's still loading.
            // Don't update visibility in this state or we will cause an awkward flash.
            if (FreeMods.Value.Count > 0 && Precision.AlmostEquals(modDisplay.DrawWidth, 0))
                return;

            bool showCountText =
                // When freestyle is enabled this text shows "ALL MODS"
                Freestyle.Value
                // Standard flow where mods are overflowing so we show count text.
                || modDisplay.DrawWidth * modDisplay.Scale.X > modContainer.DrawWidth;

            if (showCountText)
                overflowModCountDisplay.Show();
            else
                overflowModCountDisplay.Hide();
        }
    }
}
