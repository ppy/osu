﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using OpenTK;

namespace osu.Game.Screens
{
    public abstract class OsuScreen : Screen
    {
        internal BackgroundScreen Background { get; private set; }

        /// <summary>
        /// Override to create a BackgroundMode for the current screen.
        /// Note that the instance created may not be the used instance if it matches the BackgroundMode equality clause.
        /// </summary>
        protected virtual BackgroundScreen CreateBackground() => null;

        internal virtual bool ShowOverlays => true;

        protected new OsuGameBase Game => base.Game as OsuGameBase;

        internal virtual bool HasLocalCursorDisplayed => false;

        internal virtual bool AllowRulesetChange => true;

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

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

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGameBase game, OsuGame osuGame)
        {
            if (game != null)
            {
                //if we were given a beatmap at ctor time, we want to pass this on to the game-wide beatmap.
                var localMap = beatmap.Value;
                beatmap.BindTo(game.Beatmap);
                if (localMap != null)
                    beatmap.Value = localMap;
            }

            if (osuGame != null)
                ruleset.BindTo(osuGame.Ruleset);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.ValueChanged += OnBeatmapChanged;
        }

        /// <summary>
        /// The global Beatmap was changed.
        /// </summary>
        protected virtual void OnBeatmapChanged(WorkingBeatmap beatmap)
        {
        }

        protected override void Update()
        {
            if (!IsCurrentScreen) return;

            ruleset.Disabled = !AllowRulesetChange;
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
                // this makes up for the fact our padding changes when the global toolbar is visible.
                bg.Scale = new Vector2(1.06f);

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
                    //We need to use MakeCurrent in case we are jumping up multiple game screens.
                    nextOsu.Background?.MakeCurrent();
                else
                    Background.Exit();
            }

            if (base.OnExiting(next))
                return true;

            // while this is not necessary as we are constructing our own bindable, there are cases where
            // the GC doesn't run as fast as expected and this is triggered post-exit.
            // added to resolve https://github.com/ppy/osu/issues/829
            beatmap.ValueChanged -= OnBeatmapChanged;

            return false;
        }
    }
}
