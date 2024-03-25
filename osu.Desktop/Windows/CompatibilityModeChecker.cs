// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Win32;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Desktop.Windows
{
    /// <summary>
    /// Checks if the game is running with windows compatibility optimizations which could cause issues. Displays a warning notification if so.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public partial class CompatibilityModeChecker : Component
    {
        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            if (!CheckCompatibilityMode()) return;

            notifications.Post(new CompatibilityModeNotification());
            LogCompatibilityFlags();
        }

        /// <summary>
        /// Check if the game is running with windows compatibility optimizations
        /// </summary>
        /// <returns></returns>
        public static bool CheckCompatibilityMode()
        {
            using var layers = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");

            return layers != null && layers.GetValueNames().Any(name => name.Equals(Environment.ProcessPath, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Log the compatibility flags for the current process if they exist
        /// </summary>
        public static void LogCompatibilityFlags()
        {
            using var layers = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");

            if (layers?.GetValue(Environment.ProcessPath) is string flags)
                Logger.Log($"Compatibility flags for {Environment.ProcessPath}: {flags}", LoggingTarget.Information);
        }

        private partial class CompatibilityModeNotification : SimpleNotification
        {
            public override bool IsImportant => true;

            public CompatibilityModeNotification()
            {
                Text = "osu! is running in compatibility mode. This may cause issues with the game. Please ensure osu! is not set to run in compatibility mode.";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Icon = FontAwesome.Solid.ShieldAlt;
                IconContent.Colour = colours.YellowDark;
            }
        }
    }
}
