//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.IO;
using osu.Game.GameModes.Backgrounds;
using osu.Framework;
using osu.Game.Database;
using osu.Framework.Graphics.Primitives;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Textures;

namespace osu.Game.GameModes.Play
{
    public class PlaySongSelect : OsuGameMode
    {
        private Bindable<PlayMode> playMode;
        private BeatmapDatabase beatmaps;
        private BeatmapSetInfo selectedBeatmapSet;
        private BeatmapResourceStore beatmapResources;
        private TextureStore beatmapTextureResources;

        // TODO: use currently selected track as bg
        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");

        private ScrollContainer scrollContainer;
        private FlowContainer setList;

        private void selectBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            selectedBeatmapSet = beatmapSet;
            foreach (var child in setList.Children)
            {
                var childGroup = child as BeatmapGroup;
                childGroup.Collapsed = childGroup.BeatmapSet != beatmapSet;
            }
        }

        private void addBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            beatmapSet = beatmaps.GetWithChildren<BeatmapSetInfo>(beatmapSet.BeatmapSetID);
            var group = new BeatmapGroup(beatmapSet, beatmapResources, beatmapTextureResources);
            group.SetSelected += (selectedSet) => selectBeatmapSet(selectedSet);
            setList.Add(group);
        }

        private void addBeatmapSets()
        {
            foreach (var beatmapSet in beatmaps.Query<BeatmapSetInfo>())
                addBeatmapSet(beatmapSet);
        }

        public PlaySongSelect()
        {
            const float scrollWidth = 500;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1),
                    Padding = new MarginPadding { Right = scrollWidth - 100 },
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(1, 0.5f),
                            Colour = new Color4(0, 0, 0, 0.5f),
                            Shear = new Vector2(0.15f, 0),
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            RelativePositionAxes = Axes.Y,
                            Size = new Vector2(1, -0.5f),
                            Position = new Vector2(0, 1),
                            Colour = new Color4(0, 0, 0, 0.5f),
                            Shear = new Vector2(-0.15f, 0),
                        },
                    }
                },
                scrollContainer = new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(scrollWidth, 1),
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Children = new Drawable[]
                    {
                        setList = new FlowContainer
                        {
                            Padding = new MarginPadding { Left = 25, Top = 25, Bottom = 25 },
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(1, 0),
                            Direction = FlowDirection.VerticalOnly,
                            Spacing = new Vector2(0, 25),
                        }
                    }
                }
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            OsuGame osu = game as OsuGame;
            if (osu != null)
            {
                playMode = osu.PlayMode;
                playMode.ValueChanged += PlayMode_ValueChanged;
                // Temporary:
                scrollContainer.Padding = new MarginPadding { Top = osu.Toolbar.Height };
            }
            
            beatmaps = (game as OsuGameBase).Beatmaps;
            beatmapTextureResources = new TextureStore(
                new RawTextureLoaderStore(beatmapResources = new BeatmapResourceStore(beatmaps)));
            beatmaps.BeatmapSetAdded += bset => Scheduler.Add(() => addBeatmapSet(bset));
            addBeatmapSets();
            var first = setList.Children.FirstOrDefault() as BeatmapGroup;
            if (first != null)
            {
                first.Collapsed = false;
                selectedBeatmapSet = first.BeatmapSet;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (playMode != null)
                playMode.ValueChanged -= PlayMode_ValueChanged;
        }

        private void PlayMode_ValueChanged(object sender, EventArgs e)
        {
        }
    }
}
