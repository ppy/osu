//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;

namespace osu.Framework.Configuration
{
    public interface IValueChangedObservable
    {
        event EventHandler ValueChanged;

        void UnbindAll();

        string Description { get; set; }
    }
}