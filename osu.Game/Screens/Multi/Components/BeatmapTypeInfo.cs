// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Multi.Components
{
    public class BeatmapTypeInfo : MultiplayerComposite
    {
        public BeatmapTypeInfo()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LinkFlowContainer beatmapAuthor;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                LayoutDuration = 100,
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    new ModeTypeInfo(),
                    new Container
                    {
                        AutoSizeAxes = Axes.X,
                        Height = 30,
                        Margin = new MarginPadding { Left = 5 },
                        Children = new Drawable[]
                        {
                            new BeatmapTitle(),
                            beatmapAuthor = new LinkFlowContainer(s => s.Font = s.Font.With(size: 14))
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                AutoSizeAxes = Axes.Both
                            },
                        },
                    },
                }
            };

            CurrentItem.BindValueChanged(item =>
            {
                beatmapAuthor.Clear();

                var beatmap = item.NewValue?.Beatmap;

                if (beatmap != null)
                {
                    beatmapAuthor.AddText("mapped by ", s => s.Colour = OsuColour.Gray(0.8f));
                    beatmapAuthor.AddUserLink(beatmap.Metadata.Author);
                }
            }, true);
        }
    }
}
