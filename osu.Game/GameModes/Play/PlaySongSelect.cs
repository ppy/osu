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

namespace osu.Game.GameModes.Play
{
    class PlaySongSelect : OsuGameMode
    {
        private Bindable<PlayMode> playMode;

        // TODO: use currently selected track as bg
        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");

        private FlowContainer setList;

        private Drawable createSetUI(BeatmapSet bset)
        {
            return new SpriteText { Text = bset.Metadata.Title };
        }

        private void addBeatmapSets()
        {
            var sets = (Game as OsuGame).Beatmaps.GetBeatmapSets();
            foreach (var beatmapSet in sets)
                setList.Add(createSetUI(beatmapSet));
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            OsuGame osu = game as OsuGame;

            playMode = osu.PlayMode;
            playMode.ValueChanged += PlayMode_ValueChanged;

            Add(new ScrollContainer
            {
                OriginPosition = new OpenTK.Vector2(0, -osu.Toolbar.Height),
                Children = new[]
                {
                    setList = new FlowContainer
                    {
                        Direction = FlowDirection.VerticalOnly,
                        Padding = new OpenTK.Vector2(25, 25)
                    }
                }
            });

            addBeatmapSets();

            (Game as OsuGame).Beatmaps.BeatmapSetAdded += bset => setList.Add(createSetUI(bset));
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
