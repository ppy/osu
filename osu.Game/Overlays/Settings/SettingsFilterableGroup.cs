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
        public IEnumerable<LocalisableString> FilterTerms => Children.OfType<IFilterable>().SelectMany(f => f.FilterTerms);

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }
    }
}
