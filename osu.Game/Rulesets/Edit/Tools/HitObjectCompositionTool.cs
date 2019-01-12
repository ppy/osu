// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Edit.Tools
{
    public abstract class HitObjectCompositionTool
    {
        public readonly string Name;

        protected HitObjectCompositionTool(string name)
        {
            Name = name;
        }

        public abstract PlacementBlueprint CreatePlacementBlueprint();
    }
}
