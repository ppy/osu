// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;

namespace osu.Game.Overlays.Music
{
    public class FilterControl : Container
    {
        public readonly FilterTextBox Search;

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
                        new CollectionsDropdown<PlaylistCollection>
                        {
                            RelativeSizeAxes = Axes.X,
                            Items = new[] { PlaylistCollection.All },
                        }
                    },
                },
            };

            Search.Current.ValueChanged += current_ValueChanged;
        }

        private void current_ValueChanged(ValueChangedEvent<string> e) => FilterChanged?.Invoke(e.NewValue);

        public Action<string> FilterChanged;

        public class FilterTextBox : SearchTextBox
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
