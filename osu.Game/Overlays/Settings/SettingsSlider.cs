// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public class SettingsSlider<T> : SettingsSlider<T, OsuSliderBar<T>>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
    }

    public class SettingsSlider<TValue, TSlider> : SettingsItem<TValue>
        where TValue : struct, IEquatable<TValue>, IComparable<TValue>, IConvertible
        where TSlider : OsuSliderBar<TValue>, new()
    {
        protected override Drawable CreateControl() => new TSlider
        {
            RelativeSizeAxes = Axes.X
        };

        /// <summary>
        /// When set, value changes based on user input are only transferred to any bound control's Current on commit.
        /// This is useful if the UI interaction could be adversely affected by the value changing, such as the position of the <see cref="SliderBar{T}"/> on the screen.
        /// </summary>
        public bool TransferValueOnCommit
        {
            get => ((TSlider)Control).TransferValueOnCommit;
            set => ((TSlider)Control).TransferValueOnCommit = value;
        }

        /// <summary>
        /// A custom step value for each key press which actuates a change on this control.
        /// </summary>
        public float KeyboardStep
        {
            get => ((TSlider)Control).KeyboardStep;
            set => ((TSlider)Control).KeyboardStep = value;
        }

        /// <summary>
        /// Whether to format the tooltip as a percentage or the actual value.
        /// </summary>
        public bool DisplayAsPercentage
        {
            get => ((TSlider)Control).DisplayAsPercentage;
            set => ((TSlider)Control).DisplayAsPercentage = value;
        }
    }
}
