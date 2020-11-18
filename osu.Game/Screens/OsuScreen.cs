﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.Menu;
using osu.Game.Overlays;
using osu.Game.Users;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens
{
    public abstract class OsuScreen : Screen, IOsuScreen, IHasDescription
    {
        /// <summary>
        /// The amount of negative padding that should be applied to game background content which touches both the left and right sides of the screen.
        /// This allows for the game content to be pushed by the options/notification overlays without causing black areas to appear.
        /// </summary>
        public const float HORIZONTAL_OVERFLOW_PADDING = 50;

        /// <summary>
        /// A user-facing title for this screen.
        /// </summary>
        public virtual string Title => GetType().Name;

        public string Description => Title;

        public virtual bool AllowBackButton => true;

        public virtual bool AllowExternalScreenChange => false;

        /// <summary>
        /// Whether all overlays should be hidden when this screen is entered or resumed.
        /// </summary>
        public virtual bool HideOverlaysOnEnter => false;

        /// <summary>
        /// The initial overlay activation mode to use when this screen is entered for the first time.
        /// </summary>
        protected virtual OverlayActivation InitialOverlayActivationMode => OverlayActivation.All;

        protected readonly Bindable<OverlayActivation> OverlayActivationMode;

        IBindable<OverlayActivation> IOsuScreen.OverlayActivationMode => OverlayActivationMode;

        public virtual bool CursorVisible => true;

        protected new OsuGameBase Game => base.Game as OsuGameBase;

        /// <summary>
        /// The <see cref="UserActivity"/> to set the user's activity automatically to when this screen is entered.
        /// <para>This <see cref="Activity"/> will be automatically set to <see cref="InitialActivity"/> for this screen on entering for the first time
        /// unless <see cref="Activity"/> is manually set before.</para>
        /// </summary>
        protected virtual UserActivity InitialActivity => null;

        /// <summary>
        /// The current <see cref="UserActivity"/> for this screen.
        /// </summary>
        protected readonly Bindable<UserActivity> Activity = new Bindable<UserActivity>();

        IBindable<UserActivity> IOsuScreen.Activity => Activity;

        /// <summary>
        /// Whether to disallow changes to game-wise Beatmap/Ruleset bindables for this screen (and all children).
        /// </summary>
        public virtual bool DisallowExternalBeatmapRulesetChanges => false;

        private SampleChannel sampleExit;

        protected virtual bool PlayResumeSound => true;

        public virtual float BackgroundParallaxAmount => 1;

        public virtual bool AllowRateAdjustments => true;

        public Bindable<WorkingBeatmap> Beatmap { get; private set; }

        public Bindable<RulesetInfo> Ruleset { get; private set; }

        public Bindable<IReadOnlyList<Mod>> Mods { get; private set; }

        private OsuScreenDependencies screenDependencies;

        internal void CreateLeasedDependencies(IReadOnlyDependencyContainer dependencies) => createDependencies(dependencies);

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            if (screenDependencies == null)
            {
                if (DisallowExternalBeatmapRulesetChanges)
                    throw new InvalidOperationException($"Screens that specify {nameof(DisallowExternalBeatmapRulesetChanges)} must be pushed immediately.");

                createDependencies(parent);
            }

            return base.CreateChildDependencies(screenDependencies);
        }

        private void createDependencies(IReadOnlyDependencyContainer dependencies)
        {
            screenDependencies = new OsuScreenDependencies(DisallowExternalBeatmapRulesetChanges, dependencies);

            Beatmap = screenDependencies.Beatmap;
            Ruleset = screenDependencies.Ruleset;
            Mods = screenDependencies.Mods;
        }

        protected BackgroundScreen Background => backgroundStack?.CurrentScreen as BackgroundScreen;

        private BackgroundScreen localBackground;

        [Resolved(canBeNull: true)]
        private BackgroundScreenStack backgroundStack { get; set; }

        [Resolved(canBeNull: true)]
        private OsuLogo logo { get; set; }

        protected OsuScreen()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            OverlayActivationMode = new Bindable<OverlayActivation>(InitialOverlayActivationMode);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame osu, AudioManager audio)
        {
            sampleExit = audio.Samples.Get(@"UI/screen-back");

            Activity.Value ??= InitialActivity;
        }

        public override void OnResuming(IScreen last)
        {
            if (PlayResumeSound)
                sampleExit?.Play();
            applyArrivingDefaults(true);

            base.OnResuming(last);
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);

            onSuspendingLogo();
        }

        public override void OnEntering(IScreen last)
        {
            applyArrivingDefaults(false);

            backgroundStack?.Push(localBackground = CreateBackground());

            base.OnEntering(last);
        }

        public override bool OnExiting(IScreen next)
        {
            if (ValidForResume && logo != null)
                onExitingLogo();

            if (base.OnExiting(next))
                return true;

            if (localBackground != null && backgroundStack?.CurrentScreen == localBackground)
                backgroundStack?.Exit();

            return false;
        }

        /// <summary>
        /// Fired when this screen was entered or resumed and the logo state is required to be adjusted.
        /// </summary>
        protected virtual void LogoArriving(OsuLogo logo, bool resuming)
        {
            ApplyLogoArrivingDefaults(logo);
        }

        private void applyArrivingDefaults(bool isResuming)
        {
            logo?.AppendAnimatingAction(() =>
            {
                if (this.IsCurrentScreen()) LogoArriving(logo, isResuming);
            }, true);
        }

        /// <summary>
        /// Applies default animations to an arriving logo.
        /// Todo: This should not exist.
        /// </summary>
        /// <param name="logo">The logo to apply animations to.</param>
        public static void ApplyLogoArrivingDefaults(OsuLogo logo)
        {
            logo.Action = null;
            logo.FadeOut(300, Easing.OutQuint);
            logo.Anchor = Anchor.TopLeft;
            logo.Origin = Anchor.Centre;
            logo.RelativePositionAxes = Axes.Both;
            logo.BeatMatching = true;
            logo.Triangles = true;
            logo.Ripple = true;
        }

        private void onExitingLogo()
        {
            logo?.AppendAnimatingAction(() => LogoExiting(logo), false);
        }

        /// <summary>
        /// Fired when this screen was exited to add any outwards transition to the logo.
        /// </summary>
        protected virtual void LogoExiting(OsuLogo logo)
        {
        }

        private void onSuspendingLogo()
        {
            logo?.AppendAnimatingAction(() => LogoSuspending(logo), false);
        }

        /// <summary>
        /// Fired when this screen was suspended to add any outwards transition to the logo.
        /// </summary>
        protected virtual void LogoSuspending(OsuLogo logo)
        {
        }

        /// <summary>
        /// Override to create a BackgroundMode for the current screen.
        /// Note that the instance created may not be the used instance if it matches the BackgroundMode equality clause.
        /// </summary>
        protected virtual BackgroundScreen CreateBackground() => null;

        public virtual bool OnBackButton() => false;
    }
}
