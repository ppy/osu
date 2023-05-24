// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public partial class VertexPiece : Circle
    {
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
            Colour = state.IsSelected ? osuColour.Yellow.Lighten(1) : osuColour.Yellow;
            Alpha = state.IsFixed ? 0.5f : 1;
        }
    }
}
