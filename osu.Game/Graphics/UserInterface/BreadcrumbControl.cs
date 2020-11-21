﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using System.Linq;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class BreadcrumbControl<T> : OsuTabControl<T>
    {
        private const float padding = 10;

        protected override TabItem<T> CreateTabItem(T value) => new BreadcrumbTabItem(value)
        {
            AccentColour = AccentColour,
        };

        protected override float StripWidth => base.StripWidth - TabContainer.FirstOrDefault()?.Padding.Right ?? 0;

        public BreadcrumbControl()
        {
            Height = 32;
            TabContainer.Spacing = new Vector2(padding, 0f);
            SwitchTabOnRemove = false;

            Current.ValueChanged += index =>
            {
                foreach (var t in TabContainer.Children.OfType<BreadcrumbTabItem>())
                {
                    var tIndex = TabContainer.IndexOf(t);
                    var tabIndex = TabContainer.IndexOf(TabMap[index.NewValue]);

                    t.State = tIndex > tabIndex ? Visibility.Hidden : Visibility.Visible;
                    t.Chevron.FadeTo(tIndex >= tabIndex ? 0f : 1f, 500, Easing.OutQuint);
                }
            };
        }

        public class BreadcrumbTabItem : OsuTabItem, IStateful<Visibility>
        {
            protected virtual float ChevronSize => 10;

            public event Action<Visibility> StateChanged;

            public readonly SpriteIcon Chevron;

            //don't allow clicking between transitions and don't make the chevron clickable
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Alpha == 1f && Text.ReceivePositionalInputAt(screenSpacePos);

            public override bool HandleNonPositionalInput => State == Visibility.Visible;
            public override bool HandlePositionalInput => State == Visibility.Visible;

            private Visibility state;

            public Visibility State
            {
                get => state;
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

            public override void Hide() => State = Visibility.Hidden;

            public override void Show() => State = Visibility.Visible;

            public BreadcrumbTabItem(T value)
                : base(value)
            {
                Text.Font = Text.Font.With(size: 18);
                Text.Margin = new MarginPadding { Vertical = 8 };
                Padding = new MarginPadding { Right = padding + ChevronSize };
                Add(Chevron = new SpriteIcon
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(ChevronSize),
                    Icon = FontAwesome.Solid.ChevronRight,
                    Margin = new MarginPadding { Left = padding },
                    Alpha = 0f,
                });
            }
        }
    }
}
