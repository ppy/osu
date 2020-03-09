// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;

namespace osu.Game.Tournament.Components
{
    public class DrawableTournamentTitleText : TournamentSpriteText
    {
        public DrawableTournamentTitleText()
        {
            Text = "标题需要从源代码更改　osu.Game.Tournament/Components/DrawableTournamentTitleText.cs";
            Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold);
        }
    }
}
