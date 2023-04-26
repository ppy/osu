// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.Sprites
{
    /// <summary>
    /// A wrapped version of <see cref="OsuSpriteText"/> which will expand in size based on text content, but never shrink back down.
    /// </summary>
    public partial class SizePreservingSpriteText : CompositeDrawable
    {
        private readonly OsuSpriteText text = new OsuSpriteText();

        private Vector2 maximumSize;

        public SizePreservingSpriteText(Vector2? minimumSize = null)
        {
            text.Origin = Anchor.Centre;
            text.Anchor = Anchor.Centre;

            AddInternal(text);
            maximumSize = minimumSize ?? Vector2.Zero;
        }

        protected override void Update()
        {
            Width = maximumSize.X = MathF.Max(maximumSize.X, text.Width);
            Height = maximumSize.Y = MathF.Max(maximumSize.Y, text.Height);
        }

        public new Axes AutoSizeAxes
        {
            get => Axes.None;
            set => throw new InvalidOperationException("You can't set AutoSizeAxes of this container");
        }

        /// <summary>
        /// Gets or sets the text to be displayed.
        /// </summary>
        public LocalisableString Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        /// <summary>
        /// Contains information on the font used to display the text.
        /// </summary>
        public FontUsage Font
        {
            get => text.Font;
            set => text.Font = value;
        }

        /// <summary>
        /// True if a shadow should be displayed around the text.
        /// </summary>
        public bool Shadow
        {
            get => text.Shadow;
            set => text.Shadow = value;
        }

        /// <summary>
        /// The colour of the shadow displayed around the text. A shadow will only be displayed if the <see cref="Shadow"/> property is set to true.
        /// </summary>
        public Color4 ShadowColour
        {
            get => text.ShadowColour;
            set => text.ShadowColour = value;
        }

        /// <summary>
        /// The offset of the shadow displayed around the text. A shadow will only be displayed if the <see cref="Shadow"/> property is set to true.
        /// </summary>
        public Vector2 ShadowOffset
        {
            get => text.ShadowOffset;
            set => text.ShadowOffset = value;
        }

        /// <summary>
        /// True if the <see cref="SpriteText"/>'s vertical size should be equal to <see cref="FontUsage.Size"/>  (the full height) or precisely the size of used characters.
        /// Set to false to allow better centering of individual characters/numerals/etc.
        /// </summary>
        public bool UseFullGlyphHeight
        {
            get => text.UseFullGlyphHeight;
            set => text.UseFullGlyphHeight = value;
        }

        public override bool IsPresent => text.IsPresent;

        public override string ToString() => text.ToString();

        public float LineBaseHeight => text.LineBaseHeight;

        public IEnumerable<LocalisableString> FilterTerms => text.FilterTerms;
    }
}
