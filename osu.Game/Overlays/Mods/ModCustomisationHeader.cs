// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Mods
{
    public partial class ModCustomisationHeader : OsuHoverContainer
    {
        public override bool HandlePositionalInput => true;

        private Box background = null!;
        private SpriteIcon icon = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        protected override IEnumerable<Drawable> EffectTargets => new[] { background };

        public readonly BindableBool Expanded = new BindableBool();

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

            IdleColour = colourProvider.Dark3;
            HoverColour = colourProvider.Light4;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(v =>
            {
                icon.RotateTo(v.NewValue ? 180 : 0);
            }, true);

            Action = Expanded.Toggle;
        }
    }
}
