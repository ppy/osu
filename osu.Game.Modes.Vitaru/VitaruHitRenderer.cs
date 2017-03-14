using System;
using System.Collections.Generic;
using OpenTK;
using osu.Game.Modes.Objects;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Vitaru
{
    internal class VitaruHitRenderer : HitRenderer
    {
        public override bool AllObjectsJudged
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<HitObject> Enemy { get; set; }

        public override Func<Vector2, Vector2> MapPlayfieldToScreenSpace
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}