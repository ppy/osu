// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Screens.Match
{
    public class RoomSettingsOverlay : OverlayContainer
    {
        private const float transition_duration = 500;
        private const float field_padding = 45;

        private readonly Bindable<string> nameBind = new Bindable<string>();
        private readonly Bindable<RoomAvailability> availabilityBind = new Bindable<RoomAvailability>();
        private readonly Bindable<GameType> typeBind = new Bindable<GameType>();
        private readonly Bindable<int?> maxParticipantsBind = new Bindable<int?>();

        private readonly Container content;

        public RoomSettingsOverlay(Room room)
        {
            Masking = true;

            SettingsTextBox name, maxParticipants;
            RoomAvailabilityPicker availability;
            GameTypePicker type;
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
                                        Child = name = new SettingsTextBox(),
                                    },
                                    new Section("ROOM VISIBILITY")
                                    {
                                        Child = availability = new RoomAvailabilityPicker(),
                                    },
                                    new Section("GAME TYPE")
                                    {
                                        Child = type = new GameTypePicker(),
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
                                        Child = maxParticipants = new SettingsTextBox(),
                                    },
                                    new Section("PASSWORD (OPTIONAL)")
                                    {
                                        Child = new SettingsTextBox(),
                                    },
                                },
                            },
                        },
                    },
                    new ApplyButton
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(230, 35),
                        Margin = new MarginPadding { Bottom = 20 },
                        Action = () =>
                        {
                            if (room != null)
                            {
                                room.Name.Value = name.Text;
                                room.Availability.Value = availability.Current.Value;
                                room.Type.Value = type.Current.Value;

                                int max;
                                if (int.TryParse(maxParticipants.Text, out max))
                                    room.MaxParticipants.Value = max;
                                else
                                    room.MaxParticipants.Value = null;
                            }

                            Hide();
                        },
                    },
                },
            };

            nameBind.ValueChanged += n => name.Text = n;
            availabilityBind.ValueChanged += a => availability.Current.Value = a;
            typeBind.ValueChanged += t => type.Current.Value = t;
            maxParticipantsBind.ValueChanged += m => maxParticipants.Text = m?.ToString();

            nameBind.BindTo(room.Name);
            availabilityBind.BindTo(room.Availability);
            typeBind.BindTo(room.Type);
            maxParticipantsBind.BindTo(room.MaxParticipants);
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

        private class SettingsTextBox : OsuTextBox
        {
            protected override Color4 BackgroundUnfocused => Color4.Black;
            protected override Color4 BackgroundFocused => Color4.Black;

            protected override Drawable GetDrawableCharacter(char c) => new OsuSpriteText
            {
                Text = c.ToString(),
                TextSize = 18,
            };

            public SettingsTextBox()
            {
                RelativeSizeAxes = Axes.X;
            }
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

        private class ApplyButton : TriangleButton
        {
            public ApplyButton()
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
