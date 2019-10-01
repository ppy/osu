// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A <see cref="Bindable{T}"/> for the <see cref="OsuGame"/> beatmap.
    /// This should be used sparingly in-favour of <see cref="IBindable{WorkingBeatmap}"/>.
    /// </summary>
    public abstract class BindableBeatmap : NonNullableBindable<WorkingBeatmap>
    {
        protected BindableBeatmap(WorkingBeatmap defaultValue)
            : base(defaultValue)
        {
        }
    }
}
