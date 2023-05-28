// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Mods
{
    public partial class ModSearchContainer : SearchContainer
    {
        /// <summary>
        /// Same as <see cref="SearchContainer{T}.SearchTerm"/> except the filtering is guarantied to be performed
        /// </summary>
        /// <remarks>
        /// This is required because <see cref="ModColumn"/> can be hidden when search term applied
        /// therefore <see cref="SearchContainer{T}.Update"/> cannot be reached and filter cannot automatically re-validate itself.
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
