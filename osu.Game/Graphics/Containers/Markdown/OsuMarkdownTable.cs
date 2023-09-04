// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Extensions.Tables;
using osu.Framework.Graphics.Containers.Markdown;

namespace osu.Game.Graphics.Containers.Markdown
{
    public partial class OsuMarkdownTable : MarkdownTable
    {
        public OsuMarkdownTable(Table table)
            : base(table)
        {
        }

        protected override MarkdownTableCell CreateTableCell(TableCell cell, TableColumnDefinition definition, bool isHeading) => new OsuMarkdownTableCell(cell, definition, isHeading);
    }
}
