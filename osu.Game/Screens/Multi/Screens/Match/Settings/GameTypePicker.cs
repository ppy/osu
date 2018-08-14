// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Input.States;

namespace osu.Game.Screens.Multi.Screens.Match.Settings
{
    public class GameTypePicker : TabControl<GameType>
    {
        private const float height = 40;
        private const float selection_width = 3;

        protected override TabItem<GameType> CreateTabItem(GameType value) => new GameTypePickerItem(value);
        protected override Dropdown<GameType> CreateDropdown() => null;

        public GameTypePicker()
        {
            Height = height + selection_width * 2;
            TabContainer.Spacing = new Vector2(10 - selection_width * 2);

            AddItem(new GameTypeTag());
            AddItem(new GameTypeVersus());
            AddItem(new GameTypeTagTeam());
            AddItem(new GameTypeTeamVersus());
        }

        private class GameTypePickerItem : TabItem<GameType>
        {
            private const float transition_duration = 200;

            private readonly CircularContainer hover, selection;

            public GameTypePickerItem(GameType value) : base(value)
            {
                AutoSizeAxes = Axes.Both;

                DrawableGameType icon;
                Children = new Drawable[]
                {
                    selection = new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Alpha = 0,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                    icon = new DrawableGameType(Value)
                    {
                        Size = new Vector2(height),
                        Margin = new MarginPadding(selection_width),
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(selection_width),
                        Child = hover = new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            Alpha = 0,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                selection.Colour = colours.Yellow;
            }

            protected override bool OnHover(InputState state)
            {
                hover.FadeTo(0.05f, transition_duration, Easing.OutQuint);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                hover.FadeOut(transition_duration, Easing.OutQuint);
                base.OnHoverLost(state);
            }

            protected override void OnActivated()
            {
                selection.FadeIn(transition_duration, Easing.OutQuint);
            }

            protected override void OnDeactivated()
            {
                selection.FadeOut(transition_duration, Easing.OutQuint);
            }
        }
    }
}
