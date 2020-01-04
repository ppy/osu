// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class MatchSettingsOverlay : FocusedOverlayContainer
    {
        private const float transition_duration = 350;
        private const float field_padding = 45;

        protected MatchSettings Settings { get; private set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;

            Child = Settings = new MatchSettings
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Y
            };
        }

        protected override void PopIn()
        {
            Settings.MoveToY(0, transition_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            Settings.MoveToY(-1, transition_duration, Easing.InSine);
        }

        protected class MatchSettings : MultiplayerComposite
        {
            private const float disabled_alpha = 0.2f;

            public OsuTextBox NameField, MaxParticipantsField;
            public OsuDropdown<TimeSpan> DurationField;
            public RoomAvailabilityPicker AvailabilityPicker;
            public GameTypePicker TypePicker;
            public TriangleButton ApplyButton;

            public OsuSpriteText ErrorText;

            private OsuSpriteText typeLabel;
            private ProcessingOverlay processingOverlay;

            [Resolved(CanBeNull = true)]
            private IRoomManager manager { get; set; }

            [Resolved]
            private Bindable<Room> currentRoom { get; set; }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.FromHex(@"28242d"),
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Distributed),
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new OsuScrollContainer
                                {
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = OsuScreen.HORIZONTAL_OVERFLOW_PADDING,
                                        Vertical = 10
                                    },
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new[]
                                    {
                                        new Container
                                        {
                                            Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING },
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Children = new Drawable[]
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
                                                            Child = AvailabilityPicker = new RoomAvailabilityPicker
                                                            {
                                                                Enabled = { Value = false }
                                                            },
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
                                                                        Enabled = { Value = false }
                                                                    },
                                                                    typeLabel = new OsuSpriteText
                                                                    {
                                                                        Font = OsuFont.GetFont(size: 14),
                                                                        Colour = colours.Yellow
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
                                                                ReadOnly = true,
                                                                OnCommit = (sender, text) => apply()
                                                            },
                                                        },
                                                        new Section("Duration")
                                                        {
                                                            Child = DurationField = new DurationDropdown
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                Items = new[]
                                                                {
                                                                    TimeSpan.FromMinutes(30),
                                                                    TimeSpan.FromHours(1),
                                                                    TimeSpan.FromHours(2),
                                                                    TimeSpan.FromHours(4),
                                                                    TimeSpan.FromHours(8),
                                                                    TimeSpan.FromHours(12),
                                                                    //TimeSpan.FromHours(16),
                                                                    TimeSpan.FromHours(24),
                                                                    TimeSpan.FromDays(3),
                                                                    TimeSpan.FromDays(7)
                                                                }
                                                            }
                                                        },
                                                        new Section("Password (optional)")
                                                        {
                                                            Alpha = disabled_alpha,
                                                            Child = new SettingsPasswordTextBox
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                TabbableContentContainer = this,
                                                                ReadOnly = true,
                                                                OnCommit = (sender, text) => apply()
                                                            },
                                                        },
                                                    },
                                                },
                                            },
                                        }
                                    },
                                },
                            },
                            new Drawable[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Y = 2,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = OsuColour.FromHex(@"28242d").Darken(0.5f).Opacity(1f),
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, 20),
                                            Margin = new MarginPadding { Vertical = 20 },
                                            Padding = new MarginPadding { Horizontal = OsuScreen.HORIZONTAL_OVERFLOW_PADDING },
                                            Children = new Drawable[]
                                            {
                                                ApplyButton = new CreateRoomButton
                                                {
                                                    Anchor = Anchor.BottomCentre,
                                                    Origin = Anchor.BottomCentre,
                                                    Size = new Vector2(230, 55),
                                                    Enabled = { Value = false },
                                                    Action = apply,
                                                },
                                                ErrorText = new OsuSpriteText
                                                {
                                                    Anchor = Anchor.BottomCentre,
                                                    Origin = Anchor.BottomCentre,
                                                    Alpha = 0,
                                                    Depth = 1,
                                                    Colour = colours.RedDark
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    processingOverlay = new ProcessingOverlay { Alpha = 0 }
                };

                TypePicker.Current.BindValueChanged(type => typeLabel.Text = type.NewValue?.Name ?? string.Empty, true);
                RoomName.BindValueChanged(name => NameField.Text = name.NewValue, true);
                Availability.BindValueChanged(availability => AvailabilityPicker.Current.Value = availability.NewValue, true);
                Type.BindValueChanged(type => TypePicker.Current.Value = type.NewValue, true);
                MaxParticipants.BindValueChanged(count => MaxParticipantsField.Text = count.NewValue?.ToString(), true);
                Duration.BindValueChanged(duration => DurationField.Current.Value = duration.NewValue, true);
            }

            protected override void Update()
            {
                base.Update();

                ApplyButton.Enabled.Value = hasValidSettings;
            }

            private bool hasValidSettings => RoomID.Value == null && NameField.Text.Length > 0 && Playlist.Count > 0;

            private void apply()
            {
                hideError();

                RoomName.Value = NameField.Text;
                Availability.Value = AvailabilityPicker.Current.Value;
                Type.Value = TypePicker.Current.Value;

                if (int.TryParse(MaxParticipantsField.Text, out int max))
                    MaxParticipants.Value = max;
                else
                    MaxParticipants.Value = null;

                Duration.Value = DurationField.Current.Value;

                manager?.CreateRoom(currentRoom.Value, onSuccess, onError);

                processingOverlay.Show();
            }

            private void hideError() => ErrorText.FadeOut(50);

            private void onSuccess(Room room) => processingOverlay.Hide();

            private void onError(string text)
            {
                ErrorText.Text = text;
                ErrorText.FadeIn(50);

                processingOverlay.Hide();
            }
        }

        private class SettingsTextBox : OsuTextBox
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                BackgroundUnfocused = Color4.Black;
                BackgroundFocused = Color4.Black;
            }
        }

        private class SettingsNumberTextBox : SettingsTextBox
        {
            protected override bool CanAddCharacter(char character) => char.IsNumber(character);
        }

        private class SettingsPasswordTextBox : OsuPasswordTextBox
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                BackgroundUnfocused = Color4.Black;
                BackgroundFocused = Color4.Black;
            }
        }

        private class SectionContainer : FillFlowContainer<Section>
        {
            public SectionContainer()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
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
                            Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 12),
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
