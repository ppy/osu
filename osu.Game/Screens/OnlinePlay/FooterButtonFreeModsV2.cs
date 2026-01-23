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
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class FooterButtonFreeModsV2 : ScreenFooterButton
    {
        private const float bar_height = 30f;

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

        private Drawable modsWedge = null!;
        private ModDisplay modDisplay = null!;
        private Container modContainer = null!;
        private ModCountText overflowModCountDisplay = null!;

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

            Add(modsWedge = new Container
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
                            new Box
                            {
                                Colour = colourProvider.Background3,
                                RelativeSizeAxes = Axes.Both,
                            },
                            modDisplay = new ModDisplay(showExtendedInformation: true)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Shear = -OsuGame.SHEAR,
                                Scale = new Vector2(0.5f),
                                Current = { BindTarget = FreeMods },
                                ExpansionMode = ExpansionMode.AlwaysContracted,
                            },
                            overflowModCountDisplay = new ModCountText
                            {
                                Mods = { BindTarget = FreeMods },
                                Freestyle = { BindTarget = Freestyle }
                            },
                        }
                    },
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Freestyle.BindValueChanged(f => Enabled.Value = !f.NewValue, true);
            FreeMods.BindValueChanged(m =>
            {
                if (m.NewValue.Count == 0)
                    modsWedge.FadeOut(200);
                else
                    modsWedge.FadeIn(200);
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            if (modDisplay.DrawWidth * modDisplay.Scale.X > modContainer.DrawWidth)
                overflowModCountDisplay.Show();
            else
                overflowModCountDisplay.Hide();
        }

        private partial class ModCountText : CompositeDrawable
        {
            public readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>();
            public readonly Bindable<bool> Freestyle = new Bindable<bool>();

            private OsuSpriteText text = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                RelativeSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colourProvider.Background3,
                        Alpha = 0.8f,
                        RelativeSizeAxes = Axes.Both,
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.Torus.With(size: 14f, weight: FontWeight.Bold),
                        Shear = -OsuGame.SHEAR,
                    }
                };

                Mods.BindValueChanged(_ => updateText());
                Freestyle.BindValueChanged(_ => updateText());

                updateText();
            }

            private void updateText()
            {
                if (Freestyle.Value)
                    text.Text = ModSelectOverlayStrings.AllMods.ToUpper();
                else
                    text.Text = ModSelectOverlayStrings.Mods(Mods.Value.Count).ToUpper();
            }
        }
    }
}
