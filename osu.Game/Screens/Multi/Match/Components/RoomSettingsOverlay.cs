// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class RoomSettingsOverlay : FocusedOverlayContainer
    {
        private const float transition_duration = 350;
        private const float field_padding = 45;
        private const float disabled_alpha = 0.2f;

        private readonly RoomBindings bindings = new RoomBindings();

        private readonly Container content;

        private readonly OsuSpriteText typeLabel;

        protected readonly OsuTextBox NameField, MaxParticipantsField;
        protected readonly OsuDropdown<TimeSpan> DurationField;
        protected readonly RoomAvailabilityPicker AvailabilityPicker;
        protected readonly GameTypePicker TypePicker;
        protected readonly TriangleButton ApplyButton;
        protected readonly OsuPasswordTextBox PasswordField;

        private readonly Room room;

        [Resolved(CanBeNull = true)]
        private IRoomManager manager { get; set; }

        public RoomSettingsOverlay(Room room)
        {
            this.room = room;

            bindings.Room = room;

            Masking = true;

            Child = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.FromHex(@"28242d"),
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = 35, Bottom = 75, Horizontal = SearchableListOverlay.WIDTH_PADDING },
                        Children = new[]
                        {
                            new SectionContainer
                            {
                                Padding = new MarginPadding { Right = field_padding / 2 },
                                Children = new[]
                                {
                                    new Section("Room name")
                                    {
                                        Child = NameField = new SettingsTextBox
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            TabbableContentContainer = this,
                                            OnCommit = (sender, text) => apply(),
                                        },
                                    },
                                    new Section("Room visibility")
                                    {
                                        Alpha = disabled_alpha,
                                        Child = AvailabilityPicker = new RoomAvailabilityPicker(),
                                    },
                                    new Section("Game type")
                                    {
                                        Alpha = disabled_alpha,
                                        Child = new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Y,
                                            RelativeSizeAxes = Axes.X,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(7),
                                            Children = new Drawable[]
                                            {
                                                TypePicker = new GameTypePicker
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                },
                                                typeLabel = new OsuSpriteText
                                                {
                                                    TextSize = 14,
                                                },
                                            },
                                        },
                                    },
                                },
                            },
                            new SectionContainer
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Padding = new MarginPadding { Left = field_padding / 2 },
                                Children = new[]
                                {
                                    new Section("Max participants")
                                    {
                                        Alpha = disabled_alpha,
                                        Child = MaxParticipantsField = new SettingsNumberTextBox
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            TabbableContentContainer = this,
                                            OnCommit = (sender, text) => apply(),
                                        },
                                    },
                                    new Section("Duration")
                                    {
                                        Child = DurationField = new DurationDropdown
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Items = new[]
                                            {
                                                TimeSpan.FromMinutes(1),
                                                TimeSpan.FromMinutes(30),
                                                TimeSpan.FromHours(1),
                                                TimeSpan.FromHours(2),
                                                TimeSpan.FromHours(4),
                                                TimeSpan.FromHours(8),
                                                TimeSpan.FromHours(12),
                                                TimeSpan.FromHours(16),
                                                TimeSpan.FromHours(24),
                                                TimeSpan.FromDays(3),
                                                TimeSpan.FromDays(7)
                                            }
                                        }
                                    },
                                    new Section("Password (optional)")
                                    {
                                        Alpha = disabled_alpha,
                                        Child = PasswordField = new SettingsPasswordTextBox
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            TabbableContentContainer = this,
                                            OnCommit = (sender, text) => apply()
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Container
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Y = 2,
                        RelativeSizeAxes = Axes.X,
                        Height = 60,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex(@"28242d").Darken(0.5f).Opacity(1f),
                            },
                            ApplyButton = new CreateRoomButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(230, 35),
                                Action = apply,
                            },
                        }
                    }
                },
            };

            TypePicker.Current.ValueChanged += t => typeLabel.Text = t.Name;

            bindings.Name.BindValueChanged(n => NameField.Text = n, true);
            bindings.Availability.BindValueChanged(a => AvailabilityPicker.Current.Value = a, true);
            bindings.Type.BindValueChanged(t => TypePicker.Current.Value = t, true);
            bindings.MaxParticipants.BindValueChanged(m => MaxParticipantsField.Text = m?.ToString(), true);
            bindings.Duration.BindValueChanged(d => DurationField.Current.Value = d, true);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            typeLabel.Colour = colours.Yellow;

            MaxParticipantsField.ReadOnly = true;
            PasswordField.ReadOnly = true;
            AvailabilityPicker.Enabled.Value = false;
            TypePicker.Enabled.Value = false;
            ApplyButton.Enabled.Value = false;
        }

        protected override void Update()
        {
            base.Update();

            ApplyButton.Enabled.Value = hasValidSettings;
        }

        private bool hasValidSettings => NameField.Text.Length > 0 && bindings.Playlist.Count > 0;

        protected override void PopIn()
        {
            content.MoveToY(0, transition_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            content.MoveToY(-1, transition_duration, Easing.InSine);
        }

        private void apply()
        {
            bindings.Name.Value = NameField.Text;
            bindings.Availability.Value = AvailabilityPicker.Current.Value;
            bindings.Type.Value = TypePicker.Current.Value;

            if (int.TryParse(MaxParticipantsField.Text, out int max))
                bindings.MaxParticipants.Value = max;
            else
                bindings.MaxParticipants.Value = null;

            bindings.Duration.Value = DurationField.Current.Value;

            manager?.CreateRoom(room);
        }

        private class SettingsTextBox : OsuTextBox
        {
            protected override Color4 BackgroundUnfocused => Color4.Black;
            protected override Color4 BackgroundFocused => Color4.Black;
        }

        private class SettingsNumberTextBox : SettingsTextBox
        {
            protected override bool CanAddCharacter(char character) => char.IsNumber(character);
        }

        private class SettingsPasswordTextBox : OsuPasswordTextBox
        {
            protected override Color4 BackgroundUnfocused => Color4.Black;
            protected override Color4 BackgroundFocused => Color4.Black;
        }

        private class SectionContainer : FillFlowContainer<Section>
        {
            public SectionContainer()
            {
                RelativeSizeAxes = Axes.Both;
                Width = 0.5f;
                Direction = FillDirection.Vertical;
                Spacing = new Vector2(field_padding);
            }
        }

        private class Section : Container
        {
            private readonly Container content;

            protected override Container<Drawable> Content => content;

            public Section(string title)
            {
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            TextSize = 12,
                            Font = @"Exo2.0-Bold",
                            Text = title.ToUpper(),
                        },
                        content = new Container
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                };
            }
        }

        private class CreateRoomButton : TriangleButton
        {
            public CreateRoomButton()
            {
                Text = "Create";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = colours.Yellow;
                Triangles.ColourLight = colours.YellowLight;
                Triangles.ColourDark = colours.YellowDark;
            }
        }

        private class DurationDropdown : OsuDropdown<TimeSpan>
        {
            public DurationDropdown()
            {
                Menu.MaxHeight = 100;
            }

            protected override string GenerateItemText(TimeSpan item)
            {
                return item.Humanize();
            }
        }
    }
}
