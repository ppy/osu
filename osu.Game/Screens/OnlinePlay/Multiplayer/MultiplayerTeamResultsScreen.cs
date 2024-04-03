// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerTeamResultsScreen : MultiplayerResultsScreen
    {
        private readonly SortedDictionary<int, BindableLong> teamScores;

        private Container winnerBackground;
        private Drawable winnerText;

        public MultiplayerTeamResultsScreen(ScoreInfo score, long roomId, PlaylistItem playlistItem, SortedDictionary<int, BindableLong> teamScores)
            : base(score, roomId, playlistItem)
        {
            if (teamScores.Count != 2)
                throw new NotSupportedException(@"This screen currently only supports 2 teams");

            this.teamScores = teamScores;
        }

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float winner_background_half_height = 250;

            ResultsContent.Anchor = ResultsContent.Origin = Anchor.TopCentre;
            ResultsContent.Scale = new Vector2(0.9f);
            ResultsContent.Y = 75;

            var redScore = teamScores.First().Value;
            var blueScore = teamScores.Last().Value;

            LocalisableString winner;
            Colour4 winnerColour;

            int comparison = redScore.Value.CompareTo(blueScore.Value);

            if (comparison < 0)
            {
                // team name should eventually be coming from the multiplayer match state.
                winner = MultiplayerTeamResultsScreenStrings.TeamWins(@"Blue");
                winnerColour = colours.TeamColourBlue;
            }
            else if (comparison > 0)
            {
                // team name should eventually be coming from the multiplayer match state.
                winner = MultiplayerTeamResultsScreenStrings.TeamWins(@"Red");
                winnerColour = colours.TeamColourRed;
            }
            else
            {
                winner = MultiplayerTeamResultsScreenStrings.TheTeamsAreTied;
                winnerColour = Colour4.White.Opacity(0.5f);
            }

            AddRangeInternal(new Drawable[]
            {
                new MatchScoreDisplay
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Team1Score = { BindTarget = redScore },
                    Team2Score = { BindTarget = blueScore },
                },
                winnerBackground = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = winner_background_half_height,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.BottomCentre,
                            Colour = ColourInfo.GradientVertical(Colour4.Black.Opacity(0), Colour4.Black.Opacity(0.4f))
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = winner_background_half_height,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Colour = ColourInfo.GradientVertical(Colour4.Black.Opacity(0.4f), Colour4.Black.Opacity(0))
                        }
                    }
                },
                (winnerText = new OsuSpriteText
                {
                    Alpha = 0,
                    Font = OsuFont.Torus.With(size: 80, weight: FontWeight.Bold),
                    Text = winner,
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            using (BeginDelayedSequence(300))
            {
                const double fade_in_duration = 600;

                winnerText.FadeInFromZero(fade_in_duration, Easing.InQuint);
                winnerBackground.FadeInFromZero(fade_in_duration, Easing.InQuint);

                winnerText
                    .ScaleTo(10)
                    .ScaleTo(1, 600, Easing.InQuad)
                    .Then()
                    .ScaleTo(1.02f, 1600, Easing.OutQuint)
                    .FadeOut(5000, Easing.InQuad);
                winnerBackground.Delay(2200).FadeOut(2000);
            }
        }
    }
}
