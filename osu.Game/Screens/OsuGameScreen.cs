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

        private bool boundToBeatmap;
        private Bindable<WorkingBeatmap> beatmap;

        public WorkingBeatmap Beatmap
        {
            get
            {
                bindBeatmap();
                return beatmap.Value;
            }
            set
            {
                bindBeatmap();
                beatmap.Value = value;
            }
        }

        private void bindBeatmap()
        {
            if (beatmap == null)
                beatmap = new Bindable<WorkingBeatmap>();

            if (!boundToBeatmap)
            {
                beatmap.ValueChanged += beatmap_ValueChanged;
                boundToBeatmap = true;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (boundToBeatmap)
                beatmap.ValueChanged -= beatmap_ValueChanged;

            base.Dispose(isDisposing);
        }

        private void beatmap_ValueChanged(object sender, EventArgs e)
        {
            OnBeatmapChanged(beatmap.Value);
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGameBase game)
        {
            if (beatmap == null)
                beatmap = game?.Beatmap;
        }

        public override bool Push(Screen screen)
        {
            OsuScreen nextOsu = screen as OsuScreen;
            if (nextOsu != null)
            {
                nextOsu.beatmap = beatmap;
            }

            return base.Push(screen);
        }

        protected virtual void OnBeatmapChanged(WorkingBeatmap beatmap)
        {

        }

        protected override void OnEntering(Screen last)
        {
            OsuScreen lastOsu = last as OsuScreen;

            BackgroundScreen bg = CreateBackground();

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
