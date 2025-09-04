// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Matchmaking;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class MatchmakingRulesetSelector : CompositeDrawable
    {
        private const float icon_size = 36;

        public readonly Bindable<MatchmakingSettings> SelectedSettings = new Bindable<MatchmakingSettings>(new MatchmakingSettings());

        public MatchmakingRulesetSelector()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer<SelectorButton>
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(3),
                Children =
                [
                    new SelectorButton(OsuIcon.RulesetOsu)
                    {
                        Settings = new MatchmakingSettings { RulesetId = 0 },
                        SelectedSettings = { BindTarget = SelectedSettings }
                    },
                    new SelectorButton(OsuIcon.RulesetTaiko)
                    {
                        Settings = new MatchmakingSettings { RulesetId = 1 },
                        SelectedSettings = { BindTarget = SelectedSettings }
                    },
                    new SelectorButton(OsuIcon.RulesetCatch)
                    {
                        Settings = new MatchmakingSettings { RulesetId = 2 },
                        SelectedSettings = { BindTarget = SelectedSettings }
                    },
                    new ManiaSelectorButton(4)
                    {
                        Settings = new MatchmakingSettings
                        {
                            RulesetId = 3,
                            Variant = 4
                        },
                        SelectedSettings = { BindTarget = SelectedSettings }
                    },
                    new ManiaSelectorButton(7)
                    {
                        Settings = new MatchmakingSettings
                        {
                            RulesetId = 3,
                            Variant = 7
                        },
                        SelectedSettings = { BindTarget = SelectedSettings }
                    }
                ]
            };
        }

        private partial class SelectorButton : CompositeDrawable
        {
            public required MatchmakingSettings Settings { get; init; }

            public readonly Bindable<MatchmakingSettings> SelectedSettings = new Bindable<MatchmakingSettings>(new MatchmakingSettings());

            private readonly IconUsage icon;
            private Drawable iconSprite = null!;

            public SelectorButton(IconUsage icon)
            {
                this.icon = icon;

                Size = new Vector2(icon_size);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new OsuAnimatedButton
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = iconSprite = CreateIcon(),
                    Action = () => SelectedSettings.Value = Settings
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                SelectedSettings.BindValueChanged(onSelectionChanged, true);
                FinishTransforms(true);
            }

            private void onSelectionChanged(ValueChangedEvent<MatchmakingSettings> selection)
            {
                if (selection.NewValue.Equals(Settings))
                    iconSprite.FadeColour(Color4.Gold, 100, Easing.OutQuint);
                else
                    iconSprite.FadeColour(OsuColour.Gray(0.5f), 100);
            }

            protected override bool OnClick(ClickEvent e)
            {
                SelectedSettings.Value = Settings;
                return true;
            }

            protected virtual Drawable CreateIcon() => new SpriteIcon
            {
                RelativeSizeAxes = Axes.Both,
                Icon = icon
            };
        }

        private partial class ManiaSelectorButton : SelectorButton
        {
            private readonly int variant;

            public ManiaSelectorButton(int variant)
                : base(OsuIcon.RulesetMania)
            {
                this.variant = variant;
            }

            protected override Drawable CreateIcon() => new BufferedContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        RelativeSizeAxes = Axes.Both,
                        Icon = OsuIcon.RulesetMania
                    },
                    new Container
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Size = new Vector2(14, 10),
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = $"{variant}K",
                                Font = OsuFont.Default.With(size: 8, fixedWidth: true, weight: FontWeight.Bold),
                                UseFullGlyphHeight = false,
                                Blending = new BlendingParameters
                                {
                                    AlphaEquation = BlendingEquation.ReverseSubtract
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
