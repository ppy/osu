//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.GameModes.Backgrounds;
using osu.Framework;
using osu.Game.Database;
using osu.Framework.Graphics.Primitives;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.GameModes.Play
{
    public class PlaySongSelect : OsuGameMode
    {
        private Bindable<PlayMode> playMode;
        private BeatmapDatabase beatmaps;
        private BeatmapSetInfo selectedBeatmapSet;

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
            var group = new BeatmapGroup(beatmapSet);
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
            const float backgroundWidth = 0.6f;
            const float backgroundSlant = 25;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(backgroundWidth, 0.5f),
                    Colour = new Color4(0, 0, 0, 0.5f),
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Y,
                    Size = new Vector2(backgroundWidth, 0.5f),
                    Position = new Vector2(0, 0.5f),
                    Colour = new Color4(0, 0, 0, 0.5f),
                },
                scrollContainer = new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Size = new Vector2(0.5f, 1),
                    Position = new Vector2(0.5f, 0),
                    Children = new Drawable[]
                    {
                        setList = new FlowContainer
                        {
                            Padding = new MarginPadding { Top = 25, Bottom = 25 },
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
