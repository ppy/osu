// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Matchmaking;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class MatchmakingPoolSelector : CompositeDrawable
    {
        private const float icon_size = 36;

        public readonly Bindable<MatchmakingPool[]> AvailablePools = new Bindable<MatchmakingPool[]>();
        public readonly Bindable<MatchmakingPool?> SelectedPool = new Bindable<MatchmakingPool?>();

        private FillFlowContainer<SelectorButton> poolFlow = null!;

        public MatchmakingPoolSelector()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = poolFlow = new FillFlowContainer<SelectorButton>
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(3)
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AvailablePools.BindValueChanged(pools =>
            {
                poolFlow.Clear();
                foreach (var p in pools.NewValue)
                    poolFlow.Add(new SelectorButton(p) { SelectedPool = { BindTarget = SelectedPool } });
            }, true);
        }

        private partial class SelectorButton : CompositeDrawable
        {
            public readonly Bindable<MatchmakingPool?> SelectedPool = new Bindable<MatchmakingPool?>();

            [Resolved]
            private RulesetStore rulesetStore { get; set; } = null!;

            private readonly MatchmakingPool pool;
            private Drawable iconSprite = null!;

            public SelectorButton(MatchmakingPool pool)
            {
                this.pool = pool;

                Size = new Vector2(icon_size);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new OsuAnimatedButton
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = iconSprite = createIcon(),
                    Action = () => SelectedPool.Value = pool
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                SelectedPool.BindValueChanged(onSelectionChanged, true);
                FinishTransforms(true);
            }

            private void onSelectionChanged(ValueChangedEvent<MatchmakingPool?> selection)
            {
                if (selection.NewValue?.Equals(pool) == true)
                    iconSprite.FadeColour(Color4.Gold, 100, Easing.OutQuint);
                else
                    iconSprite.FadeColour(OsuColour.Gray(0.5f), 100);
            }

            private Drawable createIcon()
            {
                Ruleset? rulesetInstance = rulesetStore.GetRuleset(pool.RulesetId)?.CreateInstance();
                if (rulesetInstance == null)
                    return Empty();

                Drawable icon = rulesetInstance.CreateIcon().With(d => d.RelativeSizeAxes = Axes.Both);

                if (pool.Variant == 0)
                    return icon;

                return new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        icon,
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
                                    Text = $"{pool.Variant}K",
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
}
