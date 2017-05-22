// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;
using System;

namespace osu.Game.Overlays.Music
{
    internal class FilterControl : Container
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

        private class CollectionsDropdown<T> : OsuDropdown<T>
        {
            protected override DropdownHeader CreateHeader() => new CollectionsHeader { AccentColour = AccentColour };
            protected override Menu CreateMenu() => new CollectionsMenu();

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Gray6;
            }

            private class CollectionsHeader : OsuDropdownHeader
            {
                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    BackgroundColour = colours.Gray4;
                }

                public CollectionsHeader()
                {
                    CornerRadius = 5;
                    Height = 30;
                    Icon.TextSize = 14;
                    Icon.Margin = new MarginPadding(0);
                    Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 10, Right = 10 };
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.3f),
                        Radius = 3,
                        Offset = new Vector2(0f, 1f),
                    };
                }
            }

            private class CollectionsMenu : OsuMenu
            {
                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Background.Colour = colours.Gray4;
                }

                public CollectionsMenu()
                {
                    CornerRadius = 5;
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.3f),
                        Radius = 3,
                        Offset = new Vector2(0f, 1f),
                    };
                }
            }
        }
    }
}
