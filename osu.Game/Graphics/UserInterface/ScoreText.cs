// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class ScoreText : OsuSpriteText
    {
        protected override bool UseFixedWidthForCharacter(char c)
        {
            switch (c)
            {
                case ' ':
                    return false;
            }

            return base.UseFixedWidthForCharacter(c);
        }
    }
}
