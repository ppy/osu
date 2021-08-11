// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Play.HUD;
using osu.Game.Localisation;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerResultsScreen : PlaylistsResultsScreen
    {
        private readonly SortedDictionary<int, BindableInt> teamScores;

        private Drawable winnerText;

        public MultiplayerResultsScreen(ScoreInfo score, long roomId, PlaylistItem playlistItem, SortedDictionary<int, BindableInt> teamScores)
            : base(score, roomId, playlistItem, false, false)
        {
            this.teamScores = teamScores;
        }

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (teamScores.Count == 2)
            {
                var redScore = teamScores.First().Value;
                var blueScore = teamScores.Last().Value;

                // eventually this will be replaced by team names coming from the multiplayer match state.
                string winner = redScore.Value > blueScore.Value ? @"Red" : @"Blue";

                var winnerColour = redScore.Value > blueScore.Value ? colours.TeamColourRed : colours.TeamColourBlue;

                AddRangeInternal(new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new MatchScoreDisplay
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Team1Score = { BindTarget = redScore },
                                Team2Score = { BindTarget = blueScore },
                            },
                            (winnerText = new OsuSpriteText
                            {
                                Alpha = 0,
                                Font = OsuFont.Torus.With(size: 40, weight: FontWeight.Bold),
                                Text = MultiplayerResultsScreenStrings.TeamWins(winner)
                            }).WithEffect(new GlowEffect
                            {
                                Colour = winnerColour,
                            }).With(e =>
                            {
                                e.Anchor = Anchor.TopCentre;
                                e.Origin = Anchor.TopCentre;
                            })
                        }
                    },
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            using (BeginDelayedSequence(300))
            {
                winnerText.FadeInFromZero(600, Easing.InQuint);

                winnerText
                    .ScaleTo(10)
                    .ScaleTo(1, 600, Easing.InQuad)
                    .Then()
                    .ScaleTo(1.02f, 1600, Easing.OutQuint);
            }
        }
    }
}
