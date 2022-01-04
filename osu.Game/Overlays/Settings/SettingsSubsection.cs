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
                Margin = new MarginPadding { Top = SettingsSection.ITEM_SPACING },
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, SettingsSection.ITEM_SPACING),
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };
        }

        private const int header_height = 43;
        private const int header_font_size = 20;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = Header,
                    Margin = new MarginPadding { Vertical = (header_height - header_font_size) * 0.5f, Horizontal = SettingsPanel.CONTENT_MARGINS },
                    Font = OsuFont.GetFont(size: header_font_size),
                },
                FlowContent
            });
        }
    }
}
