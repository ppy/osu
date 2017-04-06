// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Sample;
using osu.Framework.IO.Stores;

namespace osu.Game.Audio
{
    public class BeatmapSampleStore : NamespacedResourceStore<SampleChannel>
    {
        public BeatmapSampleStore(IResourceStore<SampleChannel> store, string ns)
            : base(store, ns)
        {
        }

        public SampleChannel Get(SampleInfo sampleInfo)
        {
            return Get($@"{sampleInfo.Bank}-{sampleInfo.Name}");
        }
    }
}
