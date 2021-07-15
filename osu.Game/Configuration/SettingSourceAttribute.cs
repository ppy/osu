// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Configuration
{
    /// <summary>
    /// An attribute to mark a bindable as being exposed to the user via settings controls.
    /// Can be used in conjunction with <see cref="SettingSourceExtensions.CreateSettingsControls"/> to automatically create UI controls.
    /// </summary>
    /// <remarks>
    /// All controls with <see cref="OrderPosition"/> set will be placed first in ascending order.
    /// All controls with no <see cref="OrderPosition"/> will come afterward in default order.
    /// </remarks>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingSourceAttribute : Attribute, IComparable<SettingSourceAttribute>
    {
        public LocalisableString Label { get; }

        public LocalisableString Description { get; }

        public int? OrderPosition { get; }

        /// <summary>
        /// The type of the settings control which handles this setting source.
        /// </summary>
        /// <remarks>
        /// Must be a type deriving <see cref="SettingsItem{T}"/> with a public parameterless constructor.
        /// </remarks>
        public Type? SettingControlType { get; set; }

        public SettingSourceAttribute(string? label, string? description = null)
        {
            Label = label ?? string.Empty;
            Description = description ?? string.Empty;
        }

        public SettingSourceAttribute(string label, string description, int orderPosition)
            : this(label, description)
        {
            OrderPosition = orderPosition;
        }

        public int CompareTo(SettingSourceAttribute other)
        {
            if (OrderPosition == other.OrderPosition)
                return 0;

            // unordered items come last (are greater than any ordered items).
            if (OrderPosition == null)
                return 1;
            if (other.OrderPosition == null)
                return -1;

            // ordered items are sorted by the order value.
            return OrderPosition.Value.CompareTo(other.OrderPosition);
        }
    }

    public static class SettingSourceExtensions
    {
        public static IEnumerable<Drawable> CreateSettingsControls(this object obj)
        {
            foreach (var (attr, property) in obj.GetOrderedSettingsSourceProperties())
            {
                object value = property.GetValue(obj);

                if (attr.SettingControlType != null)
                {
                    var controlType = attr.SettingControlType;
                    if (controlType.EnumerateBaseTypes().All(t => !t.IsGenericType || t.GetGenericTypeDefinition() != typeof(SettingsItem<>)))
                        throw new InvalidOperationException($"{nameof(SettingSourceAttribute)} had an unsupported custom control type ({controlType.ReadableName()})");

                    var control = (Drawable)Activator.CreateInstance(controlType);
                    controlType.GetProperty(nameof(SettingsItem<object>.LabelText))?.SetValue(control, attr.Label);
                    controlType.GetProperty(nameof(SettingsItem<object>.TooltipText))?.SetValue(control, attr.Description);
                    controlType.GetProperty(nameof(SettingsItem<object>.Current))?.SetValue(control, value);

                    yield return control;

                    continue;
                }

                switch (value)
                {
                    case BindableNumber<float> bNumber:
                        yield return new SettingsSlider<float>
                        {
                            LabelText = attr.Label,
                            TooltipText = attr.Description,
                            Current = bNumber,
                            KeyboardStep = 0.1f,
                        };

                        break;

                    case BindableNumber<double> bNumber:
                        yield return new SettingsSlider<double>
                        {
                            LabelText = attr.Label,
                            TooltipText = attr.Description,
                            Current = bNumber,
                            KeyboardStep = 0.1f,
                        };

                        break;

                    case BindableNumber<int> bNumber:
                        yield return new SettingsSlider<int>
                        {
                            LabelText = attr.Label,
                            TooltipText = attr.Description,
                            Current = bNumber
                        };

                        break;

                    case Bindable<bool> bBool:
                        yield return new SettingsCheckbox
                        {
                            LabelText = attr.Label,
                            TooltipText = attr.Description,
                            Current = bBool
                        };

                        break;

                    case Bindable<string> bString:
                        yield return new SettingsTextBox
                        {
                            LabelText = attr.Label,
                            TooltipText = attr.Description,
                            Current = bString
                        };

                        break;

                    case IBindable bindable:
                        var dropdownType = typeof(ModSettingsEnumDropdown<>).MakeGenericType(bindable.GetType().GetGenericArguments()[0]);
                        var dropdown = (Drawable)Activator.CreateInstance(dropdownType);

                        dropdownType.GetProperty(nameof(SettingsDropdown<object>.LabelText))?.SetValue(dropdown, attr.Label);
                        dropdownType.GetProperty(nameof(SettingsDropdown<object>.TooltipText))?.SetValue(dropdown, attr.Description);
                        dropdownType.GetProperty(nameof(SettingsDropdown<object>.Current))?.SetValue(dropdown, bindable);

                        yield return dropdown;

                        break;

                    default:
                        throw new InvalidOperationException($"{nameof(SettingSourceAttribute)} was attached to an unsupported type ({value})");
                }
            }
        }

        public static IEnumerable<(SettingSourceAttribute, PropertyInfo)> GetSettingsSourceProperties(this object obj)
        {
            foreach (var property in obj.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
            {
                var attr = property.GetCustomAttribute<SettingSourceAttribute>(true);

                if (attr == null)
                    continue;

                yield return (attr, property);
            }
        }

        public static ICollection<(SettingSourceAttribute, PropertyInfo)> GetOrderedSettingsSourceProperties(this object obj)
            => obj.GetSettingsSourceProperties()
                  .OrderBy(attr => attr.Item1)
                  .ToArray();

        private class ModSettingsEnumDropdown<T> : SettingsEnumDropdown<T>
            where T : struct, Enum
        {
            protected override OsuDropdown<T> CreateDropdown() => new ModDropdownControl();

            private class ModDropdownControl : DropdownControl
            {
                // Set menu's max height low enough to workaround nested scroll issues (see https://github.com/ppy/osu-framework/issues/4536).
                protected override DropdownMenu CreateMenu() => base.CreateMenu().With(m => m.MaxHeight = 100);
            }
        }
    }
}
