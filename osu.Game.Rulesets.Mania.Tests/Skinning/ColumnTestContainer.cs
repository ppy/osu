// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    /// <summary>
    /// A container to be used in a <see cref="ManiaSkinnableTestScene"/> to provide a resolvable <see cref="Column"/> dependency.
    /// </summary>
    public class ColumnTestContainer : Container
    {
        protected override Container<Drawable> Content => content;

        private readonly Container content;

        [Cached]
        private readonly Column column;

        public ColumnTestContainer(int column, ManiaAction action, bool showColumn = false)
        {
            InternalChildren = new[]
            {
                this.column = new Column(column)
                {
                    Action = { Value = action },
                    AccentColour = Color4.Orange,
                    ColumnType = column % 2 == 0 ? ColumnType.Even : ColumnType.Odd,
                    Alpha = showColumn ? 1 : 0
                },
                content = new ManiaInputManager(new ManiaRuleset().RulesetInfo, 4)
                {
                    RelativeSizeAxes = Axes.Both
                },
                this.column.TopLevelContainer.CreateProxy()
            };
        }
    }
}
