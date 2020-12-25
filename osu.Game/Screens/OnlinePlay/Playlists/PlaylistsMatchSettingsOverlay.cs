// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
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
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class PlaylistsMatchSettingsOverlay : MatchSettingsOverlay
    {
        public Action EditPlaylist;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = Settings = new MatchSettings
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Y,
                EditPlaylist = () => EditPlaylist?.Invoke()
            };
        }

        protected class MatchSettings : OnlinePlayComposite
        {
            private const float disabled_alpha = 0.2f;

            public Action EditPlaylist;

            public OsuTextBox NameField, MaxParticipantsField;
            public OsuDropdown<TimeSpan> DurationField;
            public RoomAvailabilityPicker AvailabilityPicker;
            public GameTypePicker TypePicker;
            public TriangleButton ApplyButton;

            public OsuSpriteText ErrorText;

            private OsuSpriteText typeLabel;
            private LoadingLayer loadingLayer;
            private DrawableRoomPlaylist playlist;
            private OsuSpriteText playlistLength;

            [Resolved(CanBeNull = true)]
            private IRoomManager manager { get; set; }

            [Resolved]
            private Bindable<Room> currentRoom { get; set; }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Container dimContent;

                InternalChildren = new Drawable[]
                {
                    dimContent = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex(@"28242d"),
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
                                                    Padding = new MarginPadding { Horizontal = WaveOverlayContainer.WIDTH_PADDING },
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Children = new Drawable[]
                                                    {
                                                        new SectionContainer
                                                        {
                                                            Padding = new MarginPadding { Right = FIELD_PADDING / 2 },
                                                            Children = new[]
                                                            {
                                                                new Section("Room name")
                                                                {
                                                                    Child = NameField = new SettingsTextBox
                                                                    {
                                                                        RelativeSizeAxes = Axes.X,
                                                                        TabbableContentContainer = this,
                                                                        LengthLimit = 100
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
                                                                new Section("Max participants")
                                                                {
                                                                    Alpha = disabled_alpha,
                                                                    Child = MaxParticipantsField = new SettingsNumberTextBox
                                                                    {
                                                                        RelativeSizeAxes = Axes.X,
                                                                        TabbableContentContainer = this,
                                                                        ReadOnly = true,
                                                                    },
                                                                },
                                                                new Section("Password (optional)")
                                                                {
                                                                    Alpha = disabled_alpha,
                                                                    Child = new SettingsPasswordTextBox
                                                                    {
                                                                        RelativeSizeAxes = Axes.X,
                                                                        TabbableContentContainer = this,
                                                                        ReadOnly = true,
                                                                    },
                                                                },
                                                            },
                                                        },
                                                        new SectionContainer
                                                        {
                                                            Anchor = Anchor.TopRight,
                                                            Origin = Anchor.TopRight,
                                                            Padding = new MarginPadding { Left = FIELD_PADDING / 2 },
                                                            Children = new[]
                                                            {
                                                                new Section("Playlist")
                                                                {
                                                                    Child = new GridContainer
                                                                    {
                                                                        RelativeSizeAxes = Axes.X,
                                                                        Height = 300,
                                                                        Content = new[]
                                                                        {
                                                                            new Drawable[]
                                                                            {
                                                                                playlist = new DrawableRoomPlaylist(true, true) { RelativeSizeAxes = Axes.Both }
                                                                            },
                                                                            new Drawable[]
                                                                            {
                                                                                playlistLength = new OsuSpriteText
                                                                                {
                                                                                    Margin = new MarginPadding { Vertical = 5 },
                                                                                    Colour = colours.Yellow,
                                                                                    Font = OsuFont.GetFont(size: 12),
                                                                                }
                                                                            },
                                                                            new Drawable[]
                                                                            {
                                                                                new PurpleTriangleButton
                                                                                {
                                                                                    RelativeSizeAxes = Axes.X,
                                                                                    Height = 40,
                                                                                    Text = "Edit playlist",
                                                                                    Action = () => EditPlaylist?.Invoke()
                                                                                }
                                                                            }
                                                                        },
                                                                        RowDimensions = new[]
                                                                        {
                                                                            new Dimension(),
                                                                            new Dimension(GridSizeMode.AutoSize),
                                                                            new Dimension(GridSizeMode.AutoSize),
                                                                        }
                                                                    }
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
                                                    Colour = Color4Extensions.FromHex(@"28242d").Darken(0.5f).Opacity(1f),
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
                        }
                    },
                    loadingLayer = new LoadingLayer(dimContent)
                };

                TypePicker.Current.BindValueChanged(type => typeLabel.Text = type.NewValue?.Name ?? string.Empty, true);
                RoomName.BindValueChanged(name => NameField.Text = name.NewValue, true);
                Availability.BindValueChanged(availability => AvailabilityPicker.Current.Value = availability.NewValue, true);
                Type.BindValueChanged(type => TypePicker.Current.Value = type.NewValue, true);
                MaxParticipants.BindValueChanged(count => MaxParticipantsField.Text = count.NewValue?.ToString(), true);
                Duration.BindValueChanged(duration => DurationField.Current.Value = duration.NewValue ?? TimeSpan.FromMinutes(30), true);

                playlist.Items.BindTo(Playlist);
                Playlist.BindCollectionChanged(onPlaylistChanged, true);
            }

            protected override void Update()
            {
                base.Update();

                ApplyButton.Enabled.Value = hasValidSettings;
            }

            private void onPlaylistChanged(object sender, NotifyCollectionChangedEventArgs e) =>
                playlistLength.Text = $"Length: {Playlist.GetTotalDuration()}";

            private bool hasValidSettings => RoomID.Value == null && NameField.Text.Length > 0 && Playlist.Count > 0;

            private void apply()
            {
                if (!ApplyButton.Enabled.Value)
                    return;

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

                loadingLayer.Show();
            }

            private void hideError() => ErrorText.FadeOut(50);

            private void onSuccess(Room room) => loadingLayer.Hide();

            private void onError(string text)
            {
                ErrorText.Text = text;
                ErrorText.FadeIn(50);

                loadingLayer.Hide();
            }
        }

        public class CreateRoomButton : TriangleButton
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

            protected override string GenerateItemText(TimeSpan item) => item.Humanize();
        }
    }
}
