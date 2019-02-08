// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;
using System;

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
                            Exit = () => ExitRequested?.Invoke(),
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

        private void current_ValueChanged(string newValue) => FilterChanged?.Invoke(newValue);

        public Action ExitRequested;

        public Action<string> FilterChanged;

        public class FilterTextBox : SearchTextBox
        {
            protected override Color4 BackgroundUnfocused => OsuColour.Gray(0.06f);
            protected override Color4 BackgroundFocused => OsuColour.Gray(0.12f);

            protected override bool AllowCommit => true;

            public FilterTextBox()
            {
                Masking = true;
                CornerRadius = 5;
            }
        }
    }
}
