﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using System.Linq;

namespace osu.Game.Graphics.UserInterface
{
    public class BreadcrumbControl<T> : OsuTabControl<T>
    {
        private const float padding = 10;

        protected override TabItem<T> CreateTabItem(T value) => new BreadcrumbTabItem(value);

        public BreadcrumbControl()
        {
            Height = 26;
            TabContainer.Spacing = new Vector2(padding, 0f);
            Current.ValueChanged += tab =>
            {
                foreach (var t in TabContainer.Children.OfType<BreadcrumbTabItem>())
                {
                    var tIndex = TabContainer.IndexOf(t);
                    var tabIndex = TabContainer.IndexOf(TabMap[tab]);

                    t.State = tIndex < tabIndex ? Visibility.Hidden : Visibility.Visible;
                    t.Chevron.FadeTo(tIndex <= tabIndex ? 0f : 1f, 500, EasingTypes.OutQuint);
                }
            };
        }

        private class BreadcrumbTabItem : OsuTabItem, IStateful<Visibility>
        {
            public readonly TextAwesome Chevron;

            //don't allow clicking between transitions and don't make the chevron clickable
            protected override bool InternalContains(Vector2 screenSpacePos) => Alpha == 1f && Text.Contains(screenSpacePos);
            public override bool HandleInput => State == Visibility.Visible;

            private Visibility state;
            public Visibility State
            {
                get { return state; }
                set
                {
                    if (value == state) return;
                    state = value;

                    const float transition_duration = 500;

                    if (State == Visibility.Visible)
                    {
                        FadeIn(transition_duration, EasingTypes.OutQuint);
                        ScaleTo(new Vector2(1f), transition_duration, EasingTypes.OutQuint);
                    }
                    else
                    {
                        FadeOut(transition_duration, EasingTypes.OutQuint);
                        ScaleTo(new Vector2(0.8f, 1f), transition_duration, EasingTypes.OutQuint);
                    }
                }
            }

            public BreadcrumbTabItem(T value) : base(value)
            {
                Text.TextSize = 16;
                Padding = new MarginPadding { Right = padding + 8 }; //padding + chevron width
                Add(Chevron = new TextAwesome
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreLeft,
                    TextSize = 12,
                    Icon = FontAwesome.fa_chevron_right,
                    Margin = new MarginPadding { Left = padding },
                    Alpha = 0f,
                });
            }
        }
    }
}
