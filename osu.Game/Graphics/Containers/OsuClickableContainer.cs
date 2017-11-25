// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.Containers
{
    public class OsuClickableContainer : ClickableContainer
    {
        private readonly HoverSampleSet sampleSet;

        public OsuClickableContainer(HoverSampleSet sampleSet = HoverSampleSet.Normal)
        {
            this.sampleSet = sampleSet;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new HoverClickSounds(sampleSet));
        }
    }
}
