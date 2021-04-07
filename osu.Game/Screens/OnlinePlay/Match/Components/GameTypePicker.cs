// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.GameTypes;
using osu.Game.Screens.OnlinePlay.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public class GameTypePicker : DisableableTabControl<GameType>
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
            AddItem(new GameTypePlaylists());
        }

        private class GameTypePickerItem : DisableableTabItem
        {
            private const float transition_duration = 200;

            private readonly CircularContainer hover, selection;

            public GameTypePickerItem(GameType value)
                : base(value)
            {
                AutoSizeAxes = Axes.Both;

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
                    new DrawableGameType(Value)
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

            protected override bool OnHover(HoverEvent e)
            {
                hover.FadeTo(0.05f, transition_duration, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                hover.FadeOut(transition_duration, Easing.OutQuint);
                base.OnHoverLost(e);
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
