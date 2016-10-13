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

namespace osu.Game.GameModes.Play
{
    public class PlaySongSelect : OsuGameMode
    {
        private Bindable<PlayMode> playMode;
        private BeatmapDatabase beatmaps;

        // TODO: use currently selected track as bg
        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");

        private ScrollContainer scrollContainer;
        private FlowContainer setList;
        
        private void addBeatmapSet(BeatmapSet beatmapSet)
        {
            var group = new BeatmapGroup(beatmapSet);
            group.SetSelected += (selectedSet) =>
            {
                foreach (var child in setList.Children)
                {
                    var childGroup = child as BeatmapGroup;
                    childGroup.Collapsed = childGroup.BeatmapSet != selectedSet;
                }
            };
            setList.Add(group);
        }

        private void addBeatmapSets()
        {
            var sets = beatmaps.GetBeatmapSets();
            foreach (var beatmapSet in sets)
                addBeatmapSet(beatmapSet);
        }
        
        public PlaySongSelect()
        {
            Children = new[]
            {
                scrollContainer = new ScrollContainer
                {
                    Children = new[]
                    {
                        setList = new FlowContainer
                        {
                            Direction = FlowDirection.VerticalOnly,
                            Padding = new MarginPadding(25),
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
            (setList.Children.First() as BeatmapGroup).Collapsed = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            playMode.ValueChanged -= PlayMode_ValueChanged;
        }

        private void PlayMode_ValueChanged(object sender, EventArgs e)
        {
        }
    }
}
