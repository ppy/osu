// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Components;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Screens.Match
{
    public class Settings : Container
    {
        public readonly Bindable<string> RoomName = new Bindable<string>();
        public readonly Bindable<int?> MaxParticipants = new Bindable<int?>();
        public readonly Bindable<string> Password = new Bindable<string>();

        public Bindable<RoomAvailability> RoomAvailability => roomAvailabilityTabs.Current;
        public Bindable<GameType> GameType => gameTypeTabs.Current;

        private readonly OsuTabControl<RoomAvailability> roomAvailabilityTabs;
        private readonly OsuTabControl<GameType> gameTypeTabs;

        public Settings()
        {
            OsuTextBox roomNameBox;
            OsuTextBox maxParticipantsBox;
            SettingsPasswordTextBox passwordBox;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"28242d"),
                },
                new Container
                {
                    Margin = new MarginPadding { Vertical = 15, Left = SearchableListOverlay.WIDTH_PADDING / 2 },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.50f,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = @"ROOM NAME",
                            Colour = Color4.White,
                            Font = @"Exo2.0-Bold",
                        },
                        roomNameBox = new SettingsTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                            Margin = new MarginPadding { Top = 20 },
                            OnCommit = onRoomNameCommit,
                        },
                        new OsuSpriteText
                        {
                            Margin = new MarginPadding { Top = 100 },
                            Text = @"ROOM VISIBILITY",
                            Colour = Color4.White,
                            Font = @"Exo2.0-Bold",
                        },
                        roomAvailabilityTabs = new RoomAvailabilityTabControl
                        {
                            Margin = new MarginPadding { Top = 120 },
                            Width = 400,
                            Height = 40,
                        },
                        new OsuSpriteText
                        {
                            Margin = new MarginPadding { Top = 200 },
                            Text = @"GAME TYPE",
                            Colour = Color4.White,
                            Font = @"Exo2.0-Bold",
                        },
                        gameTypeTabs = new GameTypeTabControl
                        {
                            Margin = new MarginPadding { Top = 220 },
                            Width = 400,
                            Height = 80,
                        },
                    }
                },
                new Container
                {
                    Margin = new MarginPadding { Vertical = 15, Right = SearchableListOverlay.WIDTH_PADDING / 2 },
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.35f,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = @"MAX PARTICIPANTS",
                            Colour = Color4.White,
                            Font = @"Exo2.0-Bold",
                        },
                        maxParticipantsBox = new SettingsTextBox
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.X,
                            Margin = new MarginPadding { Top = 20 },
                            OnCommit = onMaxParticipantsCommit,
                        },
                        new OsuSpriteText
                        {
                            Margin = new MarginPadding { Top = 100 },
                            Text = @"PASSWORD (OPTIONAL)",
                            Colour = Color4.White,
                            Font = @"Exo2.0-Bold",
                        },
                        passwordBox = new SettingsPasswordTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                            Margin = new MarginPadding { Top = 120 },
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            OnCommit = onPasswordCommmit,
                        },
                    },

                },
            };

            RoomName.BindValueChanged(n => roomNameBox.Text = n, true);
            MaxParticipants.BindValueChanged(p => maxParticipantsBox.Text = p.ToString(), true);
            Password.BindValueChanged(p => passwordBox.Text = p, true);
        }

        private void onRoomNameCommit(TextBox box, bool newText) => RoomName.Value = box.Text;

        private void onMaxParticipantsCommit(TextBox box, bool newText)
        {
            if (int.TryParse(box.Text, out int number))
                MaxParticipants.Value = number;
            else
                box.Text = MaxParticipants.Value.ToString();
        }

        private void onPasswordCommmit(TextBox box, bool newText) => Password.Value = box.Text;

        private class SettingsTextBox : OsuTextBox
        {
            protected override void OnFocusLost(InputState state)
            {
                base.OnFocusLost(state);

                OnCommit?.Invoke(this, true);
            }
        }
    }
}
