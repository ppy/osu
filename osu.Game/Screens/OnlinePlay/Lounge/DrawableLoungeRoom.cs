// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
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
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge
{
    /// <summary>
    /// A <see cref="DrawableRoom"/> with lounge-specific interactions such as selection and hover sounds.
    /// </summary>
    public class DrawableLoungeRoom : DrawableRoom, IFilterable, IHasContextMenu, IHasPopover, IKeyBindingHandler<GlobalAction>
    {
        private const float transition_duration = 60;
        private const float selection_border_width = 4;

        public readonly Bindable<Room> SelectedRoom = new Bindable<Room>();

        [Resolved(canBeNull: true)]
        private LoungeSubScreen lounge { get; set; }

        private Sample sampleSelect;
        private Sample sampleJoin;
        private Drawable selectionBox;

        public DrawableLoungeRoom(Room room)
            : base(room)
        {
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleSelect = audio.Samples.Get($@"UI/{HoverSampleSet.Default.GetDescription()}-select");
            sampleJoin = audio.Samples.Get($@"UI/{HoverSampleSet.Submit.GetDescription()}-select");

            AddRangeInternal(new Drawable[]
            {
                new StatusColouredContainer(transition_duration)
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
            selectionBox.Alpha = SelectedRoom.Value == Room ? 1 : 0;

            SelectedRoom.BindValueChanged(updateSelectedRoom);
        }

        private void updateSelectedRoom(ValueChangedEvent<Room> selected)
        {
            if (selected.NewValue == Room)
                selectionBox.FadeIn(transition_duration);
            else
                selectionBox.FadeOut(transition_duration);
        }

        public bool FilteringActive { get; set; }

        public IEnumerable<string> FilterTerms => new[] { Room.Name.Value };

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

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("Create copy", MenuItemType.Standard, () =>
            {
                lounge?.Open(Room.DeepClone());
            })
        };

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            if (SelectedRoom.Value != Room)
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

        protected override bool ShouldBeConsideredForInput(Drawable child) => SelectedRoom.Value == Room || child is HoverSounds;

        protected override bool OnClick(ClickEvent e)
        {
            if (Room != SelectedRoom.Value)
            {
                sampleSelect?.Play();
                SelectedRoom.Value = Room;
                return true;
            }

            if (Room.HasPassword.Value)
            {
                sampleJoin?.Play();
                this.ShowPopover();
                return true;
            }

            sampleJoin?.Play();
            lounge?.Join(Room, null);
            return true;
        }

        public class PasswordEntryPopover : OsuPopover
        {
            private readonly Room room;

            [Resolved(canBeNull: true)]
            private LoungeSubScreen lounge { get; set; }

            public override bool HandleNonPositionalInput => true;

            protected override bool BlockNonPositionalInput => true;

            public PasswordEntryPopover(Room room)
            {
                this.room = room;
            }

            private OsuPasswordTextBox passwordTextBox;
            private TriangleButton joinButton;
            private OsuSpriteText errorText;
            private Sample sampleJoinFail;

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
                                joinButton = new TriangleButton
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

                sampleJoinFail = audio.Samples.Get(@"UI/password-fail");

                joinButton.Action = performJoin;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Schedule(() => GetContainingInputManager().ChangeFocus(passwordTextBox));
                passwordTextBox.OnCommit += (_, __) => performJoin();
            }

            private void performJoin()
            {
                lounge?.Join(room, passwordTextBox.Text, null, joinFailed);
                GetContainingInputManager().TriggerFocusContention(passwordTextBox);
            }

            private void joinFailed(string error) => Schedule(() =>
            {
                passwordTextBox.Text = string.Empty;

                GetContainingInputManager().ChangeFocus(passwordTextBox);

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
