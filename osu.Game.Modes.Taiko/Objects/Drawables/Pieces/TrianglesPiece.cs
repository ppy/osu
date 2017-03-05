// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        protected override float SpawnRatio => 1f;

        public TrianglesPiece()
        {
            TriangleScale = 2f;
        }

        protected override void Update()
        {
            if (IsPresent)
                base.Update();
        }
    }

}
