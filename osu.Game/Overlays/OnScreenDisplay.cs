// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Threading;
using osu.Game.Overlays.OSD;
using osuTK;

namespace osu.Game.Overlays
{
    /// <summary>
    /// An on-screen display which automatically tracks and displays toast notifications for <seealso cref="TrackedSettings"/>.
    /// Can also display custom content via <see cref="Display(Toast)"/>
    /// </summary>
    public class OnScreenDisplay : Container
    {
        private readonly Container box;

        private const float height = 110;
        private const float height_contracted = height * 0.9f;

        public OnScreenDisplay()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                box = new Container
                {
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(0.5f, 0.75f),
                    Masking = true,
                    AutoSizeAxes = Axes.X,
                    Height = height_contracted,
                    Alpha = 0,
                    CornerRadius = 20,
                },
            };
        }

        private readonly Dictionary<(object, IConfigManager), TrackedSettings> trackedConfigManagers = new Dictionary<(object, IConfigManager), TrackedSettings>();

        /// <summary>
        /// Registers a <see cref="ConfigManager{T}"/> to have its settings tracked by this <see cref="OnScreenDisplay"/>.
        /// </summary>
        /// <param name="source">The object that is registering the <see cref="ConfigManager{T}"/> to be tracked.</param>
        /// <param name="configManager">The <see cref="ConfigManager{T}"/> to be tracked.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="configManager"/> is null.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="configManager"/> is already being tracked from the same <paramref name="source"/>.</exception>
        public void BeginTracking(object source, ITrackableConfigManager configManager)
        {
            if (configManager == null) throw new ArgumentNullException(nameof(configManager));

            if (trackedConfigManagers.ContainsKey((source, configManager)))
                throw new InvalidOperationException($"{nameof(configManager)} is already registered.");

            var trackedSettings = configManager.CreateTrackedSettings();
            if (trackedSettings == null)
                return;

            configManager.LoadInto(trackedSettings);
            trackedSettings.SettingChanged += displayTrackedSettingChange;

            trackedConfigManagers.Add((source, configManager), trackedSettings);
        }

        /// <summary>
        /// Unregisters a <see cref="ConfigManager{T}"/> from having its settings tracked by this <see cref="OnScreenDisplay"/>.
        /// </summary>
        /// <param name="source">The object that registered the <see cref="ConfigManager{T}"/> to be tracked.</param>
        /// <param name="configManager">The <see cref="ConfigManager{T}"/> that is being tracked.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="configManager"/> is null.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="configManager"/> is not being tracked from the same <paramref name="source"/>.</exception>
        public void StopTracking(object source, ITrackableConfigManager configManager)
        {
            if (configManager == null) throw new ArgumentNullException(nameof(configManager));

            if (!trackedConfigManagers.TryGetValue((source, configManager), out var existing))
                return;

            existing.Unload();
            existing.SettingChanged -= displayTrackedSettingChange;

            trackedConfigManagers.Remove((source, configManager));
        }

        /// <summary>
        /// Displays the provided <see cref="Toast"/> temporarily.
        /// </summary>
        /// <param name="toast"></param>
        public void Display(Toast toast) => Schedule(() =>
        {
            box.Child = toast;
            DisplayTemporarily(box);
        });

        private void displayTrackedSettingChange(SettingDescription description) => Scheduler.AddOnce(Display, new TrackedSettingToast(description));

        private TransformSequence<Drawable> fadeIn;
        private ScheduledDelegate fadeOut;

        protected virtual void DisplayTemporarily(Drawable toDisplay)
        {
            // avoid starting a new fade-in if one is already active.
            if (fadeIn == null)
            {
                fadeIn = toDisplay.Animate(
                    b => b.FadeIn(500, Easing.OutQuint),
                    b => b.ResizeHeightTo(height, 500, Easing.OutQuint)
                );

                fadeIn.Finally(_ => fadeIn = null);
            }

            fadeOut?.Cancel();
            fadeOut = Scheduler.AddDelayed(() =>
            {
                toDisplay.Animate(
                    b => b.FadeOutFromOne(1500, Easing.InQuint),
                    b => b.ResizeHeightTo(height_contracted, 1500, Easing.InQuint));
            }, 500);
        }
    }
}
