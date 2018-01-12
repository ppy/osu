// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    public class SelectionInfo
    {
        /// <summary>
        /// The objects that are captured by the selection.
        /// </summary>
        public IEnumerable<DrawableHitObject> Objects;

        /// <summary>
        /// The screen space quad of the selection box surrounding <see cref="Objects"/>.
        /// </summary>
        public Quad SelectionQuad;
    }
}
