// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Framework.Bindables;
using osu.Game.Rulesets;
using osu.Game.Users;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsHeader : CompositeDrawable
    {
        private const int content_height = 250;

        public readonly Bindable<RankingsScope> Scope = new Bindable<RankingsScope>();
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();
        public readonly Bindable<Country> Country = new Bindable<Country>();

        public RankingsHeader()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddInternal(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new RankingsRulesetSelector
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Current = { BindTarget = Ruleset }
                    },
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = content_height,
                        Children = new Drawable[]
                        {
                            new HeaderBackground(),
                            new RankingsScopeSelector
                            {
                                Margin = new MarginPadding { Top = 10 },
                                Current = { BindTarget = Scope }
                            },
                            new HeaderTitle
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Scope = { BindTarget = Scope },
                                Country = { BindTarget = Country },
                            }
                        }
                    }
                }
            });
        }

        public class HeaderBackground : Sprite
        {
            public HeaderBackground()
            {
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fill;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(@"Headers/rankings");
            }
        }
    }
}
