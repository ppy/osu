// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamHeader : TournamentSpriteTextWithBackground
    {
        public DrawableTeamHeader(TeamColour colour)
        {
            Background.Colour = TournamentGame.GetTeamColour(colour);

            Text.Colour = TournamentGame.TEXT_COLOUR;
            Text.Text = $"Team {colour}".ToUpperInvariant();
            Text.Scale = new Vector2(0.6f);
        }
    }
}
