//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Configuration;

namespace osu.Framework.Audio
{
    /// <summary>
    /// A collection of audio components which need central property control.
    /// </summary>
    public class AudioCollectionManager<T> : AdjustableAudioComponent
        where T : AdjustableAudioComponent
    {
        List<T> ActiveItems = new List<T>();

        protected void AddItem(T item)
        {
            item.AddAdjustmentDependency(this);
            ActiveItems.Add(item);
        }

        public override void Update()
        {
            base.Update();

            ActiveItems.ForEach(s => s.Update());

            ActiveItems.FindAll(s => (s as IHasCompletedState)?.HasCompleted ?? false).ForEach(s =>
            {
                s.Dispose();
                ActiveItems.Remove(s);
            });
        }
    }
}
