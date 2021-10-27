// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ExceptionExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerMatchSettingsOverlay : RoomSettingsOverlay
    {
        private MatchSettings settings;

        protected override OsuButton SubmitButton => settings.ApplyButton;

        [Resolved]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        protected override bool IsLoading => ongoingOperationTracker.InProgress.Value;

        public MultiplayerMatchSettingsOverlay(Room room)
            : base(room)
        {
        }

        protected override void SelectBeatmap() => settings.SelectBeatmap();

        protected override OnlinePlayComposite CreateSettings(Room room) => settings = new MatchSettings(room)
        {
            RelativeSizeAxes = Axes.Both,
            RelativePositionAxes = Axes.Y,
            SettingsApplied = Hide
        };

        protected class MatchSettings : OnlinePlayComposite
        {
            private const float disabled_alpha = 0.2f;

            public Action SettingsApplied;

            public OsuTextBox NameField, MaxParticipantsField;
            public RoomAvailabilityPicker AvailabilityPicker;
            public MatchTypePicker TypePicker;
            public OsuTextBox PasswordTextBox;
            public TriangleButton ApplyButton;

            public OsuSpriteText ErrorText;

            private OsuSpriteText typeLabel;
            private LoadingLayer loadingLayer;
            private BeatmapSelectionControl initialBeatmapControl;

            public void SelectBeatmap() => initialBeatmapControl.BeginSelection();

            [Resolved]
            private IRoomManager manager { get; set; }

            [Resolved]
            private MultiplayerClient client { get; set; }

            [Resolved]
            private Bindable<WorkingBeatmap> beatmap { get; set; }

            [Resolved]
            private Bindable<RulesetInfo> ruleset { get; set; }

            [Resolved]
            private OngoingOperationTracker ongoingOperationTracker { get; set; }

            private readonly IBindable<bool> operationInProgress = new BindableBool();

            [CanBeNull]
            private IDisposable applyingSettingsOperation;

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
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, 10),
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
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
                                                                        LengthLimit = 100,
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
                                                                    Child = new FillFlowContainer
                                                                    {
                                                                        AutoSizeAxes = Axes.Y,
                                                                        RelativeSizeAxes = Axes.X,
                                                                        Direction = FillDirection.Vertical,
                                                                        Spacing = new Vector2(7),
                                                                        Children = new Drawable[]
                                                                        {
                                                                            TypePicker = new MatchTypePicker
                                                                            {
                                                                                RelativeSizeAxes = Axes.X,
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
                                                            Padding = new MarginPadding { Left = FIELD_PADDING / 2 },
                                                            Children = new[]
                                                            {
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
                                                                    Child = PasswordTextBox = new OsuPasswordTextBox
                                                                    {
                                                                        RelativeSizeAxes = Axes.X,
                                                                        TabbableContentContainer = this,
                                                                        LengthLimit = 255,
                                                                    },
                                                                },
                                                            }
                                                        }
                                                    },
                                                },
                                                initialBeatmapControl = new BeatmapSelectionControl
                                                {
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                    RelativeSizeAxes = Axes.X,
                                                    Width = 0.5f
                                                }
                                            }
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
                                                ApplyButton = new CreateOrUpdateButton
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

                TypePicker.Current.BindValueChanged(type => typeLabel.Text = type.NewValue.GetLocalisableDescription(), true);
                RoomName.BindValueChanged(name => NameField.Text = name.NewValue, true);
                Availability.BindValueChanged(availability => AvailabilityPicker.Current.Value = availability.NewValue, true);
                Type.BindValueChanged(type => TypePicker.Current.Value = type.NewValue, true);
                MaxParticipants.BindValueChanged(count => MaxParticipantsField.Text = count.NewValue?.ToString(), true);
                RoomID.BindValueChanged(roomId => initialBeatmapControl.Alpha = roomId.NewValue == null ? 1 : 0, true);
                Password.BindValueChanged(password => PasswordTextBox.Text = password.NewValue ?? string.Empty, true);

                operationInProgress.BindTo(ongoingOperationTracker.InProgress);
                operationInProgress.BindValueChanged(v =>
                {
                    if (v.NewValue)
                        loadingLayer.Show();
                    else
                        loadingLayer.Hide();
                });
            }

            protected override void Update()
            {
                base.Update();

                ApplyButton.Enabled.Value = Playlist.Count > 0 && NameField.Text.Length > 0 && !operationInProgress.Value;
            }

            private void apply()
            {
                if (!ApplyButton.Enabled.Value)
                    return;

                hideError();

                Debug.Assert(applyingSettingsOperation == null);
                applyingSettingsOperation = ongoingOperationTracker.BeginOperation();

                // If the client is already in a room, update via the client.
                // Otherwise, update the room directly in preparation for it to be submitted to the API on match creation.
                if (client.Room != null)
                {
                    client.ChangeSettings(name: NameField.Text, password: PasswordTextBox.Text, matchType: TypePicker.Current.Value).ContinueWith(t => Schedule(() =>
                    {
                        if (t.IsCompletedSuccessfully)
                            onSuccess(room);
                        else
                            onError(t.Exception?.AsSingular().Message ?? "Error changing settings.");
                    }));
                }
                else
                {
                    room.Name.Value = NameField.Text;
                    room.Availability.Value = AvailabilityPicker.Current.Value;
                    room.Type.Value = TypePicker.Current.Value;
                    room.Password.Value = PasswordTextBox.Current.Value;

                    if (int.TryParse(MaxParticipantsField.Text, out int max))
                        room.MaxParticipants.Value = max;
                    else
                        room.MaxParticipants.Value = null;

                    manager?.CreateRoom(room, onSuccess, onError);
                }
            }

            private void hideError() => ErrorText.FadeOut(50);

            private void onSuccess(Room room)
            {
                Debug.Assert(applyingSettingsOperation != null);

                SettingsApplied?.Invoke();

                applyingSettingsOperation.Dispose();
                applyingSettingsOperation = null;
            }

            private void onError(string text)
            {
                Debug.Assert(applyingSettingsOperation != null);

                ErrorText.Text = text;
                ErrorText.FadeIn(50);

                applyingSettingsOperation.Dispose();
                applyingSettingsOperation = null;
            }
        }

        public class CreateOrUpdateButton : TriangleButton
        {
            [Resolved(typeof(Room), nameof(Room.RoomID))]
            private Bindable<long?> roomId { get; set; }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                roomId.BindValueChanged(id => Text = id.NewValue == null ? "Create" : "Update", true);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = colours.Yellow;
                Triangles.ColourLight = colours.YellowLight;
                Triangles.ColourDark = colours.YellowDark;
            }
        }
    }
}
