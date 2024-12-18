// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public partial class VertexPiece : Circle
    {
        private VertexState state = new VertexState();

        [Resolved]
        private OsuColour osuColour { get; set; } = null!;

        public VertexPiece()
        {
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.Centre;
            Size = new Vector2(15);
        }

        public void UpdateFrom(VertexState state)
        {
            this.state = state;
            updateMarkerDisplay();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateMarkerDisplay();
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateMarkerDisplay();
        }

        /// <summary>
        /// Updates the state of the circular control point marker.
        /// </summary>
        private void updateMarkerDisplay()
        {
            var colour = osuColour.Yellow;

            if (IsHovered || state.IsSelected)
                colour = colour.Lighten(1);

            Colour = colour;
            Alpha = state.IsFixed ? 0.5f : 1;
        }
    }
}
