//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using osu.Framework.Audio.Sample;
using osu.Framework.Resources;

namespace osu.Framework.Audio.Track
{
    public class TrackManager : AudioCollectionManager<AudioTrack>
    {
        IResourceStore<byte[]> store;

        public TrackManager(IResourceStore<byte[]> store)
        {
            this.store = store;
        }

        public AudioTrack GetTrack(string name)
        {
            AudioTrackBass track = new AudioTrackBass(store.GetStream(name));
            AddItem(track);
            return track;
        }
    }
}
