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

    public class SettingsSlider<T, U> : SettingsItem<T>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
        where U : OsuSliderBar<T>, new()
    {
        protected override Drawable CreateControl() => new U
        {
            Margin = new MarginPadding { Top = 5, Bottom = 5 },
            RelativeSizeAxes = Axes.X
        };

        public bool TransferValueOnCommit
        {
            get => ((U)Control).TransferValueOnCommit;
            set => ((U)Control).TransferValueOnCommit = value;
        }

        public float KeyboardStep
        {
            get => ((U)Control).KeyboardStep;
            set => ((U)Control).KeyboardStep = value;
        }
    }
}
