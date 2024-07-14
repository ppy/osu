// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    public partial class ExpandingToolboxContainer : ExpandingContainer
    {
        protected override double HoverExpansionDelay => 250;

        public ExpandingToolboxContainer(float contractedWidth, float expandedWidth)
            : base(contractedWidth, expandedWidth)
        {
            RelativeSizeAxes = Axes.Y;

            FillFlow.Spacing = new Vector2(5);
            Padding = new MarginPadding { Vertical = 5 };
        }

        protected override bool ReceivePositionalInputAtSubTree(Vector2 screenSpacePos) => base.ReceivePositionalInputAtSubTree(screenSpacePos) && anyToolboxHovered(screenSpacePos);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => base.ReceivePositionalInputAt(screenSpacePos) && anyToolboxHovered(screenSpacePos);

        private bool anyToolboxHovered(Vector2 screenSpacePos) => FillFlow.ScreenSpaceDrawQuad.Contains(screenSpacePos);

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnClick(ClickEvent e) => true;
    }
}
