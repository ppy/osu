// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
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
            Margin = new MarginPadding { Top = 5, Bottom = 5 },
            RelativeSizeAxes = Axes.X
        };

        public bool TransferValueOnCommit
        {
            get => ((TSlider)Control).TransferValueOnCommit;
            set => ((TSlider)Control).TransferValueOnCommit = value;
        }

        public float KeyboardStep
        {
            get => ((TSlider)Control).KeyboardStep;
            set => ((TSlider)Control).KeyboardStep = value;
        }

        public bool DisplayAsPercentage
        {
            get => ((TSlider)Control).DisplayAsPercentage;
            set => ((TSlider)Control).DisplayAsPercentage = value;
        }
    }
}
