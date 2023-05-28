// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Mods
{
    public partial class ModSearchContainer : SearchContainer
    {
        /// <summary>
        /// A string that should match the <see cref="IFilterable"/> children
        /// </summary>
        /// <remarks>
        /// Same as <see cref="SearchContainer{T}.SearchTerm"/> except the filtering is guarantied to be performed even when <see cref="SearchContainer{T}.Update"/> can't be run.
        /// </remarks>
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
}
