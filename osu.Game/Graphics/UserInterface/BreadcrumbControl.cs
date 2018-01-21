// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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
                    t.Chevron.FadeTo(tIndex <= tabIndex ? 0f : 1f, 500, Easing.OutQuint);
                }
            };
        }

        private class BreadcrumbTabItem : OsuTabItem, IStateful<Visibility>
        {
            public event Action<Visibility> StateChanged;

            public readonly SpriteIcon Chevron;

            //don't allow clicking between transitions and don't make the chevron clickable
            public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => Alpha == 1f && Text.ReceiveMouseInputAt(screenSpacePos);

            public override bool HandleKeyboardInput => State == Visibility.Visible;
            public override bool HandleMouseInput => State == Visibility.Visible;

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
                        this.FadeIn(transition_duration, Easing.OutQuint);
                        this.ScaleTo(new Vector2(1f), transition_duration, Easing.OutQuint);
                    }
                    else
                    {
                        this.FadeOut(transition_duration, Easing.OutQuint);
                        this.ScaleTo(new Vector2(0.8f, 1f), transition_duration, Easing.OutQuint);
                    }

                    StateChanged?.Invoke(State);
                }
            }

            public BreadcrumbTabItem(T value) : base(value)
            {
                Text.TextSize = 16;
                Padding = new MarginPadding { Right = padding + 8 }; //padding + chevron width
                Add(Chevron = new SpriteIcon
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(12),
                    Icon = FontAwesome.fa_chevron_right,
                    Margin = new MarginPadding { Left = padding },
                    Alpha = 0f,
                });
            }
        }
    }
}
