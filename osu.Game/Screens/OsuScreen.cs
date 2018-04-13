﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Framework.Graphics;
using osu.Game.Rulesets;
using osu.Game.Screens.Menu;
using osu.Framework.Input;
using OpenTK.Input;

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

        protected BindableBool ShowOverlays = new BindableBool();

        /// <summary>
        /// Whether overlays should be shown when this screen is entered or resumed.
        /// </summary>
        public virtual bool ShowOverlaysOnEnter => true;

        /// <summary>
        /// Whether this <see cref="OsuScreen"/> allows the cursor to be displayed.
        /// </summary>
        public virtual bool CursorVisible => true;

        protected new OsuGameBase Game => base.Game as OsuGameBase;

        private OsuLogo logo;

        /// <summary>
        /// Whether the beatmap or ruleset should be allowed to be changed by the user or game.
        /// Used to mark exclusive areas where this is strongly prohibited, like gameplay.
        /// </summary>
        public virtual bool AllowBeatmapRulesetChange => true;

        protected readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        protected virtual float BackgroundParallaxAmount => 1;

        private ParallaxContainer backgroundParallaxContainer;

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
            {
                Ruleset.BindTo(osuGame.Ruleset);
                ShowOverlays.BindTo(osuGame.ShowOverlays);
            }

            sampleExit = audio.Sample.Get(@"UI/screen-back");
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat || !IsCurrentScreen) return false;

            switch (args.Key)
            {
                case Key.Escape:
                    Exit();
                    return true;
            }

            return base.OnKeyDown(state, args);
        }

        protected override void OnResuming(Screen last)
        {
            sampleExit?.Play();
            applyArrivingDefaults(true);

            base.OnResuming(last);
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);
            onSuspendingLogo();
        }

        protected override void OnEntering(Screen last)
        {
            OsuScreen lastOsu = last as OsuScreen;

            BackgroundScreen bg = CreateBackground();

            if (lastOsu?.Background != null)
            {
                backgroundParallaxContainer = lastOsu.backgroundParallaxContainer;

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

                AddInternal(backgroundParallaxContainer = new ParallaxContainer
                {
                    Depth = float.MaxValue,
                    Children = new[]
                    {
                        Background = bg
                    }
                });
            }

            if ((logo = lastOsu?.logo) == null)
                LoadComponentAsync(logo = new OsuLogo { Alpha = 0 }, AddInternal);

            applyArrivingDefaults(false);

            base.OnEntering(last);
        }

        protected override bool OnExiting(Screen next)
        {
            if (ValidForResume && logo != null)
                onExitingLogo();

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

        /// <summary>
        /// Fired when this screen was entered or resumed and the logo state is required to be adjusted.
        /// </summary>
        protected virtual void LogoArriving(OsuLogo logo, bool resuming)
        {
            logo.Action = null;
            logo.FadeOut(300, Easing.OutQuint);
            logo.Anchor = Anchor.TopLeft;
            logo.Origin = Anchor.Centre;
            logo.RelativePositionAxes = Axes.None;
            logo.Triangles = true;
            logo.Ripple = true;
        }

        private void applyArrivingDefaults(bool isResuming)
        {
            logo.AppendAnimatingAction(() => LogoArriving(logo, isResuming), true);

            if (backgroundParallaxContainer != null)
                backgroundParallaxContainer.ParallaxAmount = ParallaxContainer.DEFAULT_PARALLAX_AMOUNT * BackgroundParallaxAmount;

            ShowOverlays.Value = ShowOverlaysOnEnter;
        }

        private void onExitingLogo()
        {
            logo.AppendAnimatingAction(() => { LogoExiting(logo); }, false);
        }

        /// <summary>
        /// Fired when this screen was exited to add any outwards transition to the logo.
        /// </summary>
        protected virtual void LogoExiting(OsuLogo logo)
        {
        }

        private void onSuspendingLogo()
        {
            logo.AppendAnimatingAction(() => { LogoSuspending(logo); }, false);
        }

        /// <summary>
        /// Fired when this screen was suspended to add any outwards transition to the logo.
        /// </summary>
        protected virtual void LogoSuspending(OsuLogo logo)
        {
        }
    }
}
