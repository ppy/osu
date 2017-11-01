// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using OpenTK;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio;
using osu.Game.Rulesets;

namespace osu.Game.Screens
{
    public abstract class OsuScreen : Screen
    {
        public BackgroundScreen Background { get; private set; }

        /// <summary>
        /// Override to create a BackgroundMode for the current screen.
        /// Note that the instance created may not be the used instance if it matches the BackgroundMode equality clause.
        /// </summary>
        protected virtual BackgroundScreen CreateBackground() => null;

        public virtual bool ShowOverlays => true;

        protected new OsuGameBase Game => base.Game as OsuGameBase;

        public virtual bool HasLocalCursorDisplayed => false;

        /// <summary>
        /// Whether the beatmap or ruleset should be allowed to be changed by the user or game.
        /// Used to mark exclusive areas where this is strongly prohibited, like gameplay.
        /// </summary>
        public virtual bool AllowBeatmapRulesetChange => true;

        protected readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        public WorkingBeatmap InitialBeatmap
        {
            set
            {
                if (IsLoaded) throw new InvalidOperationException($"Cannot set {nameof(InitialBeatmap)} post-load.");
                Beatmap.Value = value;
            }
        }

        protected readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        private SampleChannel sampleExit;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGameBase game, OsuGame osuGame, AudioManager audio)
        {
            if (game != null)
            {
                //if we were given a beatmap at ctor time, we want to pass this on to the game-wide beatmap.
                var localMap = Beatmap.Value;
                Beatmap.BindTo(game.Beatmap);
                if (localMap != null)
                    Beatmap.Value = localMap;
            }

            if (osuGame != null)
                Ruleset.BindTo(osuGame.Ruleset);

            sampleExit = audio.Sample.Get(@"UI/melodic-1");
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);
            sampleExit?.Play();
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

            Beatmap.UnbindAll();
            return false;
        }
    }
}
