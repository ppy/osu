// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterface
{
    public class BreadcrumbControl<T> : OsuTabControl<T>
    {
        public const float PADDING = 10;

        protected override TabItem<T> CreateTabItem(T value) => new BreadcrumbTabItem(value);

        public BreadcrumbControl()
        {
            Height = 26;
            TabContainer.Spacing = new Vector2(PADDING, 0f);
            Current.ValueChanged += tab =>
            {
                foreach (TabItem<T> t in TabContainer.Children)
                {
                    var tIndex = TabContainer.IndexOf(t);
                    var tabIndex = TabContainer.IndexOf(TabMap[tab]);
                    var hide = tIndex < tabIndex;

                    t.FadeTo(hide ? 0f : 1f, 500, EasingTypes.OutQuint);
                    t.ScaleTo(new Vector2(hide ? 0.8f : 1f, 1f), 500, EasingTypes.OutQuint);
                    ((BreadcrumbTabItem)t).Chevron.FadeTo(tIndex <= tabIndex ? 0f : 1f, 500, EasingTypes.OutQuint);
                }
            };
        }

        private class BreadcrumbTabItem : OsuTabItem
        {
            public readonly TextAwesome Chevron;

            //don't allow clicking between transitions and don't make the chevron clickable
            protected override bool InternalContains(Vector2 screenSpacePos) => Alpha == 1f && Text.Contains(screenSpacePos);

            public BreadcrumbTabItem(T value) : base(value)
            {
                Text.TextSize = 16;
                Padding = new MarginPadding { Right = PADDING + 8 }; //padding + chevron width
                Add(Chevron = new TextAwesome
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreLeft,
                    TextSize = 12,
                    Icon = FontAwesome.fa_chevron_right,
                    Margin = new MarginPadding { Left = PADDING },
                    Alpha = 0f,
                });
            }
        }
    }
}
