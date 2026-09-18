// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;

namespace osu.Game.Overlays.Settings
{
    public abstract partial class SettingsFilterableGroup : CompositeDrawable, IFilterable
    {
        public IEnumerable<LocalisableString> FilterTerms => InternalChildren.SelectMany(getFilterTerms);

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }

        private IEnumerable<LocalisableString> getFilterTerms(Drawable drawable)
        {
            var filterTerms = new List<LocalisableString>();

            if (drawable is IContainerEnumerable<Drawable> container)
                filterTerms.AddRange(container.Children.SelectMany(getFilterTerms));

            if (drawable is IFilterable filterable)
                filterTerms.AddRange(filterable.FilterTerms);

            return filterTerms.AsEnumerable();
        }
    }
}
