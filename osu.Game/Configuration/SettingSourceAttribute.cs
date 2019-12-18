// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Configuration
{
    /// <summary>
    /// An attribute to mark a bindable as being exposed to the user via settings controls.
    /// Can be used in conjunction with <see cref="SettingSourceExtensions.CreateSettingsControls"/> to automatically create UI controls.
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingSourceAttribute : Attribute
    {
        public string Label { get; }

        public string Description { get; }

        public SettingSourceAttribute(string label, string description = null)
        {
            Label = label ?? string.Empty;
            Description = description ?? string.Empty;
        }
    }

    public static class SettingSourceExtensions
    {
        public static IEnumerable<Drawable> CreateSettingsControls(this object obj)
        {
            Drawable createFromNonGeneric(IBindable bindable)
            {
                var dropdownType = typeof(SettingsEnumDropdown<>).MakeGenericType(bindable.GetType().GetGenericArguments()[0]);
                var dropdown = (Drawable)Activator.CreateInstance(dropdownType);

                dropdown.GetType().GetProperty(nameof(IHasCurrentValue<object>.Current))?.SetValue(dropdown, obj);

                return dropdown;
            }

            foreach (var property in obj.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
            {
                var attr = property.GetCustomAttribute<SettingSourceAttribute>(true);

                if (attr == null)
                    continue;

                yield return property.GetValue(obj) switch
                {
                    BindableNumber<float> bNumber => new SettingsSlider<float>
                    {
                        LabelText = attr.Label,
                        Bindable = bNumber
                    },
                    BindableNumber<double> bNumber => new SettingsSlider<double>
                    {
                        LabelText = attr.Label,
                        Bindable = bNumber
                    },
                    BindableNumber<int> bNumber => new SettingsSlider<int>
                    {
                        LabelText = attr.Label,
                        Bindable = bNumber
                    },
                    Bindable<bool> bBool => new SettingsCheckbox
                    {
                        LabelText = attr.Label,
                        Bindable = bBool
                    },
                    Bindable<string> bString => new SettingsTextBox
                    {
                        LabelText = attr.Label,
                        Bindable = bString
                    },
                    IBindable bindable => createFromNonGeneric(bindable),
                    var other => throw new InvalidOperationException($"{nameof(SettingSourceAttribute)} was attached to an unsupported type ({other})"),
                };
            }
        }
    }
}
