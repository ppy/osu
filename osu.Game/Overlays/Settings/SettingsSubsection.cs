// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Settings
{
    [ExcludeFromDynamicCompile]
    public abstract class SettingsSubsection : FillFlowContainer, IHasFilterableChildren
    {
        protected override Container<Drawable> Content => FlowContent;

        protected readonly FillFlowContainer FlowContent;

        protected abstract LocalisableString Header { get; }

        public IEnumerable<IFilterable> FilterableChildren => Children.OfType<IFilterable>();

        // FilterTerms should contains both original string and localised string for user to search.
        // Since LocalisableString is unable to get original string at this time (2021-08-14),
        // only call .ToString() to use localised one.
        // TODO: Update here when FilterTerms accept LocalisableString.
        public virtual IEnumerable<string> FilterTerms => new[] { Header.ToString() };

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }

        protected SettingsSubsection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;

            FlowContent = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 8),
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = Header.ToString().ToUpper(), // TODO: Add localisation support after https://github.com/ppy/osu-framework/pull/4603 is merged.
                    Margin = new MarginPadding { Vertical = 30, Left = SettingsPanel.CONTENT_MARGINS, Right = SettingsPanel.CONTENT_MARGINS },
                    Font = OsuFont.GetFont(weight: FontWeight.Bold),
                },
                FlowContent
            });
        }
    }
}
