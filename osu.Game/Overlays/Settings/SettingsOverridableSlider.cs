// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Settings
{
    /// <summary>
    /// A control designed for interacting with <see cref="OverridableBindable{T}"/>.
    /// Consists of a checkbox to enable using custom value and a slider to change that custom value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SettingsOverridableSlider<T> : SettingsSlider<T>
        where T : struct, IConvertible, IComparable<T>, IEquatable<T>
    {
        private readonly OsuCheckbox checkbox;

        public override bool ShowsDefaultIndicator => false;

        public override string LabelText
        {
            get => checkbox.LabelText;
            set => checkbox.LabelText = value;
        }

        public new Bindable<T> Current
        {
            set => throw new InvalidOperationException("Must not change.");
        }

        public SettingsOverridableSlider(OverridableBindable<T> overridable)
        {
            FlowContent.Insert(-1, checkbox = new OsuCheckbox());

            checkbox.Current = overridable.HasCustomValue;
            base.Current = overridable.FinalValue;
        }
    }
}
