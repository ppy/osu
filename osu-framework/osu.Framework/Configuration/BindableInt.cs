//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Globalization;

namespace osu.Framework.Configuration
{
    public class BindableInt : Bindable<int>
    {
        internal int MinValue = int.MinValue;
        internal int MaxValue = int.MaxValue;

        public override int Value
        {
            get { return base.Value; }
            set
            {
                int boundValue = value;

                if (boundValue > MaxValue)
                    boundValue = MaxValue;
                else if (boundValue < MinValue)
                    boundValue = MinValue;

                base.Value = boundValue;
            }
        }

        public BindableInt(int value = 0) : base(value)
        {
        }

        public static implicit operator int(BindableInt value)
        {
            return value == null ? 0 : value.Value;
        }

        public override bool Parse(object s)
        {
            Value = int.Parse(s as string, NumberFormatInfo.InvariantInfo);
            return true;
        }

        public override string ToString()
        {
            return Value.ToString(NumberFormatInfo.InvariantInfo);
        }
    }
}