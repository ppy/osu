//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Globalization;

namespace osu.Framework.Configuration
{
    public class BindableDouble : Bindable<double>
    {
        internal double MinValue = double.MinValue;
        internal double MaxValue = double.MaxValue;

        public override double Value
        {
            get { return base.Value; }
            set
            {
                double boundValue = value;

                if (boundValue > MaxValue)
                    boundValue = MaxValue;
                else if (boundValue < MinValue)
                    boundValue = MinValue;

                if (Precision > double.Epsilon)
                    boundValue = Math.Round(boundValue / Precision) * Precision;

                base.Value = boundValue;
            }
        }

        public BindableDouble(double value = 0) : base(value)
        {
        }

        public static implicit operator double(BindableDouble value)
        {
            return value == null ? 0 : value.Value;
        }

        public override string ToString()
        {
            return Value.ToString("0.0###", NumberFormatInfo.InvariantInfo);
        }

        public override bool Parse(object s)
        {
            Value = double.Parse(s as string, NumberFormatInfo.InvariantInfo);
            return true;
        }

        public double Precision = double.Epsilon;

        public override bool IsDefault => Math.Abs(Value - Default) < Precision;
    }
}