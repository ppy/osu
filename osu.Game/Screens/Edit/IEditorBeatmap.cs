// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit
{
    public interface IEditorBeatmap
    {
        event Action<HitObject> HitObjectAdded;
        event Action<HitObject> HitObjectRemoved;

        void Add(HitObject hitObject);
        void Remove(HitObject hitObject);
    }
}
