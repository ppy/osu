// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;
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
                            Items = new[] { new KeyValuePair<string, PlaylistCollection>(@"All", PlaylistCollection.All) },
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
            private Color4 backgroundColour;

            protected override Color4 BackgroundUnfocused => backgroundColour;
            protected override Color4 BackgroundFocused => backgroundColour;
            protected override bool AllowCommit => true;

            public FilterTextBox()
            {
                Masking = true;
                CornerRadius = 5;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                backgroundColour = colours.Gray2;
            }
        }
    }
}
