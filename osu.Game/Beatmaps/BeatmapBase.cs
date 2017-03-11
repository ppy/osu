// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using System.Collections.Generic;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Contains basic beatmap properties.
    /// <para>
    /// This allows converted beatmaps (<see cref="Beatmap{T}"/>) to refer to an original "base" Beatmap
    /// for properties that shouldn't change unless in exceptional circumstances.
    /// Properties here may be set to be overriden in such exceptional cases.
    /// </para>
    /// </summary>
    public class BeatmapBase
    {
        private BeatmapBase original;

        public BeatmapBase(BeatmapBase original = null)
        {
            this.original = original;
        }

        private BeatmapInfo beatmapInfo;
        public BeatmapInfo BeatmapInfo
        {
            get { return beatmapInfo ?? original?.BeatmapInfo; }
            set { beatmapInfo = value; }
        }

        private List<ControlPoint> controlPoints;
        public List<ControlPoint> ControlPoints
        {
            get { return controlPoints ?? original?.ControlPoints; }
            set { controlPoints = value; }
        }

        private List<Color4> comboColors;
        public List<Color4> ComboColors
        {
            get { return comboColors ?? original?.ComboColors; }
            set { comboColors = value; }
        }

        public BeatmapMetadata Metadata => BeatmapInfo?.Metadata ?? BeatmapInfo?.BeatmapSet?.Metadata;
    }
}
