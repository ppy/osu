// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using System;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Music
{
    public partial class FilterControl : Container
    {
        public Action<FilterCriteria> FilterChanged;

        public readonly FilterTextBox Search;
        private readonly NowPlayingCollectionDropdown collectionDropdown;

        public FilterControl()
        {
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        Search = new FilterTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                        },
                        collectionDropdown = new NowPlayingCollectionDropdown { RelativeSizeAxes = Axes.X }
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Search.Current.BindValueChanged(_ => updateCriteria());
            collectionDropdown.Current.BindValueChanged(_ => updateCriteria(), true);
        }

        private void updateCriteria() => FilterChanged?.Invoke(createCriteria());

        private FilterCriteria createCriteria() => new FilterCriteria
        {
            SearchText = Search.Current.Value,
            Collection = collectionDropdown.Current.Value?.Collection
        };

        public partial class FilterTextBox : BasicSearchTextBox
        {
            protected override bool AllowCommit => true;

            [BackgroundDependencyLoader]
            private void load()
            {
                Masking = true;
                CornerRadius = 5;

                BackgroundUnfocused = OsuColour.Gray(0.06f);
                BackgroundFocused = OsuColour.Gray(0.12f);
            }
        }
    }
}
