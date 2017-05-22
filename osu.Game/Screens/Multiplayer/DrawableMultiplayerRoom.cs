// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Screens.Multiplayer
{
    public class DrawableMultiplayerRoom : ClickableContainer
    {
        private const float content_padding = 5;
        private const float height = 90;

        private readonly Box sideStrip;
        private readonly OsuSpriteText status;
        private readonly OsuSpriteText host;
        private readonly OsuSpriteText rankBounds;
        private readonly FillFlowContainer<OsuSpriteText> beatmapInfoFlow;
        private readonly OsuSpriteText beatmapTitle;
        private readonly OsuSpriteText beatmapArtist;

        private Color4 openColour;
        private Color4 playingColour;

        public readonly MultiplayerRoom Room;

        public DrawableMultiplayerRoom(MultiplayerRoom room)
        {
            Room = room;

            RelativeSizeAxes = Axes.X;
            Height = height;
            CornerRadius = 5;
            Masking = true;
            EdgeEffect = new EdgeEffect
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
                    Colour = OsuColour.Gray(34),
                },
                new Background(@"Backgrounds/bg4")
                {
                    RelativeSizeAxes = Axes.Both,
                },
                sideStrip = new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 5,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = content_padding, Bottom = content_padding * 2, Left = Height + content_padding * 2, Right = content_padding },
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
                                new OsuSpriteText
                                {
                                    Text = Room.Name,
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
                                            RelativeSizeAxes = Axes.Y,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(5f, 0f),
                                            Children = new Drawable[]
                                            {
                                                new DrawableFlag(Room.Host?.Country?.FlagName ?? "__")
                                                {
                                                    Width = 30f,
                                                    RelativeSizeAxes = Axes.Y,
                                                },
                                                new Container
                                                {
                                                    Width = 40f,
                                                    RelativeSizeAxes = Axes.Y,
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
                                                    Text = Room.Host?.Username ?? @"",
                                                    TextSize = 14,
                                                    Font = @"Exo2.0-BoldItalic",
                                                },
                                            },
                                        },
                                        rankBounds = new OsuSpriteText
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Text = "#6895 - #50024",
                                            TextSize = 14,
                                            Margin = new MarginPadding { Right = 10 },
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
                                        new OsuSpriteText
                                        {
                                            Text = @" - ",
                                            TextSize = 14,
                                            Font = @"Exo2.0-RegularItalic",
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
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, LocalisationEngine localisation)
        {
            openColour = colours.GreenLight;
            playingColour = colours.Purple;
            beatmapInfoFlow.Colour = rankBounds.Colour = colours.Gray9;
            host.Colour = colours.Blue;

            if (Room.CurrentBeatmap != null)
            {
                beatmapTitle.Current = localisation.GetUnicodePreference(Room.CurrentBeatmap.TitleUnicode, Room.CurrentBeatmap.Title);
                beatmapArtist.Current = localisation.GetUnicodePreference(Room.CurrentBeatmap.ArtistUnicode, Room.CurrentBeatmap.Artist);
            }

            updateStatus();
        }

        private void updateStatus()
        {
            if (Room == null) return;

            status.Text = Room.Status.GetDescription();

            foreach (Drawable d in new Drawable[] { sideStrip, status })
                d.FadeColour(Room.Status == MultiplayerRoomStatus.Playing? playingColour : openColour);
        }
    }
}
