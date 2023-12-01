// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace osu.Game.Graphics.Sprites
{
    /// <summary>
    /// A derived version of <see cref="OsuSpriteText"/> which automatically shows non-truncated text in tooltip when required.
    /// </summary>
    public sealed partial class TruncatingSpriteText : OsuSpriteText, IHasTooltip
    {
        /// <summary>
        /// Whether a tooltip should be shown with non-truncated text on hover.
        /// </summary>
        public bool ShowTooltip { get; init; } = true;

        public LocalisableString TooltipText => Text;

        public override bool HandlePositionalInput => IsTruncated && ShowTooltip;

        private CompositeDrawable firstNonAutoSizedWidthAncestor = null!;

        public TruncatingSpriteText()
        {
            ((SpriteText)this).Truncate = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            firstNonAutoSizedWidthAncestor = getFirstNonAutoSizedWidthAncestor();
        }

        private CompositeDrawable getFirstNonAutoSizedWidthAncestor()
        {
            var ancestor = Parent!;

            while (ancestor.AutoSizeAxes != Axes.None && ancestor.AutoSizeAxes != Axes.Y)
                ancestor = ancestor.Parent!;

            return ancestor;
        }

        protected override void Update()
        {
            base.Update();

            // Confine the auto-sized text to the child width of the first non-autosized width ancestor drawable.
            // Subtract by X to account for custom or automated positioning (e.g. inside a FillFlowContainer).
            base.MaxWidth = firstNonAutoSizedWidthAncestor.ChildSize.X - X;
        }

        [Obsolete("Width is automatically handled by Update().")]

        public new float MaxWidth
        {
            set => throw new InvalidOperationException($@"Width is automatically handled by {nameof(Update)}.");
        }

        [Obsolete("Width is automatically handled by Update().")]
        public new float Width
        {
            set => throw new InvalidOperationException($@"Width is automatically handled by {nameof(Update)}.");
        }

        [Obsolete("Width is automatically handled by Update().")]
        public new Axes RelativeSizeAxes
        {
            set => throw new InvalidOperationException($@"Width is automatically handled by {nameof(Update)}.");
        }
    }
}
