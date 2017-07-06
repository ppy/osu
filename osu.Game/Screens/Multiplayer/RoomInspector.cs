// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Screens.Multiplayer
{
    public class RoomInspector : Container
    {
        private readonly MarginPadding contentPadding = new MarginPadding { Horizontal = 20, Vertical = 10 };
        private const float transition_duration = 100;
        private const float ruleset_height = 30;

        private readonly Box statusStrip;
        private readonly Container coverContainer, rulesetContainer, gameTypeContainer, flagContainer;
        private readonly FillFlowContainer topFlow, levelRangeContainer, participantsFlow;
        private readonly OsuSpriteText participants, participantsSlash, maxParticipants, name, status, beatmapTitle, beatmapDash, beatmapArtist, beatmapAuthor, host, levelRangeLower, levelRangeHigher;
        private readonly ScrollContainer participantsScroll;

        private readonly Bindable<string> nameBind = new Bindable<string>();
        private readonly Bindable<User> hostBind = new Bindable<User>();
        private readonly Bindable<RoomStatus> statusBind = new Bindable<RoomStatus>();
        private readonly Bindable<GameType> typeBind = new Bindable<GameType>();
        private readonly Bindable<BeatmapInfo> beatmapBind = new Bindable<BeatmapInfo>();
        private readonly Bindable<int?> maxParticipantsBind = new Bindable<int?>();
        private readonly Bindable<User[]> participantsBind = new Bindable<User[]>();

        private OsuColour colours;
        private LocalisationEngine localisation;
        private TextureStore textures;

        private Room room;

        public Room Room
        {
            get { return room; }
            set
            {
                if (value == room) return;
                room = value;

                nameBind.BindTo(Room.Name);
                hostBind.BindTo(Room.Host);
                statusBind.BindTo(Room.Status);
                typeBind.BindTo(Room.Type);
                beatmapBind.BindTo(Room.Beatmap);
                maxParticipantsBind.BindTo(Room.MaxParticipants);
                participantsBind.BindTo(Room.Participants);
            }
        }

        public RoomInspector()
        {
            Width = 520;
            RelativeSizeAxes = Axes.Y;

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
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
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
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ColourInfo = ColourInfo.GradientVertical(Color4.Black.Opacity(0.5f), Color4.Black.Opacity(0)),
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(20),
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer
                                        {
                                            Anchor = Anchor.TopRight,
                                            Origin = Anchor.TopRight,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            LayoutDuration = transition_duration,
                                            Children = new[]
                                            {
                                                participants = new OsuSpriteText
                                                {
                                                    TextSize = 30,
                                                    Font = @"Exo2.0-Bold"
                                                },
                                                participantsSlash = new OsuSpriteText
                                                {
                                                    Text = @"/",
                                                    TextSize = 30,
                                                    Font = @"Exo2.0-Light"
                                                },
                                                maxParticipants = new OsuSpriteText
                                                {
                                                    TextSize = 30,
                                                    Font = @"Exo2.0-Light"
                                                },
                                            },
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
                                    Padding = contentPadding,
                                    Spacing = new Vector2(0f, 5f),
                                    Children = new Drawable[]
                                    {
                                        status = new OsuSpriteText
                                        {
                                            TextSize = 14,
                                            Font = @"Exo2.0-Bold",
                                        },
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.X,
                                            Height = ruleset_height,
                                            Direction = FillDirection.Horizontal,
                                            LayoutDuration = transition_duration,
                                            Spacing = new Vector2(5f, 0f),
                                            Children = new Drawable[]
                                            {
                                                rulesetContainer = new Container
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                },
                                                gameTypeContainer = new Container
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                },
                                                new Container
                                                {
                                                    AutoSizeAxes = Axes.X,
                                                    RelativeSizeAxes = Axes.Y,
                                                    Margin = new MarginPadding { Left = 5 },
                                                    Children = new[]
                                                    {
                                                        new FillFlowContainer
                                                        {
                                                            AutoSizeAxes = Axes.Both,
                                                            Direction = FillDirection.Horizontal,
                                                            Children = new[]
                                                            {
                                                                beatmapTitle = new OsuSpriteText
                                                                {
                                                                    Font = @"Exo2.0-BoldItalic",
                                                                },
                                                                beatmapDash = new OsuSpriteText
                                                                {
                                                                    Font = @"Exo2.0-BoldItalic",
                                                                },
                                                                beatmapArtist = new OsuSpriteText
                                                                {
                                                                    Font = @"Exo2.0-RegularItalic",
                                                                },
                                                            },
                                                        },
                                                        beatmapAuthor = new OsuSpriteText
                                                        {
                                                            Anchor = Anchor.BottomLeft,
                                                            Origin = Anchor.BottomLeft,
                                                            TextSize = 14,
                                                        },
                                                    },
                                                },
                                            },
                                        },
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
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.X,
                                    Height = 15f,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5f, 0f),
                                    Children = new Drawable[]
                                    {
                                        flagContainer = new Container
                                        {
                                            Width = 22f,
                                            RelativeSizeAxes = Axes.Y,
                                        },
                                        new Container //todo: team banners
                                        {
                                            Width = 38f,
                                            RelativeSizeAxes = Axes.Y,
                                            CornerRadius = 2f,
                                            Masking = true,
                                            Children = new[]
                                            {
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = OsuColour.FromHex(@"ad387e"),
                                                },
                                            },
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "hosted by",
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            TextSize = 14,
                                        },
                                        host = new OsuSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            TextSize = 14,
                                            Font = @"Exo2.0-BoldItalic",
                                        },
                                    },
                                },
                                levelRangeContainer = new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Children = new[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = "Rank Range ",
                                            TextSize = 14,
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "#",
                                            TextSize = 14,
                                        },
                                        levelRangeLower = new OsuSpriteText
                                        {
                                            TextSize = 14,
                                            Font = @"Exo2.0-Bold",
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = " - ",
                                            TextSize = 14,
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "#",
                                            TextSize = 14,
                                        },
                                        levelRangeHigher = new OsuSpriteText
                                        {
                                            Text = "6251",
                                            TextSize = 14,
                                            Font = @"Exo2.0-Bold",
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                participantsScroll = new ScrollContainer
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

            nameBind.ValueChanged += displayName;
            hostBind.ValueChanged += displayUser;
            maxParticipantsBind.ValueChanged += displayMaxParticipants;
            participantsBind.ValueChanged += displayParticipants;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, LocalisationEngine localisation, TextureStore textures)
        {
            this.localisation = localisation;
            this.colours = colours;
            this.textures = textures;

            beatmapAuthor.Colour = levelRangeContainer.Colour = colours.Gray9;
            host.Colour = colours.Blue;

            //binded here instead of ctor because dependencies are needed
            statusBind.ValueChanged += displayStatus;
            typeBind.ValueChanged += displayGameType;
            beatmapBind.ValueChanged += displayBeatmap;

            statusBind.TriggerChange();
            typeBind.TriggerChange();
            beatmapBind.TriggerChange();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            participantsScroll.Height = DrawHeight - topFlow.DrawHeight;
        }

        private void displayName(string value)
        {
            name.Text = value;
        }

        private void displayUser(User value)
        {
            host.Text = value.Username;
            flagContainer.Children = new[]
            {
                new DrawableFlag(value.Country?.FlagName ?? @"__")
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        private void displayStatus(RoomStatus value)
        {
            status.Text = value.Message;

            foreach (Drawable d in new Drawable[] { statusStrip, status })
                d.FadeColour(value.GetAppropriateColour(colours), transition_duration);
        }

        private void displayGameType(GameType value)
        {
            gameTypeContainer.Children = new[]
            {
                new DrawableGameType(value)
                {
                    Size = new Vector2(ruleset_height),
                },
            };
        }

        private void displayBeatmap(BeatmapInfo value)
        {
            if (value != null)
            {
                coverContainer.FadeIn(transition_duration);
                coverContainer.Children = new[]
                {
                    new AsyncLoadWrapper(new BeatmapBackgroundSprite(new OnlineWorkingBeatmap(value, textures, null))
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        FillMode = FillMode.Fill,
                        OnLoadComplete = d => d.FadeInFromZero(400, EasingTypes.Out),
                    }) { RelativeSizeAxes = Axes.Both }
                };

                rulesetContainer.FadeIn(transition_duration);
                rulesetContainer.Children = new[]
                {
                    new DifficultyIcon(value)
                    {
                        Size = new Vector2(ruleset_height),
                    }
                };

                beatmapTitle.Current = localisation.GetUnicodePreference(value.Metadata.TitleUnicode, value.Metadata.Title);
                beatmapDash.Text = @" - ";
                beatmapArtist.Current = localisation.GetUnicodePreference(value.Metadata.ArtistUnicode, value.Metadata.Artist);
                beatmapAuthor.Text = $"mapped by {value.Metadata.Author}";
            }
            else
            {
                coverContainer.FadeOut(transition_duration);
                rulesetContainer.FadeOut(transition_duration);

                beatmapTitle.Current = null;
                beatmapArtist.Current = null;

                beatmapTitle.Text = "Changing map";
                beatmapDash.Text = beatmapArtist.Text = beatmapAuthor.Text = string.Empty;
            }
        }

        private void displayMaxParticipants(int? value)
        {
            if (value == null)
            {
                participantsSlash.FadeOut(transition_duration);
                maxParticipants.FadeOut(transition_duration);
            }
            else
            {
                participantsSlash.FadeIn(transition_duration);
                maxParticipants.FadeIn(transition_duration);
                maxParticipants.Text = value.ToString();
            }
        }

        private void displayParticipants(User[] value)
        {
            participants.Text = value.Length.ToString();

            var ranks = value.Select(u => u.GlobalRank);
            levelRangeLower.Text = ranks.Min().ToString();
            levelRangeHigher.Text = ranks.Max().ToString();

            participantsFlow.ChildrenEnumerable = value.Select(u => new UserTile(u));
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
    }
}
