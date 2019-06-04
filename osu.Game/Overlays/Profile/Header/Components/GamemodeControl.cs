// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class GamemodeControl : TabControl<string>
    {
        protected override Dropdown<string> CreateDropdown() => null;

        protected override TabItem<string> CreateTabItem(string value) => new GamemodeTabItem(value)
        {
            AccentColour = AccentColour
        };

        private Color4 accentColour = Color4.White;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (accentColour == value)
                    return;

                accentColour = value;

                foreach (TabItem<string> tabItem in TabContainer)
                {
                    ((GamemodeTabItem)tabItem).AccentColour = value;
                }
            }
        }

        public GamemodeControl()
        {
            TabContainer.Masking = false;
            TabContainer.Spacing = new Vector2(15, 0);
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            foreach (var r in rulesets.AvailableRulesets)
                AddItem(r.Name);
        }

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            Direction = FillDirection.Horizontal,
            AutoSizeAxes = Axes.Both,
        };

        private class GamemodeTabItem : TabItem<string>
        {
            private readonly OsuSpriteText text;

            private Color4 accentColour;

            public Color4 AccentColour
            {
                get => accentColour;
                set
                {
                    if (accentColour == value)
                        return;

                    accentColour = value;

                    updateState();
                }
            }

            public GamemodeTabItem(string value)
                : base(value)
            {
                AutoSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Bottom = 10 },
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        Text = value,
                        Font = OsuFont.GetFont()
                    },
                    new HoverClickSounds()
                };
            }

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
                if (Active.Value || IsHovered)
                {
                    text.FadeColour(Color4.White, 120, Easing.InQuad);

                    if (Active.Value)
                        text.Font = text.Font.With(weight: FontWeight.Bold);
                }
                else
                {
                    text.FadeColour(AccentColour, 120, Easing.InQuad);
                    text.Font = text.Font.With(weight: FontWeight.Medium);
                }
            }
        }
    }
}
