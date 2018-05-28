// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework;
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
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Components
{
    public class DrawableRoom : OsuClickableContainer, IStateful<SelectionState>, IFilterable
    {
        public const float SELECTION_BORDER_WIDTH = 4;
        private const float corner_radius = 5;
        private const float transition_duration = 60;
        private const float content_padding = 10;
        private const float height = 100;
        private const float side_strip_width = 5;
        private const float cover_width = 145;

        private readonly Box selectionBox;

        private readonly Bindable<string> nameBind = new Bindable<string>();
        private readonly Bindable<User> hostBind = new Bindable<User>();
        private readonly Bindable<RoomStatus> statusBind = new Bindable<RoomStatus>();
        private readonly Bindable<GameType> typeBind = new Bindable<GameType>();
        private readonly Bindable<BeatmapInfo> beatmapBind = new Bindable<BeatmapInfo>();
        private readonly Bindable<User[]> participantsBind = new Bindable<User[]>();

        public readonly Room Room;

        private SelectionState state;
        public SelectionState State
        {
            get { return state; }
            set
            {
                if (value == state) return;
                state = value;

                if (state == SelectionState.Selected)
                    selectionBox.FadeIn(transition_duration);
                else
                    selectionBox.FadeOut(transition_duration);

                StateChanged?.Invoke(State);
            }
        }

        public IEnumerable<string> FilterTerms => new[] { Room.Name.Value };

        private bool matchingFilter;
        public bool MatchingFilter
        {
            get { return matchingFilter; }
            set
            {
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

        public event Action<SelectionState> StateChanged;

        public DrawableRoom(Room room)
        {
            Room = room;

            RelativeSizeAxes = Axes.X;
            Height = height + SELECTION_BORDER_WIDTH * 2;
            CornerRadius = corner_radius + SELECTION_BORDER_WIDTH / 2;
            Masking = true;

            // create selectionBox here so State can be set before being loaded
            selectionBox = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0f,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, LocalisationEngine localisation)
        {
            Box sideStrip;
            Container coverContainer;
            OsuSpriteText name, status, beatmapTitle, beatmapDash, beatmapArtist;
            ParticipantInfo participantInfo;
            ModeTypeInfo modeTypeInfo;

            Children = new Drawable[]
            {
                selectionBox,
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(SELECTION_BORDER_WIDTH),
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = corner_radius,
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
                                            new FillFlowContainer<OsuSpriteText>
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Colour = colours.Gray9,
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

            nameBind.ValueChanged += n => name.Text = n;
            hostBind.ValueChanged += h => participantInfo.Host = h;
            typeBind.ValueChanged += m => modeTypeInfo.Type = m;
            participantsBind.ValueChanged += p => participantInfo.Participants = p;

            statusBind.ValueChanged += s =>
            {
                status.Text = s.Message;

                foreach (Drawable d in new Drawable[] { selectionBox, sideStrip, status })
                    d.FadeColour(s.GetAppropriateColour(colours), transition_duration);
            };

            beatmapBind.ValueChanged += b =>
            {
                modeTypeInfo.Beatmap = b;

                if (b != null)
                {
                    coverContainer.FadeIn(transition_duration);

                    LoadComponentAsync(new BeatmapSetCover(b.BeatmapSet)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        FillMode = FillMode.Fill,
                        OnLoadComplete = d => d.FadeInFromZero(400, Easing.Out),
                    }, coverContainer.Add);

                    beatmapTitle.Current = localisation.GetUnicodePreference(b.Metadata.TitleUnicode, b.Metadata.Title);
                    beatmapDash.Text = @" - ";
                    beatmapArtist.Current = localisation.GetUnicodePreference(b.Metadata.ArtistUnicode, b.Metadata.Artist);
                }
                else
                {
                    coverContainer.FadeOut(transition_duration);

                    beatmapTitle.Current = null;
                    beatmapArtist.Current = null;

                    beatmapTitle.Text = "Changing map";
                    beatmapDash.Text = beatmapArtist.Text = string.Empty;
                }
            };

            nameBind.BindTo(Room.Name);
            hostBind.BindTo(Room.Host);
            statusBind.BindTo(Room.Status);
            typeBind.BindTo(Room.Type);
            beatmapBind.BindTo(Room.Beatmap);
            participantsBind.BindTo(Room.Participants);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeInFromZero(transition_duration);
        }

        protected override bool OnClick(InputState state)
        {
            if (Enabled.Value)
            {
                Action?.Invoke(this);
                State = SelectionState.Selected;
            }

            return true;
        }
    }
}
