// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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
            var configProperties = obj.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttribute<SettingSourceAttribute>(true) != null);

            foreach (var property in configProperties)
            {
                var attr = property.GetCustomAttribute<SettingSourceAttribute>(true);

                switch (property.GetValue(obj))
                {
                    case BindableNumber<float> bNumber:
                        yield return new SettingsSlider<float>
                        {
                            LabelText = attr.Label,
                            Bindable = bNumber
                        };

                        break;

                    case BindableNumber<double> bNumber:
                        yield return new SettingsSlider<double>
                        {
                            LabelText = attr.Label,
                            Bindable = bNumber
                        };

                        break;

                    case BindableNumber<int> bNumber:
                        yield return new SettingsSlider<int>
                        {
                            LabelText = attr.Label,
                            Bindable = bNumber
                        };

                        break;

                    case Bindable<bool> bBool:
                        yield return new SettingsCheckbox
                        {
                            LabelText = attr.Label,
                            Bindable = bBool
                        };

                        break;
                }
            }
        }
    }
}
