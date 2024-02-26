// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModClassic : Mod
    {
        public override string Name => "Classic";

        public override string Acronym => "CL";

        public override double ScoreMultiplier => 0.96;

        public override IconUsage? Icon => FontAwesome.Solid.History;

        public override LocalisableString Description => "Feeling nostalgic?";

        public override ModType Type => ModType.Conversion;

        /// <summary>
        /// Classic mods are not to be ranked yet due to compatibility and multiplier concerns.
        /// Right now classic mods are considered, for leaderboard purposes, to be equal as scores set on osu-stable.
        /// But this is not the case.
        ///
        /// Some examples for things to resolve before even considering this:
        ///  - Hit windows differ (https://github.com/ppy/osu/issues/11311).
        ///  - Sliders always gives combo for slider end, even on miss (https://github.com/ppy/osu/issues/11769).
        /// </summary>
        public sealed override bool Ranked => false;
    }
}
