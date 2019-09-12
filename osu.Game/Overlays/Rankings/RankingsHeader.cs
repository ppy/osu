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
        private const int dropdown_height = 50;
        private const int spacing = 20;
        private const int title_offset = 30;
        private const int duration = 200;

        public IEnumerable<Spotlight> Spotlights
        {
            get => dropdown.Items;
            set => dropdown.Items = value;
        }

        public readonly Bindable<RankingsScope> Scope = new Bindable<RankingsScope>();
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();
        public readonly Bindable<Country> Country = new Bindable<Country>();
        public readonly Bindable<Spotlight> Spotlight = new Bindable<Spotlight>();

        private readonly Container dropdownPlaceholder;
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
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                Child = new HeaderBackground(),
                            },
                            new RankingsScopeSelector
                            {
                                Margin = new MarginPadding { Top = 10 },
                                Current = { BindTarget = Scope }
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Y = title_offset,
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, spacing),
                                Children = new Drawable[]
                                {
                                    new HeaderTitle
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Scope = { BindTarget = Scope },
                                        Country = { BindTarget = Country },
                                    },
                                    dropdownPlaceholder = new Container
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        RelativeSizeAxes = Axes.X,
                                        Height = dropdown_height,
                                        Width = 0.8f,
                                        AlwaysPresent = true,
                                        Child = dropdown = new OsuDropdown<Spotlight>
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Current = { BindTarget = Spotlight },
                                        }
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
            Scope.BindValueChanged(scope => onScopeChanged(scope.NewValue), true);
            base.LoadComplete();
        }

        private void onScopeChanged(RankingsScope scope) =>
            dropdownPlaceholder.FadeTo(scope == RankingsScope.Spotlights ? 1 : 0, duration, Easing.OutQuint);

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
