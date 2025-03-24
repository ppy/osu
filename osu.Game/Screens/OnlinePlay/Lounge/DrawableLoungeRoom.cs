// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Playlists;
using osuTK;
using osuTK.Graphics;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Screens.OnlinePlay.Lounge
{
    /// <summary>
    /// A <see cref="DrawableRoom"/> with lounge-specific interactions such as selection and hover sounds.
    /// </summary>
    public partial class DrawableLoungeRoom : DrawableRoom, IFilterable, IHasPopover, IKeyBindingHandler<GlobalAction>
    {
        private const float transition_duration = 60;
        private const float selection_border_width = 4;

        public required Bindable<Room?> SelectedRoom
        {
            get => selectedRoom;
            set => selectedRoom.Current = value;
        }

        [Resolved(canBeNull: true)]
        private IOnlinePlayLounge? lounge { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly BindableWithCurrent<Room?> selectedRoom = new BindableWithCurrent<Room?>();
        private Sample? sampleSelect;
        private Sample? sampleJoin;
        private Drawable selectionBox = null!;

        public DrawableLoungeRoom(Room room)
            : base(room)
        {
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleSelect = audio.Samples.Get($@"UI/{HoverSampleSet.Default.GetDescription()}-select");
            sampleJoin = audio.Samples.Get($@"UI/{HoverSampleSet.Button.GetDescription()}-select");

            AddRangeInternal(new Drawable[]
            {
                new StatusColouredContainer(Room, transition_duration)
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = selectionBox = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Masking = true,
                        CornerRadius = CORNER_RADIUS,
                        BorderThickness = selection_border_width,
                        BorderColour = Color4.White,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                },
                new HoverSounds()
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Alpha = matchingFilter ? 1 : 0;
            selectionBox.Alpha = selectedRoom.Value == Room ? 1 : 0;

            selectedRoom.BindValueChanged(updateSelectedRoom);

            Room.PropertyChanged += onRoomPropertyChanged;
            updateSelectedItem();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.CurrentPlaylistItem))
                updateSelectedItem();
        }

        private void updateSelectedItem()
            => SelectedItem.Value = Room.CurrentPlaylistItem;

        private void updateSelectedRoom(ValueChangedEvent<Room?> selected)
        {
            if (selected.NewValue == Room)
                selectionBox.FadeIn(transition_duration);
            else
                selectionBox.FadeOut(transition_duration);
        }

        public bool FilteringActive { get; set; }

        public IEnumerable<LocalisableString> FilterTerms => new LocalisableString[] { Room.Name };

        private bool matchingFilter = true;

        public bool MatchingFilter
        {
            get => matchingFilter;
            set
            {
                matchingFilter = value;

                if (!IsLoaded)
                    return;

                if (matchingFilter)
                    this.FadeIn(200);
                else
                    Hide();
            }
        }

        public Popover GetPopover() => new PasswordEntryPopover(Room);

        public override MenuItem[] ContextMenuItems
        {
            get
            {
                var items = new List<MenuItem>();

                items.AddRange(base.ContextMenuItems);

                items.Add(new OsuMenuItemSpacer());
                items.Add(new OsuMenuItem("Create copy", MenuItemType.Standard, () => lounge?.OpenCopy(Room)));

                if (Room.Type == MatchType.Playlists && Room.Host?.Id == api.LocalUser.Value.Id && Room.StartDate?.AddMinutes(5) >= DateTimeOffset.Now && !Room.HasEnded)
                {
                    items.Add(new OsuMenuItem("Close playlist", MenuItemType.Destructive, () =>
                    {
                        dialogOverlay?.Push(new ClosePlaylistDialog(Room, () => lounge?.Close(Room)));
                    }));
                }

                return items.ToArray();
            }
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            if (selectedRoom.Value != Room)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Select:
                    TriggerClick();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        protected override bool ShouldBeConsideredForInput(Drawable child) => selectedRoom.Value == Room || child is HoverSounds;

        protected override bool OnClick(ClickEvent e)
        {
            if (Room != selectedRoom.Value)
            {
                sampleSelect?.Play();
                selectedRoom.Value = Room;
                return true;
            }

            if (Room.HasPassword)
            {
                this.ShowPopover();
                return true;
            }

            sampleJoin?.Play();
            lounge?.Join(Room, null);
            return true;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Room.PropertyChanged -= onRoomPropertyChanged;
        }

        public partial class PasswordEntryPopover : OsuPopover
        {
            private readonly Room room;

            [Resolved(canBeNull: true)]
            private IOnlinePlayLounge? lounge { get; set; }

            public override bool HandleNonPositionalInput => true;

            protected override bool BlockNonPositionalInput => true;

            public PasswordEntryPopover(Room room)
            {
                this.room = room;
            }

            private OsuPasswordTextBox passwordTextBox = null!;
            private RoundedButton joinButton = null!;
            private OsuSpriteText errorText = null!;
            private Sample? sampleJoinFail;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, AudioManager audio)
            {
                Child = new FillFlowContainer
                {
                    Margin = new MarginPadding(10),
                    Spacing = new Vector2(5),
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    LayoutDuration = 500,
                    LayoutEasing = Easing.OutQuint,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                passwordTextBox = new OsuPasswordTextBox
                                {
                                    Width = 200,
                                    PlaceholderText = "password",
                                },
                                joinButton = new RoundedButton
                                {
                                    Width = 80,
                                    Text = "Join Room",
                                }
                            }
                        },
                        errorText = new OsuSpriteText
                        {
                            Colour = colours.Red,
                        },
                    }
                };

                sampleJoinFail = audio.Samples.Get(@"UI/generic-error");

                joinButton.Action = performJoin;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ScheduleAfterChildren(() => GetContainingFocusManager()!.ChangeFocus(passwordTextBox));
                passwordTextBox.OnCommit += (_, _) => performJoin();
            }

            private void performJoin()
            {
                lounge?.Join(room, passwordTextBox.Text, null, joinFailed);
                GetContainingFocusManager()?.TriggerFocusContention(passwordTextBox);
            }

            private void joinFailed(string error) => Schedule(() =>
            {
                passwordTextBox.Text = string.Empty;

                GetContainingFocusManager()!.ChangeFocus(passwordTextBox);

                errorText.Text = error;
                errorText
                    .FadeIn()
                    .FlashColour(Color4.White, 200)
                    .Delay(1000)
                    .FadeOutFromOne(1000, Easing.In);

                Body.Shake();

                sampleJoinFail?.Play();
            });
        }
    }
}
