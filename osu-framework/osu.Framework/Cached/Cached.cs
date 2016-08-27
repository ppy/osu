//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using osu.Framework.Timing;

namespace osu.Framework.Cached
{
    public class Cached<T>
    {
        public delegate T PropertyUpdater<T>();

        /// <summary>
        /// How often this property is refreshed.
        /// </summary>
        public readonly int RefreshInterval;

        /// <summary>
        /// Whether we allow reads of stale values. If this is set to false, there may be a potential blocking delay when accessing the property.
        /// </summary>
        public bool AllowStaleReads = true;

        private bool isStale => lastUpdateTime < 0 || (RefreshInterval >= 0 && clock?.CurrentTime > lastUpdateTime + RefreshInterval);
        public bool IsValid => !isStale;

        private PropertyUpdater<T> updateDelegate;
        private readonly IClock clock;
        private double lastUpdateTime = -1;

        /// <summary>
        /// Create a new cached property.
        /// </summary>
        /// <param name="updateDelegate">The delegate method which will perform future updates to this property.</param>
        /// <param name="refreshInterval">How often we should refresh this property. Set to -1 to never update. Set to 0 for once per frame.</param>
        public Cached(PropertyUpdater<T> updateDelegate = null, IClock clock = null, int refreshInterval = -1)
        {
            RefreshInterval = refreshInterval;
            this.updateDelegate = updateDelegate;
            this.clock = clock;
        }

        public static implicit operator T(Cached<T> value)
        {
            return value.Value;
        }

        /// <summary>
        /// Refresh this cached object with a custom delegate.
        /// </summary>
        /// <param name="providedDelegate"></param>
        public T Refresh(PropertyUpdater<T> providedDelegate)
        {
            if (isStale)
            {
                updateDelegate = updateDelegate ?? providedDelegate;
                Refresh();
            }

            return value;
        }

        /// <summary>
        /// Refresh this property.
        /// </summary>
        public void Refresh()
        {
            if (updateDelegate == null)
                throw new Exception("No value cached and no update delegate prepared!");

            value = updateDelegate();
            lastUpdateTime = clock?.CurrentTime ?? 0;
        }

        public bool Invalidate()
        {
            if (lastUpdateTime < 0) return false;

            lastUpdateTime = -1;
            return true;
        }

        private T value;
        public T Value
        {
            get
            {
                if (isStale)
                    Refresh();

                return value;
            }

            set
            {
                throw new Exception("Can't manually update value!");
            }
        }
    }
}
