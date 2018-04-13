﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class ScoreModsContainer : FlowContainer<ModIcon>
    {
        protected override IEnumerable<Vector2> ComputeLayoutPositions()
        {
            int count = FlowingChildren.Count();
            for (int i = 0; i < count; i++)
                yield return new Vector2(DrawWidth * i * (count == 1 ? 0 : 1f / (count - 1)), 0);
        }
    }
}
