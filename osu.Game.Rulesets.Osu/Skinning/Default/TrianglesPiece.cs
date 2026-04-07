// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class TrianglesPiece : Triangles
    {
        protected override bool CreateNewTriangles => false;

        public TrianglesPiece(int? seed = null)
            : base(seed)
        {
            TriangleScale = 1.2f;
            SpawnRatio = 0.5f;
            HideAlphaDiscrepancies = false;
            ClampAxes = Axes.None;
        }

        protected override void Update()
        {
            if (IsPresent)
                base.Update();
        }
    }
}
