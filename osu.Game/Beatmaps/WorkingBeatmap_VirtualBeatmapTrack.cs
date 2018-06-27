// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Audio.Track;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Beatmaps
{
    public partial class WorkingBeatmap
    {
        private class VirtualBeatmapTrack : TrackVirtual
        {
            private readonly IBeatmap beatmap;

            public VirtualBeatmapTrack(IBeatmap beatmap)
            {
                this.beatmap = beatmap;
            }

            protected override void UpdateState()
            {
                updateVirtualLength();
                base.UpdateState();
            }

            private void updateVirtualLength()
            {
                var lastObject = beatmap.HitObjects.LastOrDefault();

                switch (lastObject)
                {
                    case null:
                        Length = 1000;
                        break;
                    case IHasEndTime endTime:
                        Length = endTime.EndTime + 1000;
                        break;
                    default:
                        Length = lastObject.StartTime + 1000;
                        break;
                }
            }
        }
    }
}
