// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mods
{
    public class OverridableBindable<T> : IParseable
        where T : struct, IComparable<T>, IConvertible, IEquatable<T>
    {
        public Bindable<T> BaseValue => baseValue;
        public Bindable<T> CustomValue => customValue;
        public Bindable<T> FinalValue => finalValue;

        public Bindable<bool> HasCustomValue { get; } = new BindableBool();

        private readonly BindableNumber<T> immutableBaseValue = new BindableNumber<T>();
        private readonly BindableNumber<T> customValue = new BindableNumber<T>();
        private readonly BindableNumber<T> finalValue = new BindableNumber<T>();

        private readonly LeasedBindable<T> baseValue;

        public OverridableBindable(T? defaultValue = null, T? minValue = null, T? maxValue = null, T? precision = null)
        {
            if (defaultValue != null)
            {
                immutableBaseValue.Value = customValue.Value = finalValue.Value = defaultValue.Value;
                immutableBaseValue.Default = customValue.Default = finalValue.Default = defaultValue.Value;
            }

            if (minValue != null)
                immutableBaseValue.MinValue = customValue.MinValue = finalValue.MinValue = minValue.Value;

            if (maxValue != null)
                immutableBaseValue.MaxValue = customValue.MaxValue = finalValue.MaxValue = maxValue.Value;

            if (precision != null)
                immutableBaseValue.Precision = customValue.Precision = finalValue.Precision = precision.Value;

            // this lease is never returned on purpose.
            // the leased bindable is exposed publicly to be mutable from outside,
            // while the inner one is immutable, so that updateFinalValue() can bind to it if HasCustomValue = false,
            // therefore ensuring that FinalValue is disabled.
            baseValue = immutableBaseValue.BeginLease(false);

            HasCustomValue.BindValueChanged(_ => updateFinalValue(), true);
        }

        public void Parse(object input)
        {
            switch (input)
            {
                case null:
                    HasCustomValue.Value = false;
                    return;

                case OverridableBindable<T> setting:
                    if (setting.HasCustomValue.Value)
                    {
                        CustomValue.Value = setting.CustomValue.Value;
                        HasCustomValue.Value = true;
                    }
                    else
                        HasCustomValue.Value = false;

                    break;

                default:
                    CustomValue.Parse(input);
                    HasCustomValue.Value = true;
                    break;
            }
        }

        private void updateFinalValue()
        {
            if (HasCustomValue.Value)
            {
                finalValue.UnbindFrom(immutableBaseValue);
                // manually re-enable before proceeding; see https://github.com/ppy/osu-framework/issues/3218
                finalValue.Disabled = false;
                finalValue.BindTo(customValue);
            }
            else
            {
                finalValue.UnbindFrom(customValue);
                finalValue.BindTo(immutableBaseValue);
            }
        }
    }
}
