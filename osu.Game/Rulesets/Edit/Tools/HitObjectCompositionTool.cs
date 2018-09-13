// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Edit.Tools
{
    public abstract class HitObjectCompositionTool : ICompositionTool
    {
        public string Name { get; }

        protected HitObjectCompositionTool(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Visualises the <see cref="DrawableHitObject"/> which this tool would construct.
        /// </summary>
        /// <returns>The visualiser.</returns>
        public abstract PlacementVisualiser CreatePlacementVisualiser();
    }
}
