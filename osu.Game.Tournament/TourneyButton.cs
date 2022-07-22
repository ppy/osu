// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tournament
{
    public class TourneyButton : OsuButton
    {
        public new Box Background => base.Background;

        public TourneyButton()
            : base(null)
        {
        }
    }
}
