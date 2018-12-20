// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.Multi.Components
{
    public class BeatmapTypeInfo : CompositeDrawable
    {
        private readonly OsuSpriteText beatmapAuthor;

        public readonly IBindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        public readonly IBindable<GameType> Type = new Bindable<GameType>();

        public BeatmapTypeInfo()
        {
            AutoSizeAxes = Axes.Both;

            BeatmapTitle beatmapTitle;
            ModeTypeInfo modeTypeInfo;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                LayoutDuration = 100,
                Spacing = new Vector2(5, 0),
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
                }
            };

            modeTypeInfo.Beatmap.BindTo(Beatmap);
            modeTypeInfo.Type.BindTo(Type);

            beatmapTitle.Beatmap.BindTo(Beatmap);

            Beatmap.BindValueChanged(v => beatmapAuthor.Text = v == null ? string.Empty : $"mapped by {v.Metadata.Author}");
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            beatmapAuthor.Colour = colours.GrayC;
        }
    }
}
