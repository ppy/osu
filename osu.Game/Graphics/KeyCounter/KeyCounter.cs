using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.KeyCounter
{
    /// <summary>
    /// Class that contains a series of keyboard/mouse button press counters
    /// Counters can be added using the AddKey method
    /// </summary>
    class KeyCounter : Drawable
    {
        private FlowContainer counterContainer;
        private List<Count> counterList;

        private bool isCounting = true;
        public bool IsCounting
        {
            get { return isCounting; }
            set
            {
                isCounting = value;

                foreach (Count counter in counterList)
                {
                    counter.isCounting = value;
                }
            }
        }

        public override void Load()
        {
            base.Load();

            counterList = new List<Count>();

            counterContainer = new FlowContainer
            {
                Direction = FlowDirection.HorizontalOnly,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };

            Add(counterContainer);
        }

        internal void AddKey(Count counter)
        {
            counterContainer.Add(counter);
            counterList.Add(counter);
        }
    }
}