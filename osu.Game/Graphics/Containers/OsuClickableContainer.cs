// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.Containers
{
    public class OsuClickableContainer : ClickableContainer
    {
        private readonly HoverSampleSet sampleSet;

        private readonly Container content = new Container { RelativeSizeAxes = Axes.Both };

        protected override Container<Drawable> Content => content;

        protected virtual HoverClickSounds CreateHoverClickSounds(HoverSampleSet sampleSet) => new HoverClickSounds(sampleSet);

        public OsuClickableContainer(HoverSampleSet sampleSet = HoverSampleSet.Normal)
        {
            this.sampleSet = sampleSet;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (AutoSizeAxes != Axes.None)
            {
                content.RelativeSizeAxes = RelativeSizeAxes;
                content.AutoSizeAxes = AutoSizeAxes;
            }

            InternalChildren = new Drawable[]
            {
                content,
                CreateHoverClickSounds(sampleSet)
            };
        }
    }
}
