using osu.Game.Graphics.Backgrounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    class DrumRollTrianglesPiece : Triangles
    {
        protected override float SpawnRatio => 1f;

        public DrumRollTrianglesPiece()
        {
            TriangleScale = 1.5f;
        }

        protected override void Update()
        {
            if (IsPresent)
                base.Update();
        }
    }
}
