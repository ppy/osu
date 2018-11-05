// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for a <see cref="Mod"/> that applies changes to a <see cref="Beatmap"/>
    /// after conversion and post-processing has completed.
    /// </summary>
    public interface IApplicableToBeatmap<TObject> : IApplicableMod
        where TObject : HitObject
    {
        /// <summary>
        /// Applies this <see cref="IApplicableToBeatmap{TObject}"/> to a <see cref="Beatmap{TObject}"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="Beatmap{TObject}"/> to apply to.</param>
        void ApplyToBeatmap(Beatmap<TObject> beatmap);
    }
}
