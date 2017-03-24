// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Database;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using OpenTK;
using osu.Framework.Allocation;

namespace osu.Game.Screens.Select
{
    public class Details : Container
    {
        private FillFlowContainer metadataContainer;
        private SpriteText description;
        private SpriteText source;
        private FillFlowContainer<SpriteText> tags;
        private BeatmapMetadata metadata;
        public BeatmapMetadata Metadata
        {
            get
            {
                return metadata;
            }

            set
            {
                if (metadata == value) return;
                metadata = value;
                source.Text = metadata.Source;
                tags.Children = metadata.Tags.Split(' ').ToList().Select(text => new SpriteText { Text = text });
            }
        }

        public Details()
        {
            Children = new[]
            {
                metadataContainer = new FillFlowContainer()
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.4f,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = "Description",
                        },
                        description = new SpriteText(),
                        new SpriteText
                        {
                            Text = "Source",
                            Margin = new MarginPadding { Top = 20 },
                        },
                        source = new SpriteText(),
                        new SpriteText
                        {
                            Text = "Tags",
                            Margin = new MarginPadding { Top = 20 },
                        },
                        tags = new FillFlowContainer<SpriteText>
                        {
                            RelativeSizeAxes = Axes.X,
                            Spacing = new Vector2(3,0),
                        },
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            description.Colour = colour.GrayB;
            source.Colour = colour.GrayB;
            tags.Colour = colour.Yellow;
        }
    }
}
