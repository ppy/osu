//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Resources;

namespace osu.Framework.Resources
{
    public class ChangeableResourceStore<T> : ResourceStore<T>
    {
        public event Action<string> OnChanged;

        protected void TriggerOnChanged(string name)
        {
            OnChanged?.Invoke(name);
        }
    }
}
