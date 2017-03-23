// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens
{
    public abstract class OsuScreen : Screen
    {
        internal BackgroundScreen Background { get; private set; }

        /// <summary>
        /// Override to create a BackgroundMode for the current GameMode.
        /// Note that the instance created may not be the used instance if it matches the BackgroundMode equality clause.
        /// </summary>
        protected virtual BackgroundScreen CreateBackground() => null;

        internal virtual bool ShowOverlays => true;

        protected new OsuGameBase Game => base.Game as OsuGameBase;

        internal virtual bool HasLocalCursorDisplayed => false;

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        public WorkingBeatmap Beatmap
        {
            get
            {
                return beatmap.Value;
            }
            set
            {
                beatmap.Value = value;
            }
        }

        private void beatmap_ValueChanged(object sender, EventArgs e)
        {
            OnBeatmapChanged(beatmap.Value);
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGameBase game)
        {
            if (game != null)
            {
                //if we were given a beatmap at ctor time, we want to pass this on to the game-wide beatmap.
                var localMap = beatmap.Value;
                beatmap.BindTo(game.Beatmap);
                if (localMap != null)
                    beatmap.Value = localMap;
            }

            beatmap.ValueChanged += beatmap_ValueChanged;
        }

        protected virtual void OnBeatmapChanged(WorkingBeatmap beatmap)
        {

        }

        protected override void OnEntering(Screen last)
        {
            OsuScreen lastOsu = last as OsuScreen;

            BackgroundScreen bg = CreateBackground();

            OnBeatmapChanged(Beatmap);

            if (lastOsu?.Background != null)
            {
                if (bg == null || lastOsu.Background.Equals(bg))
                    //we can keep the previous mode's background.
                    Background = lastOsu.Background;
                else
                {
                    lastOsu.Background.Push(Background = bg);
                }
            }
            else if (bg != null)
            {
                AddInternal(new ParallaxContainer
                {
                    Depth = float.MaxValue,
                    Children = new[]
                    {
                        Background = bg
                    }
                });
            }

            base.OnEntering(last);
        }

        protected override bool OnExiting(Screen next)
        {
            OsuScreen nextOsu = next as OsuScreen;

            if (Background != null && !Background.Equals(nextOsu?.Background))
            {
                if (nextOsu != null)
                    //We need to use MakeCurrent in case we are jumping up multiple game modes.
                    nextOsu.Background?.MakeCurrent();
                else
                    Background.Exit();
            }

            return base.OnExiting(next);
        }
    }
}
