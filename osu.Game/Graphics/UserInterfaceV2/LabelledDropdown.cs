// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class LabelledDropdown<TItem> : LabelledComponent<OsuDropdown<TItem>, TItem>
    {
        public LabelledDropdown()
            : base(true)
        {
        }

        public IEnumerable<TItem> Items
        {
            get => Component.Items;
            set => Component.Items = value;
        }

        protected sealed override OsuDropdown<TItem> CreateComponent() => CreateDropdown().With(d =>
        {
            d.RelativeSizeAxes = Axes.X;
            d.Width = 0.5f;
        });

        protected virtual OsuDropdown<TItem> CreateDropdown() => new OsuDropdown<TItem>();
    }
}
