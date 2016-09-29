using osu.Framework.Graphics.Containers;

namespace osu.Desktop.KeyCounterTutorial
{
    class KeyCounter : FlowContainer
    {
        public bool IsCounting
        {
            get { return isCounting; }
            set
            {
                isCounting = value;
                foreach (var child in Children)
                {
                    var counter = (Count)child;
                    counter.IsCounting = value;
                }
            }
        }

        private bool isCounting;

        public KeyCounter(bool isCounting = true)
        {
            IsCounting = isCounting;
            Direction = FlowDirection.HorizontalOnly;
        }

        public void AddKey(Count counter)
        {
            counter.IsCounting = IsCounting;
            Add(counter);
        }

        public void Reset()
        {
            foreach (var child in Children)
            {
                var counter = (Count)child;
                counter.Reset();
            }
        }
    }
}