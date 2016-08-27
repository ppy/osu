//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;

namespace osu.Framework.Configuration
{
    public class Bindable<T> : IBindable
        where T : IComparable
    {
        private T value;

        public T Default;

        public virtual bool IsDefault => Equals(value, Default);

        public event EventHandler ValueChanged;

        public virtual T Value
        {
            get { return value; }
            set
            {
                if (this.value?.CompareTo(value) == 0) return;

                this.value = value;

                TriggerChange();
            }
        }

        public Bindable(T value)
        {
            Value = value;
        }

        public static implicit operator T(Bindable<T> value)
        {
            return value.Value;
        }

        public object ObjectValue
        {
            get
            {
                return Value;
            }
            set
            {
                try
                {
                    Value = (T)value;
                }
                catch { }
            }
        }

        public virtual bool Parse(object s)
        {
            if (s is T)
                Value = (T)s;
            else if (typeof(T).IsEnum)
                Value = (T)Enum.Parse(typeof(T), s as string);
            else
                return false;

            return true;
        }

        internal void TriggerChange()
        {
            if (ValueChanged != null) ValueChanged(this, null);
        }

        public void UnbindAll()
        {
            ValueChanged = null;
        }

        string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public override string ToString()
        {
            return value.ToString();
        }

        internal void Reset()
        {
            Value = Default;
        }
    }
}
