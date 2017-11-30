// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Edit
{
    public class PlayfieldOverlay : CompositeDrawable
    {

        private Playfield playfield;

        public PlayfieldOverlay(Playfield playfield)
        {
            this.playfield = playfield;

            RelativeSizeAxes = Axes.Both;

        }

    }
}
