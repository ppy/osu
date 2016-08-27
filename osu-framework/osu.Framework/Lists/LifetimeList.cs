//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace osu.Framework.Lists
{
    class LifetimeList<T> : SortedList<T> where T : IHasLifetime
    {
        public delegate void ElementChangedHandler(T element);

        private List<bool> lifeStatus = new List<bool>();

        private double lastTime;

        public LifetimeList(IComparer<T> comparer)
            : base(comparer)
        {
        }

        public IEnumerable<T> Current
        {
            get
            {
                for (int i = 0; i < base.Count; i++)
                {
                    if (lifeStatus[i])
                        yield return base[i];
                }
            }
        }

        public void Update(double time)
        {
            for (int i = 0; i < this.Count; i++)
            {
                bool isAlive = this[i].IsAlive;

                if (lifeStatus[i] == isAlive)
                    continue;

                bool removed = false;

                if (lifeStatus[i] && !isAlive)
                {
                    if (this[i].RemoveWhenNotAlive)
                    {
                        RemoveAt(i--);
                        removed = true;
                    }
                }
                else if (this[i].LoadRequired)
                    this[i].Load();

                if (!removed)
                    lifeStatus[i] = isAlive;
            }

            lastTime = time;
        }

        public override int Add(T item)
        {
            int index = base.Add(item);
            lifeStatus.Insert(index, item.IsAlive);

            if (item.LoadRequired)
                item.Load();

            return index;
        }

        public override void Clear()
        {
            base.Clear();
            lifeStatus.Clear();
        }

        public override bool Remove(T item)
        {
            int index = base.IndexOf(item);
            if (index < 0)
                return false;

            base.RemoveAt(index);
            lifeStatus.RemoveAt(index);

            return true;
        }

        public override void RemoveAt(int index)
        {
            base.RemoveAt(index);
            lifeStatus.RemoveAt(index);
        }
    }
}
