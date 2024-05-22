// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class LabelledSliderBar<TNumber> : LabelledComponent<SettingsSlider<TNumber>, TNumber>
        where TNumber : struct, IEquatable<TNumber>, IComparable<TNumber>, IConvertible
    {
        public LabelledSliderBar()
            : base(true)
        {
        }

        protected override SettingsSlider<TNumber> CreateComponent() => new SettingsSlider<TNumber>
        {
            TransferValueOnCommit = true,
            RelativeSizeAxes = Axes.X,
        };
    }
}
