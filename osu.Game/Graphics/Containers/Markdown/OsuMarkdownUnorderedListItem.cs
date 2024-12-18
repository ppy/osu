// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.Containers.Markdown
{
    public partial class OsuMarkdownUnorderedListItem : OsuMarkdownListItem
    {
        private const float left_padding = 20;

        private readonly int level;

        public OsuMarkdownUnorderedListItem(int level)
        {
            this.level = level;

            Padding = new MarginPadding { Left = left_padding };
        }

        protected override SpriteText CreateMarker() => base.CreateMarker().With(t =>
        {
            t.Text = GetTextMarker(level);
            t.Font = t.Font.With(size: t.Font.Size / 2);
            t.Origin = Anchor.Centre;
            t.X = -left_padding / 2;
            t.Y = t.Font.Size;
        });

        /// <summary>
        /// Get text marker based on <paramref name="level"/>
        /// </summary>
        /// <param name="level">The markdown level of current list item.</param>
        /// <returns>The marker string of this list item</returns>
        protected virtual string GetTextMarker(int level)
        {
            switch (level)
            {
                case 1:
                    return "●";

                case 2:
                    return "○";

                default:
                    return "■";
            }
        }
    }
}
