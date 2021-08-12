// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Play.HUD;
using osuTK;

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
            ScorePanelList.Anchor = ScorePanelList.Origin = Anchor.TopCentre;
            ScorePanelList.Scale = new Vector2(0.9f);
            ScorePanelList.Y = 75;

            if (teamScores.Count == 2)
            {
                var redScore = teamScores.First().Value;
                var blueScore = teamScores.Last().Value;

                // eventually this will be replaced by team names coming from the multiplayer match state.
                string winner = redScore.Value > blueScore.Value ? @"Red" : @"Blue";

                var winnerColour = redScore.Value > blueScore.Value ? colours.TeamColourRed : colours.TeamColourBlue;

                AddRangeInternal(new Drawable[]
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
                        Font = OsuFont.Torus.With(size: 80, weight: FontWeight.Bold),
                        Text = MultiplayerResultsScreenStrings.TeamWins(winner),
                        Blending = BlendingParameters.Additive
                    }).WithEffect(new GlowEffect
                    {
                        Colour = winnerColour,
                    }).With(e =>
                    {
                        e.Anchor = Anchor.Centre;
                        e.Origin = Anchor.Centre;
                    })
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
                    .ScaleTo(1.02f, 1600, Easing.OutQuint)
                    .FadeOut(5000, Easing.InQuad);
            }
        }
    }
}
