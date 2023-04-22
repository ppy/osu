// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    public partial class OsuClickableContainer : ClickableContainer, IHasTooltip
    {
        private readonly HoverSampleSet sampleSet;

        private readonly Container content = new Container { RelativeSizeAxes = Axes.Both };

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            // base call is checked for cases when `OsuClickableContainer` has masking applied to it directly (ie. externally in object initialisation).
            base.ReceivePositionalInputAt(screenSpacePos)
            // Implementations often apply masking / edge rounding at a content level, so it's imperative to check that as well.
            && Content.ReceivePositionalInputAt(screenSpacePos);

        protected override Container<Drawable> Content => content;

        protected virtual HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverClickSounds(sampleSet) { Enabled = { BindTarget = Enabled } };

        public OsuClickableContainer(HoverSampleSet sampleSet = HoverSampleSet.Default)
        {
            this.sampleSet = sampleSet;
        }

        public virtual LocalisableString TooltipText { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (AutoSizeAxes != Axes.None)
            {
                content.RelativeSizeAxes = (Axes.Both & ~AutoSizeAxes);
                content.AutoSizeAxes = AutoSizeAxes;
            }

            AddRangeInternal(new Drawable[]
            {
                CreateHoverSounds(sampleSet),
                content,
            });
        }

        protected override void ClearInternal(bool disposeChildren = true) =>
            throw new InvalidOperationException($"Clearing {nameof(InternalChildren)} will cause critical failure. Use {nameof(Clear)} instead.");
    }
}
