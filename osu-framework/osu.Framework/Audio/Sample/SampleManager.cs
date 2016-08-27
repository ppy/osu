//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Resources;

namespace osu.Framework.Audio.Sample
{
    public class SampleManager : AudioCollectionManager<AudioSample>
    {
        IResourceStore<byte[]> store;

        public SampleManager(IResourceStore<byte[]> store)
        {
            this.store = store;
        }

        public AudioSample GetSample(string name)
        {
            byte[] data = store.Get(name);

            AudioSample sample = new AudioSampleBass(data);
            AddItem(sample);
            return sample;
        }
    }
}
