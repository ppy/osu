// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit
{
    internal class WaveformOpacityMenuItem : GenericChangeOpacityMenuItem
    {
        public WaveformOpacityMenuItem(Bindable<float> waveformOpacity)
            : base(waveformOpacity, EditorStrings.WaveformOpacity, [0f, 0.25f, 0.5f, 0.75f, 1f])
        {
        }
    }
}
