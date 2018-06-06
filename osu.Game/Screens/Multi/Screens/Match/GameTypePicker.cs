// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;
using OpenTK;

namespace osu.Game.Screens.Multi.Screens.Match
{
    public class GameTypePicker : FillFlowContainer, IHasCurrentValue<GameType>
    {
        private readonly OsuSpriteText tooltip;

        public Bindable<GameType> Current { get; } = new Bindable<GameType>();

        public GameTypePicker()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(7);

            Picker picker;
            Children = new Drawable[]
            {
                picker = new Picker
                {
                    RelativeSizeAxes = Axes.X,
                },
                tooltip = new OsuSpriteText
                {
                    TextSize = 14,
                },
            };

            Current.ValueChanged += t => tooltip.Text = t.Name;

            picker.AddItem(new GameTypeTag());
            picker.AddItem(new GameTypeVersus());
            picker.AddItem(new GameTypeTagTeam());
            picker.AddItem(new GameTypeTeamVersus());

            Current.BindTo(picker.Current);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            tooltip.Colour = colours.Yellow;
        }

        private class Picker : TabControl<GameType>
        {
            private const float height = 40;
            private const float selection_width = 3;

            protected override TabItem<GameType> CreateTabItem(GameType value) => new PickerItem(value);
            protected override Dropdown<GameType> CreateDropdown() => null;

            public Picker()
            {
                Height = height + selection_width * 2;
                TabContainer.Spacing = new Vector2(10 - selection_width * 2);
            }

            private class PickerItem : TabItem<GameType>
            {
                private const float transition_duration = 200;

                private readonly Container selection;

                public PickerItem(GameType value)
                    : base(value)
                {
                    AutoSizeAxes = Axes.Both;

                    Child = selection = new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Alpha = 0,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    };
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    DrawableGameType icon;
                    Add(icon = new DrawableGameType(Value)
                    {
                        Size = new Vector2(height),
                    });

                    selection.Colour = colours.Yellow;
                    icon.Margin = new MarginPadding(selection_width);
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
}
