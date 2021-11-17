// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
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
    public class PlaylistsRoomSettingsOverlay : RoomSettingsOverlay
    {
        public Action EditPlaylist;

        private MatchSettings settings;

        protected override OsuButton SubmitButton => settings.ApplyButton;

        protected override bool IsLoading => settings.IsLoading; // should probably be replaced with an OngoingOperationTracker.

        public PlaylistsRoomSettingsOverlay(Room room)
            : base(room)
        {
        }

        protected override void SelectBeatmap() => settings.SelectBeatmap();

        protected override OnlinePlayComposite CreateSettings(Room room) => settings = new MatchSettings(room)
        {
            RelativeSizeAxes = Axes.Both,
            RelativePositionAxes = Axes.Y,
            EditPlaylist = () => EditPlaylist?.Invoke()
        };

        protected class MatchSettings : OnlinePlayComposite
        {
            private const float disabled_alpha = 0.2f;

            public Action EditPlaylist;

            public OsuTextBox NameField, MaxParticipantsField, MaxAttemptsField;
            public OsuDropdown<TimeSpan> DurationField;
            public RoomAvailabilityPicker AvailabilityPicker;
            public TriangleButton ApplyButton;

            public bool IsLoading => loadingLayer.State.Value == Visibility.Visible;

            public OsuSpriteText ErrorText;

            private LoadingLayer loadingLayer;
            private DrawableRoomPlaylist playlist;
            private OsuSpriteText playlistLength;

            private PurpleTriangleButton editPlaylistButton;

            [Resolved(CanBeNull = true)]
            private IRoomManager manager { get; set; }

            private readonly Room room;

            public MatchSettings(Room room)
            {
                this.room = room;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider, OsuColour colours)
            {
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background4
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(),
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
                                                            Child = NameField = new OsuTextBox
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
                                                        new Section("Allowed attempts (across all playlist items)")
                                                        {
                                                            Child = MaxAttemptsField = new OsuNumberBox
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                TabbableContentContainer = this,
                                                                PlaceholderText = "Unlimited",
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
                                                        new Section("Max participants")
                                                        {
                                                            Alpha = disabled_alpha,
                                                            Child = MaxParticipantsField = new OsuNumberBox
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                TabbableContentContainer = this,
                                                                ReadOnly = true,
                                                            },
                                                        },
                                                        new Section("Password (optional)")
                                                        {
                                                            Alpha = disabled_alpha,
                                                            Child = new OsuPasswordTextBox
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
                                                                Height = 448,
                                                                Content = new[]
                                                                {
                                                                    new Drawable[]
                                                                    {
                                                                        playlist = new DrawableRoomPlaylist(true, false) { RelativeSizeAxes = Axes.Both }
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
                                                                        editPlaylistButton = new PurpleTriangleButton
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
                                            Colour = colourProvider.Background5
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
                    loadingLayer = new LoadingLayer(true)
                };

                RoomName.BindValueChanged(name => NameField.Text = name.NewValue, true);
                Availability.BindValueChanged(availability => AvailabilityPicker.Current.Value = availability.NewValue, true);
                MaxParticipants.BindValueChanged(count => MaxParticipantsField.Text = count.NewValue?.ToString(), true);
                MaxAttempts.BindValueChanged(count => MaxAttemptsField.Text = count.NewValue?.ToString(), true);
                Duration.BindValueChanged(duration => DurationField.Current.Value = duration.NewValue ?? TimeSpan.FromMinutes(30), true);

                playlist.Items.BindTo(Playlist);
                Playlist.BindCollectionChanged(onPlaylistChanged, true);
            }

            protected override void Update()
            {
                base.Update();

                ApplyButton.Enabled.Value = hasValidSettings;
            }

            public void SelectBeatmap() => editPlaylistButton.TriggerClick();

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

                if (int.TryParse(MaxParticipantsField.Text, out int max))
                    MaxParticipants.Value = max;
                else
                    MaxParticipants.Value = null;

                if (int.TryParse(MaxAttemptsField.Text, out max))
                    MaxAttempts.Value = max;
                else
                    MaxAttempts.Value = null;

                Duration.Value = DurationField.Current.Value;

                loadingLayer.Show();
                manager?.CreateRoom(room, onSuccess, onError);
            }

            private void hideError() => ErrorText.FadeOut(50);

            private void onSuccess(Room room) => loadingLayer.Hide();

            private void onError(string text)
            {
                // see https://github.com/ppy/osu-web/blob/2c97aaeb64fb4ed97c747d8383a35b30f57428c7/app/Models/Multiplayer/PlaylistItem.php#L48.
                const string not_found_prefix = "beatmaps not found:";

                if (text.StartsWith(not_found_prefix, StringComparison.Ordinal))
                {
                    ErrorText.Text = "One or more beatmaps were not available online. Please remove or replace the highlighted items.";

                    int[] invalidBeatmapIDs = text
                                              .Substring(not_found_prefix.Length + 1)
                                              .Split(", ")
                                              .Select(int.Parse)
                                              .ToArray();

                    foreach (var item in Playlist)
                    {
                        if (invalidBeatmapIDs.Contains(item.BeatmapID))
                            item.MarkInvalid();
                    }
                }
                else
                {
                    ErrorText.Text = text;
                }

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

            protected override LocalisableString GenerateItemText(TimeSpan item) => item.Humanize();
        }
    }
}
