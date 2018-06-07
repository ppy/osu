// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using OpenTK;

namespace osu.Game.Screens.Multi.Components
{
    public class BeatmapTypeInfo : FillFlowContainer
    {
        private readonly ModeTypeInfo modeTypeInfo;
        private readonly BeatmapTitle beatmapTitle;
        private readonly OsuSpriteText beatmapAuthor;

        public BeatmapInfo Beatmap
        {
            set
            {
                modeTypeInfo.Beatmap = beatmapTitle.Beatmap = value;
                beatmapAuthor.Text = value == null ? string.Empty : $"mapped by {value.Metadata.Author}";
            }
        }

        public GameType Type
        {
            set { modeTypeInfo.Type = value; }
        }

        public BeatmapTypeInfo()
        {
            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;
            LayoutDuration = 100;
            Spacing = new Vector2(5f, 0f);

            Children = new Drawable[]
            {
                modeTypeInfo = new ModeTypeInfo(),
                new Container
                {
                    AutoSizeAxes = Axes.X,
                    Height = 30,
                    Margin = new MarginPadding { Left = 5 },
                    Children = new Drawable[]
                    {
                        beatmapTitle = new BeatmapTitle(),
                        beatmapAuthor = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            TextSize = 14,
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            beatmapAuthor.Colour = colours.Gray9;
        }
    }
}
