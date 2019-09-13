// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Users;
using osu.Framework.Graphics;
using osuTK;
using osu.Game.Graphics;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Rankings
{
    public class HeaderTitle : CompositeDrawable
    {
        private const int spacing = 10;
        private const int flag_margin = 5;
        private const int text_size = 40;

        public readonly Bindable<RankingsScope> Scope = new Bindable<RankingsScope>();
        public readonly Bindable<Country> Country = new Bindable<Country>();

        private readonly SpriteText scopeText;
        private readonly DismissableFlag flag;

        public HeaderTitle()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(spacing, 0),
                Children = new Drawable[]
                {
                    flag = new DismissableFlag
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Margin = new MarginPadding { Bottom = flag_margin },
                        Size = new Vector2(30, 20),
                    },
                    scopeText = new SpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Light)
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Light),
                        Text = @"Ranking"
                    }
                }
            };

            flag.Action += () => Country.Value = null;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            scopeText.Colour = colours.Lime;
        }

        protected override void LoadComplete()
        {
            Scope.BindValueChanged(onScopeChanged, true);
            Country.BindValueChanged(onCountryChanged, true);
            base.LoadComplete();
        }

        private void onScopeChanged(ValueChangedEvent<RankingsScope> scope)
        {
            scopeText.Text = scope.NewValue.ToString();

            if (scope.NewValue != RankingsScope.Performance)
                Country.Value = null;
        }

        private void onCountryChanged(ValueChangedEvent<Country> country)
        {
            if (country.NewValue == null)
            {
                flag.Hide();
                return;
            }

            Scope.Value = RankingsScope.Performance;

            flag.Country = country.NewValue;
            flag.Show();
        }
    }
}
