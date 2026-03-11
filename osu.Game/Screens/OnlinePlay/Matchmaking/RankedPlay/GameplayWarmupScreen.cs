// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class GameplayWarmupScreen : RankedPlaySubScreen
    {
        public override bool ShowBeatmapBackground => true;

        protected override LocalisableString StageHeading => "Gameplay";
        protected override LocalisableString StageCaption => string.Empty;

        [Cached(typeof(IBindable<SongSelect.BeatmapSetLookupResult?>))]
        private readonly Bindable<SongSelect.BeatmapSetLookupResult?> lastLookupResult = new Bindable<SongSelect.BeatmapSetLookupResult?>();

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private RankedPlayMatchInfo matchInfo { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider overlayColours { get; set; } = null!;

        private Container<RankedPlayCard> cardColumn = null!;
        private Drawable separator = null!;
        private Drawable detailsColumn = null!;
        private Drawable wedgesContainer = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            APIBeatmap beatmap = beatmapLookupCache.GetBeatmapAsync(Client.Room!.CurrentPlaylistItem.BeatmapID).GetResultSafely()!;
            lastLookupResult.Value = SongSelect.BeatmapSetLookupResult.Completed(beatmap.BeatmapSet);

            var matchState = Client.Room?.MatchState as RankedPlayRoomState;
            Debug.Assert(matchState != null);

            Children =
            [
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.5f,
                    Spacing = new Vector2(20),
                    LayoutDuration = 500,
                    LayoutEasing = Easing.OutPow10,
                    Children = new[]
                    {
                        cardColumn = new Container<RankedPlayCard>
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                        },
                        separator = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(2, 50),
                            Scale = new Vector2(1, 0),
                            Alpha = 0,
                            Colour = overlayColours.Colour0
                        },
                        detailsColumn = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Masking = true,
                            Scale = new Vector2(0.8f),
                            Alpha = 0,
                            Child = wedgesContainer = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = OsuGame.SHEAR,
                                X = -20,
                                Padding = new MarginPadding
                                {
                                    Left = -SongSelect.CORNER_RADIUS_HIDE_OFFSET,
                                },
                                Child = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Spacing = new Vector2(0f, 4f),
                                    Direction = FillDirection.Vertical,
                                    Children =
                                    [
                                        new ShearAligningWrapper(new TitleWedge(beatmap))
                                        {
                                            Shear = -OsuGame.SHEAR,
                                        },
                                        new ShearAligningWrapper(new MetadataWedge(beatmap))
                                        {
                                            Shear = -OsuGame.SHEAR,
                                        },
                                    ]
                                }
                            }
                        }
                    }
                }
            ];
        }

        public override void OnEntering(RankedPlaySubScreen? previous)
        {
            base.OnEntering(previous);

            if (matchInfo.LastPlayedCard == null)
                return;

            RankedPlayCard? card = null;

            switch (previous)
            {
                case PickScreen pick:
                {
                    if (pick.CenterRow.RemoveCard(matchInfo.LastPlayedCard, out card, out var screenSpaceDrawQuad))
                        card.MatchScreenSpaceDrawQuad(screenSpaceDrawQuad, cardColumn);
                    break;
                }

                case OpponentPickScreen opponentPick:
                {
                    if (opponentPick.CenterRow.RemoveCard(matchInfo.LastPlayedCard, out card, out var screenSpaceDrawQuad))
                        card.MatchScreenSpaceDrawQuad(screenSpaceDrawQuad, cardColumn);
                    break;
                }
            }

            if (card == null)
            {
                Logger.Log($"Played card {matchInfo.LastPlayedCard.Card.ID} was not on the screen.", level: LogLevel.Error);

                card = new RankedPlayCard(matchInfo.LastPlayedCard)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            }

            cardColumn.Add(card);

            separator.AlwaysPresent = true;
            detailsColumn.AlwaysPresent = true;

            using (BeginDelayedSequence(500))
            {
                separator.FadeIn();
                separator.ScaleTo(Vector2.One, 1000, Easing.OutPow10);

                using (BeginDelayedSequence(200))
                {
                    detailsColumn.FadeIn(800, Easing.OutPow10);
                    wedgesContainer.MoveToX(0, 1000, Easing.OutPow10);
                }
            }
        }
    }
}
