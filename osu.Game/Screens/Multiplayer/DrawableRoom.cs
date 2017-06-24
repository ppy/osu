// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
    public class DrawableRoom : ClickableContainer
    {
        private const float transition_duration = 100;
        private const float content_padding = 10;
        private const float height = 100;
        private const float side_strip_width = 5;
        private const float cover_width = 145;
        private const float ruleset_height = 30;

        private readonly Box sideStrip;
        private readonly Container coverContainer, rulesetContainer, gameTypeContainer;
        private readonly OsuSpriteText name;
        private readonly Container flagContainer;
        private readonly OsuSpriteText host;
        private readonly FillFlowContainer levelRangeContainer;
        private readonly OsuSpriteText levelRangeLower;
        private readonly OsuSpriteText levelRangeHigher;
        private readonly OsuSpriteText status;
        private readonly FillFlowContainer<OsuSpriteText> beatmapInfoFlow;
        private readonly OsuSpriteText beatmapTitle;
        private readonly OsuSpriteText beatmapDash;
        private readonly OsuSpriteText beatmapArtist;

        private readonly Bindable<string> nameBind = new Bindable<string>();
        private readonly Bindable<User> hostBind = new Bindable<User>();
        private readonly Bindable<RoomStatus> statusBind = new Bindable<RoomStatus>();
        private readonly Bindable<GameType> typeBind = new Bindable<GameType>();
        private readonly Bindable<BeatmapInfo> beatmapBind = new Bindable<BeatmapInfo>();
        private readonly Bindable<User[]> participantsBind = new Bindable<User[]>();

        private OsuColour colours;
        private TextureStore textures;
        private LocalisationEngine localisation;

        public readonly Room Room;

        public DrawableRoom(Room room)
        {
            Room = room;

            RelativeSizeAxes = Axes.X;
            Height = height;
            CornerRadius = 5;
            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(40),
                Radius = 5,
            };

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
                    Padding = new MarginPadding { Top = content_padding, Bottom = content_padding, Left = side_strip_width + cover_width + content_padding, Right = content_padding },
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
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 20f,
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
                                                    TextSize = 14,
                                                    Font = @"Exo2.0-Bold",
                                                },
                                            },
                                        },
                                    },
                                },
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
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Height = ruleset_height,
                            Direction = FillDirection.Horizontal,
                            LayoutDuration = transition_duration,
                            Spacing = new Vector2(5f, 0f),
                            Children = new[]
                            {
                                rulesetContainer = new Container
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    AutoSizeAxes = Axes.Both,
                                },
                                gameTypeContainer = new Container
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    AutoSizeAxes = Axes.Both,
                                },
                            },
                        },
                    },
                },
            };

            nameBind.ValueChanged += displayName;
            hostBind.ValueChanged += displayUser;
            participantsBind.ValueChanged += displayParticipants;

            nameBind.BindTo(Room.Name);
            hostBind.BindTo(Room.Host);
            statusBind.BindTo(Room.Status);
            typeBind.BindTo(Room.Type);
            beatmapBind.BindTo(Room.Beatmap);
            participantsBind.BindTo(Room.Participants);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures, LocalisationEngine localisation)
        {
            this.localisation = localisation;
            this.textures = textures;
            this.colours = colours;

            beatmapInfoFlow.Colour = levelRangeContainer.Colour = colours.Gray9;
            host.Colour = colours.Blue;

            //binded here instead of ctor because dependencies are needed
            statusBind.ValueChanged += displayStatus;
            typeBind.ValueChanged += displayGameType;
            beatmapBind.ValueChanged += displayBeatmap;

            statusBind.TriggerChange();
            typeBind.TriggerChange();
            beatmapBind.TriggerChange();
        }

        private void displayName(string value)
        {
            name.Text = value;
        }

        private void displayUser(User value)
        {
            host.Text = value.Username;
            flagContainer.Children = new[] { new DrawableFlag(value.Country?.FlagName ?? @"__") { RelativeSizeAxes = Axes.Both } };
        }

        private void displayStatus(RoomStatus value)
        {
            if (value == null) return;
            status.Text = value.Message;

            foreach (Drawable d in new Drawable[] { sideStrip, status })
                d.FadeColour(value.GetAppropriateColour(colours), 100);
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
            }
            else
            {
                coverContainer.FadeOut(transition_duration);
                rulesetContainer.FadeOut(transition_duration);

                beatmapTitle.Current = null;
                beatmapArtist.Current = null;

                beatmapTitle.Text = "Changing map";
                beatmapDash.Text = beatmapArtist.Text = string.Empty;
            }
        }

        private void displayParticipants(User[] value)
        {
            var ranks = value.Select(u => u.GlobalRank);
            levelRangeLower.Text = ranks.Min().ToString();
            levelRangeHigher.Text = ranks.Max().ToString();
        }
    }
}
