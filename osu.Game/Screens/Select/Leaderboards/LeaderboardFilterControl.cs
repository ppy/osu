// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using System;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class LeaderboardFilterControl : Container
    {
        public static readonly float HEIGHT = 31;

        private LeaderboardTabControl<LeaderboardTab> tabs;
        private ModCheckbox mods;

        public Action<LeaderboardTab, bool> Action; //passed the selected tab and if mods is checked

        private void invokeAction()
        {
            Action?.Invoke(tabs.SelectedItem, mods.State == CheckBoxState.Checked);
        }

        public LeaderboardFilterControl()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Colour = Color4.White.Opacity(0.2f),
                },
                tabs = new LeaderboardTabControl<LeaderboardTab>
                {
                    RelativeSizeAxes = Axes.Both,
                },
                mods = new ModCheckbox
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                }
            };

            tabs.ItemChanged += (sender, e) => invokeAction();
            mods.Action = obj => invokeAction();
        }

        private class LeaderboardTabControl<T> : OsuTabControl<T>
        {
            //protected override TabItem<T> CreateTabItem(T value) => new LeaderboardTabItem<T> { Value = value };

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.GreenLight;
            }

            public LeaderboardTabControl()
            {
                TabContainer.Spacing = new Vector2(10f, 0f);
            }
        }

        //private class LeaderboardTabItem<T> : OsuTabItem<T>
        //{
        //    public LeaderboardTabItem()
        //    {
        //        Text.Margin = new MarginPadding { Top = 8, Bottom = 8, };
        //    }
        //}

        private class ModCheckbox : CheckBox
        {
            private const float transition_length = 500;
            private Color4 accentColour;

            private Box box;
            private SpriteText text;
            private TextAwesome icon;

            public Action<CheckBoxState> Action;

            private void fadeIn()
            {
                box.FadeIn(transition_length, EasingTypes.OutQuint);
                text.FadeColour(Color4.White, transition_length, EasingTypes.OutQuint);
            }

            private void fadeOut()
            {
                box.FadeOut(transition_length, EasingTypes.OutQuint);
                text.FadeColour(accentColour, transition_length, EasingTypes.OutQuint);
            }

            protected override void OnChecked()
            {
                fadeIn();
                icon.Icon = FontAwesome.fa_check_circle_o;
                Action?.Invoke(State);
            }

            protected override void OnUnchecked()
            {
                fadeOut();
                icon.Icon = FontAwesome.fa_circle_o;
                Action?.Invoke(State);
            }

            protected override bool OnHover(InputState state)
            {
                fadeIn();
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                if (State == CheckBoxState.Unchecked)
                    fadeOut();

                base.OnHoverLost(state);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                accentColour = colours.YellowLight;
                text.Colour = accentColour;
                icon.Colour = accentColour;
            }

            public ModCheckbox()
            {
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Margin = new MarginPadding { Top = 8, Bottom = 8, },
                        Spacing = new Vector2(5f, 0f),
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            text = new OsuSpriteText
                            {
                                TextSize = 14,
                                Font = @"Exo2.0-Bold",
                                Text = @"Mods",
                            },
                            icon = new TextAwesome
                            {
                                TextSize = 14,
                                Icon = FontAwesome.fa_circle_o,
                                Shadow = true,
                            },
                        },
                    },
                    box = new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 1,
                        Alpha = 0,
                        Colour = Color4.White,
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                    }
                };
            }
        }
    }

    public enum LeaderboardTab
    {
        Local,
        Country,
        Global,
        Friends
    }
}
