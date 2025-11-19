// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osuTK;
using osuTK.Graphics;
using static osu.Game.Overlays.Mods.ModCustomisationPanel;

namespace osu.Game.Overlays.Mods
{
    public partial class ModCustomisationHeader : OsuClickableContainer
    {
        private Box background = null!;
        private Box hoverBackground = null!;
        private Box backgroundFlash = null!;
        private SpriteIcon icon = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public readonly Bindable<ModCustomisationPanelState> ExpandedState = new Bindable<ModCustomisationPanelState>();

        private readonly ModCustomisationPanel panel;

        public ModCustomisationHeader(ModCustomisationPanel panel)
        {
            this.panel = panel;
            Enabled.Value = false;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            CornerRadius = 10f;
            Masking = true;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                hoverBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(50),
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
                backgroundFlash = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White.Opacity(0.4f),
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = ModSelectOverlayStrings.CustomisationPanelHeader,
                    UseFullGlyphHeight = false,
                    Font = OsuFont.Torus.With(size: 20f, weight: FontWeight.SemiBold),
                    Margin = new MarginPadding { Left = 20f },
                },
                new Container
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Size = new Vector2(16f),
                    Margin = new MarginPadding { Right = 20f },
                    Child = icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.Solid.ChevronDown,
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Enabled.BindValueChanged(e =>
            {
                TooltipText = e.NewValue
                    ? string.Empty
                    : ModSelectOverlayStrings.CustomisationPanelDisabledReason;

                if (e.NewValue)
                {
                    backgroundFlash.FadeInFromZero(150, Easing.OutQuad).Then()
                                   .FadeOutFromOne(350, Easing.OutQuad);
                }
            }, true);

            ExpandedState.BindValueChanged(v =>
            {
                icon.ScaleTo(v.NewValue > ModCustomisationPanelState.Collapsed ? new Vector2(1, -1) : Vector2.One, 300, Easing.OutQuint);

                switch (v.NewValue)
                {
                    case ModCustomisationPanelState.Collapsed:
                        background.FadeColour(colourProvider.Dark3, 500, Easing.OutQuint);
                        break;

                    case ModCustomisationPanelState.Expanded:
                    case ModCustomisationPanelState.ExpandedByMod:
                        background.FadeColour(colourProvider.Light4, 500, Easing.OutQuint);
                        break;
                }
            }, true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (!Enabled.Value)
                return base.OnHover(e);

            if (panel.ExpandedState.Value == ModCustomisationPanelState.Collapsed)
                panel.ExpandedState.Value = ModCustomisationPanelState.Expanded;

            hoverBackground.FadeTo(0.4f, 200, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverBackground.FadeOut(200, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
