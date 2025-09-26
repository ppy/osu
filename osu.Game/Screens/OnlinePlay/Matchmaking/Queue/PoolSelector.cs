// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Matchmaking;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public partial class PoolSelector : CompositeDrawable
    {
        private const float icon_size = 48;

        public readonly Bindable<MatchmakingPool[]> AvailablePools = new Bindable<MatchmakingPool[]>();
        public readonly Bindable<MatchmakingPool?> SelectedPool = new Bindable<MatchmakingPool?>();

        private FillFlowContainer<SelectorButton> poolFlow = null!;

        public PoolSelector()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = poolFlow = new FillFlowContainer<SelectorButton>
            {
                AutoSizeAxes = Axes.X,
                Height = icon_size * 1.2f,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AvailablePools.BindValueChanged(pools =>
            {
                poolFlow.Clear();

                foreach (var p in pools.NewValue)
                {
                    poolFlow.Add(new SelectorButton(p)
                    {
                        SelectedPool = { BindTarget = SelectedPool },
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    });
                }
            }, true);
        }

        private partial class SelectorButton : OsuAnimatedButton
        {
            public readonly Bindable<MatchmakingPool?> SelectedPool = new Bindable<MatchmakingPool?>();

            [Resolved]
            private RulesetStore rulesetStore { get; set; } = null!;

            private readonly MatchmakingPool pool;
            private Drawable iconSprite = null!;

            private Box flashLayer = null!;

            public SelectorButton(MatchmakingPool pool)
            {
                this.pool = pool;

                Size = new Vector2(icon_size);
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Content.Masking = true;
                Content.CornerRadius = 20;
                Content.CornerExponent = 10;

                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = colourProvider.Background2,
                        Alpha = 0.4f,
                        RelativeSizeAxes = Axes.Both,
                    },
                    flashLayer = new Box
                    {
                        Colour = Color4.White,
                        Blending = BlendingParameters.Additive,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(10),
                        Children = new[]
                        {
                            iconSprite = createIcon(),
                        }
                    },
                };

                Action = () => SelectedPool.Value = pool;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                SelectedPool.BindValueChanged(onSelectionChanged, true);
                FinishTransforms(true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                if (!isSelected)
                    flashLayer.FadeTo(0.05f, 200, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                if (!isSelected)
                    flashLayer.FadeTo(0f, 200, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            private bool isSelected => SelectedPool.Value?.Equals(pool) == true;

            private void onSelectionChanged(ValueChangedEvent<MatchmakingPool?> selection)
            {
                if (isSelected)
                {
                    this.ScaleTo(1.2f, 200, Easing.OutQuint);
                    iconSprite.FadeColour(Color4.Gold, 100, Easing.OutQuint);
                    flashLayer.FadeTo(0.1f, 200, Easing.OutQuint);
                }
                else
                {
                    this.ScaleTo(1f, 200, Easing.OutQuint);
                    iconSprite.FadeColour(OsuColour.Gray(0.5f), 100);
                    flashLayer.FadeOut(200, Easing.OutQuint);
                }
            }

            private Drawable createIcon()
            {
                Ruleset? rulesetInstance = rulesetStore.GetRuleset(pool.RulesetId)?.CreateInstance();
                if (rulesetInstance == null)
                    return Empty();

                Drawable icon = rulesetInstance.CreateIcon().With(d => d.RelativeSizeAxes = Axes.Both);

                if (pool.Variant == 0)
                    return icon;

                return new BufferedContainer(pixelSnapping: true)
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        icon,
                        new Container
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Size = icon_size * new Vector2(0.4f, 0.28f),
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
                                    Text = $"{pool.Variant}K",
                                    Font = OsuFont.Default.With(size: icon_size * 0.3f, weight: FontWeight.Bold),
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
}
