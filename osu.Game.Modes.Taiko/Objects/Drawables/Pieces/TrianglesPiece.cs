using osu.Game.Graphics.Backgrounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    class TrianglesPiece : Triangles
    {
        protected override float SpawnRatio => 0.5f;

        public TrianglesPiece()
        {
        }

        protected override void Update()
        {
            if (IsPresent)
                base.Update();
        }
    }

}
