// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class RoomInspector : Container
    {
        private const float transition_duration = 100;

        public readonly IBindable<Room> Room = new Bindable<Room>();

        private readonly MarginPadding contentPadding = new MarginPadding { Horizontal = 20, Vertical = 10 };

        private readonly RoomBindings bindings = new RoomBindings();

        private OsuColour colours;
        private Box statusStrip;
        private UpdateableBeatmapBackgroundSprite background;
        private ParticipantCountDisplay participantCount;
        private OsuSpriteText name, status;
        private BeatmapTypeInfo beatmapTypeInfo;
        private ParticipantInfo participantInfo;
        private MatchParticipants participants;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            this.colours = colours;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"343138"),
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Distributed),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 200,
                                        Masking = true,
                                        Children = new Drawable[]
                                        {
                                            background = new UpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both },
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.5f), Color4.Black.Opacity(0)),
                                            },
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding(20),
                                                Children = new Drawable[]
                                                {
                                                    participantCount = new ParticipantCountDisplay
                                                    {
                                                        Anchor = Anchor.TopRight,
                                                        Origin = Anchor.TopRight,
                                                    },
                                                    name = new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.BottomLeft,
                                                        Origin = Anchor.BottomLeft,
                                                        TextSize = 30,
                                                    },
                                                },
                                            },
                                        },
                                    },
                                    statusStrip = new Box
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 5,
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = OsuColour.FromHex(@"28242d"),
                                            },
                                            new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Direction = FillDirection.Vertical,
                                                LayoutDuration = transition_duration,
                                                Padding = contentPadding,
                                                Spacing = new Vector2(0f, 5f),
                                                Children = new Drawable[]
                                                {
                                                    status = new OsuSpriteText
                                                    {
                                                        TextSize = 14,
                                                        Font = @"Exo2.0-Bold",
                                                    },
                                                    beatmapTypeInfo = new BeatmapTypeInfo(),
                                                },
                                            },
                                        },
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = contentPadding,
                                        Children = new Drawable[]
                                        {
                                            participantInfo = new ParticipantInfo(),
                                        },
                                    },
                                },
                            },
                        },
                        new Drawable[]
                        {
                            participants = new MatchParticipants
                            {
                                RelativeSizeAxes = Axes.Both,
                            }
                        }
                    }
                }
            };

            participantInfo.Host.BindTo(bindings.Host);
            participantInfo.ParticipantCount.BindTo(bindings.ParticipantCount);
            participantInfo.Participants.BindTo(bindings.Participants);
            participantCount.Participants.BindTo(bindings.Participants);
            participantCount.ParticipantCount.BindTo(bindings.ParticipantCount);
            participantCount.MaxParticipants.BindTo(bindings.MaxParticipants);
            beatmapTypeInfo.Beatmap.BindTo(bindings.CurrentBeatmap);
            beatmapTypeInfo.Ruleset.BindTo(bindings.CurrentRuleset);
            beatmapTypeInfo.Type.BindTo(bindings.Type);
            background.Beatmap.BindTo(bindings.CurrentBeatmap);
            bindings.Status.BindValueChanged(displayStatus);
            bindings.Name.BindValueChanged(n => name.Text = n);
            Room.BindValueChanged(updateRoom, true);
        }

        private void updateRoom(Room room)
        {
            bindings.Room = room;
            participants.Room = room;

            if (room != null)
            {
                participantCount.FadeIn(transition_duration);
                beatmapTypeInfo.FadeIn(transition_duration);
                name.FadeIn(transition_duration);
                participantInfo.FadeIn(transition_duration);
            }
            else
            {
                participantCount.FadeOut(transition_duration);
                beatmapTypeInfo.FadeOut(transition_duration);
                name.FadeOut(transition_duration);
                participantInfo.FadeOut(transition_duration);

                displayStatus(new RoomStatusNoneSelected());
            }
        }

        private void displayStatus(RoomStatus s)
        {
            status.Text = s.Message;

            Color4 c = s.GetAppropriateColour(colours);
            statusStrip.FadeColour(c, transition_duration);
            status.FadeColour(c, transition_duration);
        }

        private class RoomStatusNoneSelected : RoomStatus
        {
            public override string Message => @"No Room Selected";
            public override Color4 GetAppropriateColour(OsuColour colours) => colours.Gray8;
        }

        private class MatchParticipants : CompositeDrawable
        {
            private Room room;
            private readonly FillFlowContainer fill;

            public Room Room
            {
                get { return room; }
                set
                {
                    if (room == value)
                        return;

                    room = value;
                    updateParticipants();
                }
            }

            public MatchParticipants()
            {
                Padding = new MarginPadding { Horizontal = 10 };

                InternalChild = new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = fill = new FillFlowContainer
                    {
                        Spacing = new Vector2(10),
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                    }
                };
            }

            [Resolved]
            private APIAccess api { get; set; }

            private GetRoomScoresRequest request;

            private void updateParticipants()
            {
                var roomId = room?.RoomID.Value ?? 0;

                request?.Cancel();

                // nice little progressive fade
                int time = 500;
                foreach (var c in fill.Children)
                {
                    c.Delay(500 - time).FadeOut(time, Easing.Out);
                    time = Math.Max(20, time - 20);
                    c.Expire();
                }

                if (roomId == 0) return;

                request = new GetRoomScoresRequest(roomId);
                request.Success += scores =>
                {
                    if (roomId != room.RoomID.Value)
                        return;

                    fill.Clear();
                    foreach (var s in scores)
                        fill.Add(new UserTile(s.User));

                    fill.FadeInFromZero(1000, Easing.OutQuint);
                };

                api.Queue(request);
            }

            protected override void Dispose(bool isDisposing)
            {
                request?.Cancel();
                base.Dispose(isDisposing);
            }

            private class UserTile : CompositeDrawable, IHasTooltip
            {
                private readonly User user;

                public string TooltipText => user.Username;

                public UserTile(User user)
                {
                    this.user = user;
                    Size = new Vector2(70f);
                    CornerRadius = 5f;
                    Masking = true;

                    InternalChildren = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.FromHex(@"27252d"),
                        },
                        new UpdateableAvatar
                        {
                            RelativeSizeAxes = Axes.Both,
                            User = user,
                        },
                    };
                }
            }
        }
    }
}
