﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
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
        private const float item_chevron_size = 10;

        protected override TabItem<T> CreateTabItem(T value) => new BreadcrumbTabItem(value)
        {
            AccentColour = AccentColour,
        };

        protected override float StripWidth() => base.StripWidth() - (padding + item_chevron_size);

        public BreadcrumbControl()
        {
            Height = 32;
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
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Alpha == 1f && Text.ReceivePositionalInputAt(screenSpacePos);

            public override bool HandleNonPositionalInput => State == Visibility.Visible;
            public override bool HandlePositionalInput => State == Visibility.Visible;
            public override bool IsRemovable => true;

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
                Text.TextSize = 18;
                Text.Margin = new MarginPadding { Vertical = 8 };
                Padding = new MarginPadding { Right = padding + item_chevron_size };
                Add(Chevron = new SpriteIcon
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(item_chevron_size),
                    Icon = FontAwesome.fa_chevron_right,
                    Margin = new MarginPadding { Left = padding },
                    Alpha = 0f,
                });
            }
        }
    }
}
