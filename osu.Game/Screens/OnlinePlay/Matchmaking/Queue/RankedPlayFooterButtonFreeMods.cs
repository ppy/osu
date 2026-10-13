// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public partial class RankedPlayFooterButtonFreeMods : ScreenFooterButton
    {
        public new readonly IBindable<ScreenQueue.MatchmakingScreenState> State = new Bindable<ScreenQueue.MatchmakingScreenState>();

        public readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>([]);

        public new Action Action
        {
            set => throw new NotSupportedException("The click action is handled by the button itself.");
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private Container modsWedge = null!;

        public RankedPlayFooterButtonFreeMods(ModSelectOverlay overlay)
            : base(overlay)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Text = OnlinePlayStrings.FooterButtonFreemods;
            TooltipText = MultiplayerMatchStrings.FreeModsButtonTooltip;
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
                    new Container
                    {
                        CornerRadius = CORNER_RADIUS,
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new ModDisplay(showExtendedInformation: true)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Shear = -OsuGame.SHEAR,
                                Scale = new Vector2(0.5f),
                                Current = { BindTarget = Mods },
                                ExpansionMode = ExpansionMode.AlwaysContracted,
                            },
                        }
                    },
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            State.BindValueChanged(s =>
            {
                if (s.NewValue == ScreenQueue.MatchmakingScreenState.Idle)
                    Enabled.Value = true;
                else
                {
                    Enabled.Value = false;
                    Overlay?.Hide();
                }
            }, true);

            Mods.BindValueChanged(m =>
            {
                if (m.NewValue.Count == 0)
                    modsWedge.FadeOut(300, Easing.OutExpo);
                else
                    modsWedge.FadeIn(300, Easing.OutExpo);
            }, true);
        }
    }
}
