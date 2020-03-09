// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;

namespace osu.Game.Tournament.Components
{
    public class DrawableTournamentTitleText : TournamentSpriteText
    {
        public DrawableTournamentTitleText()
        {
            Text = "osu!taiko world cup 2020";
            Font = OsuFont.Torus.With(size: 26, weight: FontWeight.SemiBold);
        }
    }
}
