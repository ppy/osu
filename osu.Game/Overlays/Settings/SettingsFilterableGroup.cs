// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;

namespace osu.Game.Overlays.Settings
{
    /// <summary>
    /// Sometimes we want a group of settings to show together when any of the contained filter terms matches.
    /// Surrounding drawables with this container will ensure that happens.
    /// </summary>
    public sealed partial class SettingsFilterableGroup : FillFlowContainer, IFilterable
    {
        public IEnumerable<LocalisableString> FilterTerms => InternalChildren.SelectMany(getFilterTerms);

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }

        private IEnumerable<LocalisableString> getFilterTerms(Drawable drawable)
        {
            // Roughly matches how `SearchContainer` works.
            // See https://github.com/ppy/osu-framework/blob/a8f24f1cdebe84031c71f2e185818455863a08c1/osu.Framework/Graphics/Containers/SearchContainer.cs#L127
            var filterTerms = new List<LocalisableString>();

            if (drawable is IContainerEnumerable<Drawable> container)
            {
                foreach (var child in container.Children)
                    filterTerms.AddRange(getFilterTerms(child));
            }

            if (drawable is IFilterable filterable)
                filterTerms.AddRange(filterable.FilterTerms);

            return filterTerms;
        }
    }
}
