// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.Containers
{
    public class OsuClickableContainer : ClickableContainer, IHasTooltip
    {
        private readonly HoverSampleSet sampleSet;

        private readonly Container content = new Container { RelativeSizeAxes = Axes.Both };

        protected override Container<Drawable> Content => content;

        protected virtual HoverClickSounds CreateHoverClickSounds(HoverSampleSet sampleSet) => new HoverClickSounds(sampleSet);

        public OsuClickableContainer(HoverSampleSet sampleSet = HoverSampleSet.Normal)
        {
            this.sampleSet = sampleSet;
        }

        public virtual string TooltipText { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (AutoSizeAxes != Axes.None)
            {
                content.RelativeSizeAxes = (Axes.Both & ~AutoSizeAxes);
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
