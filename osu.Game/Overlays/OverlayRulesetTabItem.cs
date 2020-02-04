// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Allocation;

namespace osu.Game.Overlays
{
    public class OverlayRulesetTabItem : TabItem<RulesetInfo>
    {
        private Color4 accentColour;

        protected virtual Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                text.FadeColour(value, 120, Easing.OutQuint);
            }
        }

        protected override Container<Drawable> Content { get; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        private readonly OsuSpriteText text;

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
                    Spacing = new Vector2(3, 0),
                    Child = text = new OsuSpriteText
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Text = value.Name,
                    }
                },
                new HoverClickSounds()
            });

            Enabled.Value = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Enabled.BindValueChanged(_ => updateState(), true);
        }

        public override bool PropagatePositionalInputSubTree => Enabled.Value && !Active.Value && base.PropagatePositionalInputSubTree;

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
            text.Font = text.Font.With(weight: Active.Value ? FontWeight.Bold : FontWeight.Medium);
            AccentColour = Enabled.Value ? getActiveColour() : colourProvider.Foreground1;
        }

        private Color4 getActiveColour() => IsHovered || Active.Value ? Color4.White : colourProvider.Highlight1;
    }
}
