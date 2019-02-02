// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class DrawableRoom : OsuClickableContainer, IStateful<SelectionState>, IFilterable
    {
        public const float SELECTION_BORDER_WIDTH = 4;
        private const float corner_radius = 5;
        private const float transition_duration = 60;
        private const float content_padding = 10;
        private const float height = 110;
        private const float side_strip_width = 5;
        private const float cover_width = 145;

        public event Action<SelectionState> StateChanged;

        private readonly RoomBindings bindings = new RoomBindings();

        private readonly Box selectionBox;
        private UpdateableBeatmapBackgroundSprite background;
        private BeatmapTitle beatmapTitle;
        private ModeTypeInfo modeTypeInfo;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

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

        public DrawableRoom(Room room)
        {
            Room = room;
            bindings.Room = room;

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
        private void load(OsuColour colours)
        {
            Box sideStrip;
            ParticipantInfo participantInfo;
            OsuSpriteText name;

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
                                RelativeSizeAxes = Axes.Y,
                                Width = cover_width,
                                Masking = true,
                                Margin = new MarginPadding { Left = side_strip_width },
                                Child = background = new UpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both }
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
                                            name = new OsuSpriteText { TextSize = 18 },
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
                                        Spacing = new Vector2(0, 5),
                                        Children = new Drawable[]
                                        {
                                            new RoomStatusInfo(Room),
                                            beatmapTitle = new BeatmapTitle { TextSize = 14 },
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

            background.Beatmap.BindTo(bindings.CurrentBeatmap);
            modeTypeInfo.Beatmap.BindTo(bindings.CurrentBeatmap);
            modeTypeInfo.Ruleset.BindTo(bindings.CurrentRuleset);
            modeTypeInfo.Type.BindTo(bindings.Type);
            beatmapTitle.Beatmap.BindTo(bindings.CurrentBeatmap);
            participantInfo.Host.BindTo(bindings.Host);
            participantInfo.Participants.BindTo(bindings.Participants);
            participantInfo.ParticipantCount.BindTo(bindings.ParticipantCount);

            bindings.Name.BindValueChanged(n => name.Text = n, true);
            bindings.Status.BindValueChanged(s =>
            {
                foreach (Drawable d in new Drawable[] { selectionBox, sideStrip })
                    d.FadeColour(s.GetAppropriateColour(colours), transition_duration);
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeInFromZero(transition_duration);
        }
    }
}
