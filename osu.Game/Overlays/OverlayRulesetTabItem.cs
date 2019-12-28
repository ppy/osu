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

namespace osu.Game.Overlays
{
    public class OverlayRulesetTabItem : TabItem<RulesetInfo>, IHasAccentColour
    {
        protected readonly OsuSpriteText Text;
        private readonly FillFlowContainer content;

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (accentColour == value)
                    return;

                accentColour = value;

                UpdateState();
            }
        }

        protected override Container<Drawable> Content => content;

        public OverlayRulesetTabItem(RulesetInfo value)
            : base(value)
        {
            AutoSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]
            {
                content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(3, 0),
                    Child = Text = new OsuSpriteText
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Text = value.Name,
                    }
                },
                new HoverClickSounds()
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);
            UpdateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            UpdateState();
        }

        protected override void OnActivated() => UpdateState();

        protected override void OnDeactivated() => UpdateState();

        protected virtual void UpdateState()
        {
            Text.Font = Text.Font.With(weight: Active.Value ? FontWeight.Bold : FontWeight.Medium);
            Text.FadeColour(IsHovered || Active.Value ? Color4.White : AccentColour, 120, Easing.OutQuint);
        }
    }
}
