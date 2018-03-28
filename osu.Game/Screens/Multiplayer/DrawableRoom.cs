// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Screens.Multiplayer
{
    public class DrawableRoom : OsuClickableContainer, IStateful<Visibility>, IFilterable
    {
        public const float SELECTION_BORDER_WIDTH = 4;

        private const float transition_duration = 100;
        private const float content_padding = 10;
        private const float height = 100;
        private const float side_strip_width = 5;
        private const float cover_width = 145;

        private readonly Box selectionBox, sideStrip;
        private readonly Container coverContainer;
        private readonly OsuSpriteText name, status, beatmapTitle, beatmapDash, beatmapArtist;
        private readonly FillFlowContainer<OsuSpriteText> beatmapInfoFlow;
        private readonly ParticipantInfo participantInfo;
        private readonly ModeTypeInfo modeTypeInfo;

        private readonly Bindable<string> nameBind = new Bindable<string>();
        private readonly Bindable<User> hostBind = new Bindable<User>();
        private readonly Bindable<RoomStatus> statusBind = new Bindable<RoomStatus>();
        private readonly Bindable<GameType> typeBind = new Bindable<GameType>();
        private readonly Bindable<BeatmapInfo> beatmapBind = new Bindable<BeatmapInfo>();
        private readonly Bindable<IEnumerable<User>> participantsBind = new Bindable<IEnumerable<User>>();

        private OsuColour colours;
        private LocalisationEngine localisation;

        public readonly Room Room;

        public event Action<Visibility> StateChanged;

        private Visibility state = Visibility.Hidden;
        public Visibility State
        {
            get { return state; }
            set
            {
                if (value == state) return;
                state = value;

                if (state == Visibility.Visible)
                    selectionBox.FadeIn(transition_duration);
                else
                    selectionBox.FadeOut(transition_duration);
            }
        }

        public IEnumerable<string> FilterTerms => new[] { Room.Name.Value };

        private bool matchingFilter;
        public bool MatchingFilter
        {
            get { return matchingFilter; }
            set
            {
                if (value == matchingFilter) return;
                matchingFilter = value;

                this.FadeTo(MatchingFilter ? 1 : 0, 200);
            }
        }

        private Action<DrawableRoom> action;
        public new Action<DrawableRoom> Action
        {
            get { return action; }
            set
            {
                action = value;
                Enabled.Value = action != null;
            }
        }

        public DrawableRoom(Room room)
        {
            Room = room;

            RelativeSizeAxes = Axes.X;
            Height = height + SELECTION_BORDER_WIDTH * 2;
            Masking = true;
            CornerRadius = 5 + SELECTION_BORDER_WIDTH / 2;

            Children = new Drawable[]
            {
                selectionBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0f,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(SELECTION_BORDER_WIDTH),
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 5,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = Color4.Black.Opacity(40),
                            Radius = 5,
                        },
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex(@"212121"),
                            },
                            sideStrip = new Box
                            {
                                RelativeSizeAxes = Axes.Y,
                                Width = side_strip_width,
                            },
                            new Container
                            {
                                Width = cover_width,
                                RelativeSizeAxes = Axes.Y,
                                Masking = true,
                                Margin = new MarginPadding { Left = side_strip_width },
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Black,
                                    },
                                    coverContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                },
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Vertical = content_padding,
                                    Left = side_strip_width + cover_width + content_padding,
                                    Right = content_padding,
                                },
                                Children = new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5f),
                                        Children = new Drawable[]
                                        {
                                            name = new OsuSpriteText
                                            {
                                                TextSize = 18,
                                            },
                                            participantInfo = new ParticipantInfo(),
                                        },
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            status = new OsuSpriteText
                                            {
                                                TextSize = 14,
                                                Font = @"Exo2.0-Bold",
                                            },
                                            beatmapInfoFlow = new FillFlowContainer<OsuSpriteText>
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Direction = FillDirection.Horizontal,
                                                Children = new[]
                                                {
                                                    beatmapTitle = new OsuSpriteText
                                                    {
                                                        TextSize = 14,
                                                        Font = @"Exo2.0-BoldItalic",
                                                    },
                                                    beatmapDash = new OsuSpriteText
                                                    {
                                                        TextSize = 14,
                                                        Font = @"Exo2.0-BoldItalic",
                                                    },
                                                    beatmapArtist = new OsuSpriteText
                                                    {
                                                        TextSize = 14,
                                                        Font = @"Exo2.0-RegularItalic",
                                                    },
                                                },
                                            },
                                        },
                                    },
                                    modeTypeInfo = new ModeTypeInfo
                                    {
                                        Anchor = Anchor.BottomRight,
                                        Origin = Anchor.BottomRight,
                                    },
                                },
                            },
                        },
                    },
                },
            };

            nameBind.ValueChanged += displayName;
            hostBind.ValueChanged += displayUser;
            typeBind.ValueChanged += displayGameType;
            participantsBind.ValueChanged += displayParticipants;

            nameBind.BindTo(Room.Name);
            hostBind.BindTo(Room.Host);
            statusBind.BindTo(Room.Status);
            typeBind.BindTo(Room.Type);
            beatmapBind.BindTo(Room.Beatmap);
            participantsBind.BindTo(Room.Participants);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, LocalisationEngine localisation)
        {
            this.localisation = localisation;
            this.colours = colours;

            beatmapInfoFlow.Colour = colours.Gray9;

            //binded here instead of ctor because dependencies are needed
            statusBind.ValueChanged += displayStatus;
            beatmapBind.ValueChanged += displayBeatmap;

            statusBind.TriggerChange();
            beatmapBind.TriggerChange();
        }

        protected override bool OnClick(InputState state)
        {
            if (Enabled.Value)
                Action?.Invoke(this);
            return true;
        }

        private void displayName(string value)
        {
            name.Text = value;
        }

        private void displayUser(User value)
        {
            participantInfo.Host = value;
        }

        private void displayStatus(RoomStatus value)
        {
            if (value == null) return;
            status.Text = value.Message;

            foreach (Drawable d in new Drawable[] { selectionBox, sideStrip, status })
                d.FadeColour(value.GetAppropriateColour(colours), 100);
        }

        private void displayGameType(GameType value)
        {
            modeTypeInfo.Type = value;
        }

        private void displayBeatmap(BeatmapInfo value)
        {
            modeTypeInfo.Beatmap = value;

            if (value != null)
            {
                coverContainer.FadeIn(transition_duration);

                LoadComponentAsync(new BeatmapSetCover(value.BeatmapSet)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fill,
                    OnLoadComplete = d => d.FadeInFromZero(400, Easing.Out),
                },
                coverContainer.Add);

                beatmapTitle.Current = localisation.GetUnicodePreference(value.Metadata.TitleUnicode, value.Metadata.Title);
                beatmapDash.Text = @" - ";
                beatmapArtist.Current = localisation.GetUnicodePreference(value.Metadata.ArtistUnicode, value.Metadata.Artist);
            }
            else
            {
                coverContainer.FadeOut(transition_duration);

                beatmapTitle.Current = null;
                beatmapArtist.Current = null;

                beatmapTitle.Text = "Changing map";
                beatmapDash.Text = beatmapArtist.Text = string.Empty;
            }
        }

        private void displayParticipants(IEnumerable<User> value)
        {
            participantInfo.Participants = value;
        }
    }
}
