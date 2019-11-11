// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class LabelledDropdown<T> : LabelledComponent<OsuDropdown<T>, T>
    {
        public LabelledDropdown()
            : base(true)
        {
        }

        public IEnumerable<T> Items
        {
            get => Component.Items;
            set => Component.Items = value;
        }

        protected override OsuDropdown<T> CreateComponent() => new OsuDropdown<T>
        {
            RelativeSizeAxes = Axes.X,
            Width = 0.5f,
        };
    }
}
