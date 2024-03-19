// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays
{
    public partial class OverlayRulesetTabItem : TabItem<RulesetInfo>, IHasTooltip
    {
        private Color4 accentColour;

        protected virtual Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                icon.FadeColour(value, 120, Easing.OutQuint);
            }
        }

        protected override Container<Drawable> Content { get; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private readonly Drawable icon;

        public LocalisableString TooltipText => Value.Name;

        public OverlayRulesetTabItem(RulesetInfo value)
            : base(value)
        {
            AutoSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]
            {
                Content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(4, 0),
                    Child = icon = new ConstrainedIconContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(20f),
                        Icon = value.CreateInstance().CreateIcon(),
                    },
                },
                new HoverSounds()
            });

            Enabled.Value = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Enabled.BindValueChanged(_ => updateState(), true);
        }

        public override bool PropagatePositionalInputSubTree => Enabled.Value && base.PropagatePositionalInputSubTree;

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateState();
        }

        protected override void OnActivated() => updateState();

        protected override void OnDeactivated() => updateState();

        private void updateState()
        {
            AccentColour = Enabled.Value ? getActiveColour() : colourProvider.Foreground1;
        }

        private Color4 getActiveColour() => IsHovered || Active.Value ? Color4.White : colourProvider.Highlight1;
    }
}
