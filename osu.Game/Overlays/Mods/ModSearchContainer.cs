// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Mods
{
    public partial class ModSearchContainer : SearchContainer
    {
        public new string SearchTerm
        {
            get => base.SearchTerm;
            set
            {
                if (value == SearchTerm)
                    return;

                base.SearchTerm = value;

                // Manual filtering here is required because ModColumn can be hidden when search term applied,
                // causing the whole SearchContainer to become non-present and never actually perform a subsequent
                // filter.
                Filter();
            }
        }
    }
}
