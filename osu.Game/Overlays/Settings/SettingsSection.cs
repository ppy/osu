// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Overlays.Settings
{
    public abstract class SettingsSection : Container, IHasFilterableChildren
    {
        protected FillFlowContainer FlowContent;
        protected override Container<Drawable> Content => FlowContent;

        public abstract FontAwesome Icon { get; }
        public abstract string Header { get; }

        public IEnumerable<IFilterable> FilterableChildren => Children.OfType<IFilterable>();
        public string[] FilterTerms => new[] { Header };
        public bool MatchingCurrentFilter
        {
            set
            {
                FadeTo(value ? 1 : 0);
            }
        }

        private readonly SpriteText headerLabel;

        protected SettingsSection()
        {
            Margin = new MarginPadding { Top = 20 };
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            const int header_size = 26;
            const int header_margin = 25;
            const int border_size = 2;
            AddInternal(new Drawable[]
            {
                new Box
                {
                    Colour = new Color4(0, 0, 0, 255),
                    RelativeSizeAxes = Axes.X,
                    Height = border_size,
                },
                new Container
                {
                    Padding = new MarginPadding
                    {
                        Top = 20 + border_size,
                        Left = SettingsOverlay.CONTENT_MARGINS,
                        Right = SettingsOverlay.CONTENT_MARGINS,
                        Bottom = 10,
                    },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new[]
                    {
                        headerLabel = new OsuSpriteText
                        {
                            TextSize = header_size,
                            Text = Header,
                        },
                        FlowContent = new FillFlowContainer
                        {
                            Margin = new MarginPadding { Top = header_size + header_margin },
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 30),
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        },
                    }
                },
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            headerLabel.Colour = colours.Yellow;
        }
    }
}