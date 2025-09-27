// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit
{
    internal class TimelineObjectsOpacityMenuItem : GenericChangeOpacityMenuItem
    {
        public TimelineObjectsOpacityMenuItem(Bindable<float> timelineObjectsOpacity)
            : base(timelineObjectsOpacity, EditorStrings.TimelineObjectsOpacity, [0.25f, 0.5f, 0.75f, 1f])
        {
        }
    }
}
