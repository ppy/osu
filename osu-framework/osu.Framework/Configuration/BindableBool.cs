//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;

namespace osu.Framework.Configuration
{
    public class BindableBool : Bindable<bool>
    {
        public BindableBool(bool value = false) : base(value)
        {
        }

        public static implicit operator bool(BindableBool value)
        {
            return value == null ? false : value.Value;
        }

        public override string ToString()
        {
            return Value ? @"true" : @"false";
        }

        public override bool Parse(object s)
        {
            string str = s as string;
            Value = str == @"1" || str == @"true";
            return true;
        }

        public void Toggle()
        {
            Value = !Value;
        }
    }
}