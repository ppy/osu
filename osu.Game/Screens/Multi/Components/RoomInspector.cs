// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
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
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Components
{
    public class RoomInspector : Container
    {
        private const float transition_duration = 100;

        private readonly MarginPadding contentPadding = new MarginPadding { Horizontal = 20, Vertical = 10 };
        private readonly Bindable<string> nameBind = new Bindable<string>();
        private readonly Bindable<User> hostBind = new Bindable<User>();
        private readonly Bindable<RoomStatus> statusBind = new Bindable<RoomStatus>();
        private readonly Bindable<GameType> typeBind = new Bindable<GameType>();
        private readonly Bindable<BeatmapInfo> beatmapBind = new Bindable<BeatmapInfo>();
        private readonly Bindable<int?> maxParticipantsBind = new Bindable<int?>();
        private readonly Bindable<IEnumerable<User>> participantsBind = new Bindable<IEnumerable<User>>();

        private OsuColour colours;
        private Box statusStrip;
        private UpdateableBeatmapSetCover cover;
        private ParticipantCount participantCount;
        private FillFlowContainer topFlow, participantsFlow;
        private OsuSpriteText name, status;
        private BeatmapTypeInfo beatmapTypeInfo;
        private ScrollContainer participantsScroll;
        private ParticipantInfo participantInfo;

        private Room room;
        public Room Room
        {
            get { return room; }
            set
            {
                if (value == room) return;
                room = value;

                nameBind.UnbindBindings();
                hostBind.UnbindBindings();
                statusBind.UnbindBindings();
                typeBind.UnbindBindings();
                beatmapBind.UnbindBindings();
                maxParticipantsBind.UnbindBindings();
                participantsBind.UnbindBindings();

                if (room != null)
                {
                    nameBind.BindTo(room.Name);
                    hostBind.BindTo(room.Host);
                    statusBind.BindTo(room.Status);
                    typeBind.BindTo(room.Type);
                    beatmapBind.BindTo(room.Beatmap);
                    maxParticipantsBind.BindTo(room.MaxParticipants);
                    participantsBind.BindTo(room.Participants);
                }

                updateState();
            }
        }

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
                topFlow = new FillFlowContainer
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
                                cover = new UpdateableBeatmapSetCover
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
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
                                        participantCount = new ParticipantCount
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
                                participantInfo = new ParticipantInfo(@"Rank Range "),
                            },
                        },
                    },
                },
                participantsScroll = new OsuScrollContainer
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding { Top = contentPadding.Top, Left = 38, Right = 37 },
                    Children = new[]
                    {
                        participantsFlow = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            LayoutDuration = transition_duration,
                            Spacing = new Vector2(5f),
                        },
                    },
                },
            };

            nameBind.ValueChanged += n => name.Text = n;
            hostBind.ValueChanged += h => participantInfo.Host = h;
            typeBind.ValueChanged += t => beatmapTypeInfo.Type = t;
            maxParticipantsBind.ValueChanged += m => participantCount.Max = m;
            statusBind.ValueChanged += displayStatus;

            beatmapBind.ValueChanged += b =>
            {
                cover.BeatmapSet = b?.BeatmapSet;
                beatmapTypeInfo.Beatmap = b;
            };

            participantsBind.ValueChanged += p =>
            {
                participantCount.Count = p.Count();
                participantInfo.Participants = p;
                participantsFlow.ChildrenEnumerable = p.Select(u => new UserTile(u));
            };

            updateState();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            participantsScroll.Height = DrawHeight - topFlow.DrawHeight;
        }

        private void displayStatus(RoomStatus s)
        {
            status.Text = s.Message;

            Color4 c = s.GetAppropriateColour(colours);
            statusStrip.FadeColour(c, transition_duration);
            status.FadeColour(c, transition_duration);
        }

        private void updateState()
        {
            if (Room == null)
            {
                cover.BeatmapSet = null;
                participantsFlow.FadeOut(transition_duration);
                participantCount.FadeOut(transition_duration);
                beatmapTypeInfo.FadeOut(transition_duration);
                name.FadeOut(transition_duration);
                participantInfo.FadeOut(transition_duration);

                displayStatus(new RoomStatusNoneSelected());
            }
            else
            {
                participantsFlow.FadeIn(transition_duration);
                participantCount.FadeIn(transition_duration);
                beatmapTypeInfo.FadeIn(transition_duration);
                name.FadeIn(transition_duration);
                participantInfo.FadeIn(transition_duration);

                statusBind.TriggerChange();
                beatmapBind.TriggerChange();
            }
        }

        private class UserTile : Container, IHasTooltip
        {
            private readonly User user;

            public string TooltipText => user.Username;

            public UserTile(User user)
            {
                this.user = user;
                Size = new Vector2(70f);
                CornerRadius = 5f;
                Masking = true;

                Children = new Drawable[]
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

        private class RoomStatusNoneSelected : RoomStatus
        {
            public override string Message => @"No Room Selected";
            public override Color4 GetAppropriateColour(OsuColour colours) => colours.Gray8;
        }
    }
}
