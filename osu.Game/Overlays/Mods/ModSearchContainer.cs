// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Mods;

public partial class ModSearchContainer : SearchContainer
{
    /// <summary>
    /// A string that should match the <see cref="IFilterable"/> children
    /// </summary>
    public string ForcedSearchTerm
    {
        get => SearchTerm;
        set
        {
            if (value == SearchTerm)
                return;

            SearchTerm = value;
            Update();
        }
    }
}
