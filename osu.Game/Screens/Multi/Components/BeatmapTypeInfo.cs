// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
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
        private LinkFlowContainer beatmapAuthor;

        public BeatmapTypeInfo()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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

            Playlist.ItemsAdded += _ => updateInfo();
            Playlist.ItemsRemoved += _ => updateInfo();

            updateInfo();
        }

        private void updateInfo()
        {
            beatmapAuthor.Clear();

            var beatmap = Playlist.FirstOrDefault()?.Beatmap;

            if (beatmap != null)
            {
                beatmapAuthor.AddText("mapped by ", s => s.Colour = OsuColour.Gray(0.8f));
                beatmapAuthor.AddUserLink(beatmap.Value.Metadata.Author);
            }
        }
    }
}
