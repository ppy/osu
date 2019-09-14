// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets;
using osu.Game.Users;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;
using osu.Game.Graphics.UserInterface;
using System.Collections.Generic;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsHeader : CompositeDrawable
    {
        private const int content_height = 250;

        public IEnumerable<Spotlight> Spotlights
        {
            get => dropdown.Items;
            set => dropdown.Items = value;
        }

        public readonly Bindable<RankingsScope> Scope = new Bindable<RankingsScope>();
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();
        public readonly Bindable<Country> Country = new Bindable<Country>();
        public readonly Bindable<Spotlight> Spotlight = new Bindable<Spotlight>();

        private readonly OsuDropdown<Spotlight> dropdown;

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
                        Current = Ruleset
                    },
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = content_height,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                Child = new HeaderBackground(),
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 20),
                                Children = new Drawable[]
                                {
                                    new RankingsScopeSelector
                                    {
                                        Margin = new MarginPadding { Top = 10 },
                                        Current = Scope
                                    },
                                    new HeaderTitle
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Margin = new MarginPadding { Top = 10 },
                                        Scope = { BindTarget = Scope },
                                        Country = { BindTarget = Country },
                                    },
                                    dropdown = new OsuDropdown<Spotlight>
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.8f,
                                        Current = Spotlight,
                                    }
                                }
                            },
                        }
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            Scope.BindValueChanged(onScopeChanged, true);
            base.LoadComplete();
        }

        private void onScopeChanged(ValueChangedEvent<RankingsScope> scope) =>
            dropdown.FadeTo(scope.NewValue == RankingsScope.Spotlights ? 1 : 0, 200, Easing.OutQuint);

        private class HeaderBackground : Sprite
        {
            public HeaderBackground()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
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
