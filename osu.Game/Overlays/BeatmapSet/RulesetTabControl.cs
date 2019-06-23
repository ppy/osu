// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class RulesetTabControl : TabControl<RulesetInfo>
    {
        private readonly Box bar;

        private BeatmapSetInfo beatmapSet;

        public BeatmapSetInfo BeatmapSet
        {
            get => beatmapSet;
            set
            {
                beatmapSet = value;

                this.FadeTo(beatmapSet != null ? 1 : 0.25f);

                foreach (var tabItem in TabContainer)
                    ((RulesetTabItem)tabItem).BeatmapsCount = value?.Beatmaps.FindAll(r => r.Ruleset.ID == tabItem.Value.ID).Count ?? 0;
            }
        }

        protected override Dropdown<RulesetInfo> CreateDropdown() => null;

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new RulesetTabItem(value)
        {
            Anchor = TabContainer.Anchor,
            Origin = TabContainer.Origin,
        };

        public RulesetTabControl()
        {
            TabContainer.Anchor = Anchor.BottomCentre;
            TabContainer.Origin = Anchor.BottomCentre;
            TabContainer.Spacing = new Vector2(15, 0);

            AddInternal(bar = new Box
            {
                Anchor = TabContainer.Anchor,
                Origin = TabContainer.Origin,
                RelativeSizeAxes = Axes.X,
                Height = 2,
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            bar.Colour = colours.Yellow;
        }

        /// <summary>
        ///  Selects a <see cref="RulesetTabItem"/> that contains a matching <see cref="RulesetInfo"/>.
        /// </summary>
        public void SelectRuleset(RulesetInfo ruleset)
        {
            if (ruleset == null)
                return;

            foreach (var tab in TabContainer)
                if (tab.Value.ID == ruleset.ID)
                    SelectTab(tab);
        }

        private class RulesetTabItem : TabItem<RulesetInfo>
        {
            private readonly OsuSpriteText text;
            private readonly ExpandingBar bar;

            private readonly OsuSpriteText count;
            private readonly Container countContainer;

            private OsuColour colours;

            public int BeatmapsCount
            {
                set
                {
                    Enabled.Value = value > 0;
                    count.Text = Enabled.Value ? value.ToString() : null;

                    updateState();
                }
            }

            public override bool PropagatePositionalInputSubTree => Enabled.Value;

            public RulesetTabItem(RulesetInfo value)
                : base(value)
            {
                AutoSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Margin = new MarginPadding { Bottom = 10 },
                        Children = new Drawable[]
                        {
                            text = new OsuSpriteText
                            {
                                Text = value.Name,
                                Font = OsuFont.GetFont(),
                            },
                            countContainer = new Container
                            {
                                Margin = new MarginPadding { Left = 4 },
                                AutoSizeAxes = Axes.Both,
                                CornerRadius = 5,
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    count = new OsuSpriteText
                                    {
                                        Font = OsuFont.GetFont(),
                                        Padding = new MarginPadding { Left = 5, Right = 6, Bottom = 1 },
                                    },
                                },
                            },
                        },
                    },
                    bar = new ExpandingBar
                    {
                        Anchor = Anchor.BottomCentre,
                        ExpandedSize = 10f,
                        CollapsedSize = 0f,
                    },
                    new HoverClickSounds(),
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                this.colours = colours;

                text.Colour = colours.GrayC;
                bar.Colour = colours.Yellow;

                count.Colour = colours.Gray3;
                countContainer.EdgeEffect = new EdgeEffectParameters
                {
                    Roundness = 1.25f,
                    Radius = 1,
                    Colour = colours.GrayC,
                    Type = EdgeEffectType.Shadow,
                };

                updateState();
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
                this.FadeTo(Enabled.Value ? 1 : 0.25f);

                if ((Active.Value || IsHovered) && Enabled.Value)
                {
                    text.FadeColour(Color4.White, 120, Easing.InQuad);
                    bar.Expand();

                    text.Font = text.Font.With(weight: Active.Value ? FontWeight.Bold : FontWeight.Medium);
                }
                else
                {
                    text.FadeColour(colours.GrayC, 120, Easing.InQuad);
                    bar.Collapse();

                    text.Font = text.Font.With(weight: FontWeight.Medium);
                }
            }
        }
    }
}
