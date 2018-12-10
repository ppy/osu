// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Screens.Match
{
    public class RoomSettingsOverlay : FocusedOverlayContainer
    {
        private const float transition_duration = 350;
        private const float field_padding = 45;

        /// <summary>
        /// Invoked when room settings were applied.
        /// </summary>
        public Action Applied;

        private readonly Bindable<string> nameBind = new Bindable<string>();
        private readonly Bindable<RoomAvailability> availabilityBind = new Bindable<RoomAvailability>();
        private readonly Bindable<GameType> typeBind = new Bindable<GameType>();
        private readonly Bindable<int?> maxParticipantsBind = new Bindable<int?>();

        private readonly Container content;

        private readonly OsuSpriteText typeLabel;

        protected readonly OsuTextBox NameField, MaxParticipantsField;
        protected readonly RoomAvailabilityPicker AvailabilityPicker;
        protected readonly GameTypePicker TypePicker;
        protected readonly TriangleButton ApplyButton;
        protected readonly OsuPasswordTextBox PasswordField;

        public RoomSettingsOverlay()
        {
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
                                    new Section("ROOM NAME")
                                    {
                                        Child = NameField = new SettingsTextBox
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            TabbableContentContainer = this,
                                            OnCommit = (sender, text) => apply(),
                                        },
                                    },
                                    new Section("ROOM VISIBILITY")
                                    {
                                        Child = AvailabilityPicker = new RoomAvailabilityPicker(),
                                    },
                                    new Section("GAME TYPE")
                                    {
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
                                    new Section("MAX PARTICIPANTS")
                                    {
                                        Child = MaxParticipantsField = new SettingsNumberTextBox
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            TabbableContentContainer = this,
                                            OnCommit = (sender, text) => apply(),
                                        },
                                    },
                                    new Section("PASSWORD (OPTIONAL)")
                                    {
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
                    ApplyButton = new ApplySettingsButton
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(230, 35),
                        Margin = new MarginPadding { Bottom = 20 },
                        Action = apply,
                    },
                },
            };

            TypePicker.Current.ValueChanged += t => typeLabel.Text = t.Name;

            nameBind.ValueChanged += n => NameField.Text = n;
            availabilityBind.ValueChanged += a => AvailabilityPicker.Current.Value = a;
            typeBind.ValueChanged += t => TypePicker.Current.Value = t;
            maxParticipantsBind.ValueChanged += m => MaxParticipantsField.Text = m?.ToString();

            Room = new Room();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            typeLabel.Colour = colours.Yellow;
        }

        private bool readOnly;

        public bool ReadOnly
        {
            get => readOnly;
            set
            {
                if (readOnly == value)
                    return;
                readOnly = value;

                NameField.ReadOnly = value;
                MaxParticipantsField.ReadOnly = value;
                PasswordField.ReadOnly = value;
                AvailabilityPicker.ReadOnly.Value = value;
                TypePicker.ReadOnly.Value = value;
                ApplyButton.Enabled.Value = !value;
            }
        }


        private Room room;

        /// <summary>
        /// The room which settings are being applied to.
        /// </summary>
        public virtual Room Room
        {
            get => room;
            set
            {
                if (room == value)
                    return;

                if (room != null)
                {
                    nameBind.UnbindFrom(room.Name);
                    availabilityBind.UnbindFrom(room.Availability);
                    typeBind.UnbindFrom(room.Type);
                    maxParticipantsBind.UnbindFrom(room.MaxParticipants);
                }

                room = value;

                if (room != null)
                {
                    nameBind.BindTo(room.Name);
                    availabilityBind.BindTo(room.Availability);
                    typeBind.BindTo(room.Type);
                    maxParticipantsBind.BindTo(room.MaxParticipants);
                }
            }
        }

        protected override void PopIn()
        {
            // reapply the rooms values if the overlay was completely closed
            if (content.Y == -1)
            {
                nameBind.TriggerChange();
                availabilityBind.TriggerChange();
                typeBind.TriggerChange();
                maxParticipantsBind.TriggerChange();
            }

            content.MoveToY(0, transition_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            content.MoveToY(-1, transition_duration, Easing.InSine);
        }

        private void apply()
        {
            nameBind.Value = NameField.Text;
            availabilityBind.Value = AvailabilityPicker.Current.Value;
            typeBind.Value = TypePicker.Current.Value;

            if (int.TryParse(MaxParticipantsField.Text, out int max))
                maxParticipantsBind.Value = max;
            else
                maxParticipantsBind.Value = null;

            Applied?.Invoke();
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

        private class ApplySettingsButton : TriangleButton
        {
            public ApplySettingsButton()
            {
                Text = "Apply";
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
