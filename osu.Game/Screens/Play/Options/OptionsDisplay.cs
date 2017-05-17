// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Play.Options
{
    public class OptionsDisplay : FillFlowContainer<OptionContainer>
    {
        public OptionsDisplay()
        {
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(0, 20);

            Add(new PlaybackOption());
            Add(new PlaybackOption());
            Add(new PlaybackOption());
        }
    }
}
