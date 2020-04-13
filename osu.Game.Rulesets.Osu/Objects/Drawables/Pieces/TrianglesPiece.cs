// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class TrianglesPiece : Triangles
    {
        protected override bool ExpireOffScreenTriangles => false;
        protected override bool CreateNewTriangles => false;
        protected override float SpawnRatio => 0.5f;

        public TrianglesPiece()
        {
            TriangleScale = 1.2f;
            HideAlphaDiscrepancies = false;
        }

        protected override void Update()
        {
            if (IsPresent)
                base.Update();
        }
    }
}
