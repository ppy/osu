// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Updater
{
    [SupportedOSPlatform("windows")]
    public partial class AssociationManager : CompositeDrawable
    {
        // Needed so that Explorer windows get refreshed after the registry is updated
        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        /// <summary>
        /// A dictionary of extensions and their descriptions
        /// </summary>
        private static readonly Dictionary<string, string> associated_extensions = new Dictionary<string, string>
        {
            { ".osz", "osu! beatmap" },
            { ".osr", "osu! replay" },
            { ".osk", "osu! skin" },
            { ".osu", "osu! difficulty" },
            { ".olz", "osu! beatmap" }
        };

        [Resolved]
        protected INotificationOverlay Notifications { get; private set; } = null!;

        // TODO: Somehow make this run on initial setup without prompting the user
        /// <summary>
        /// This checks the integrity of the file associations. If they look out of place, it will prompt the user via notifications to fix them.
        /// </summary>
        [BackgroundDependencyLoader]
        private void load()
        {
            Logger.Log("Checking file associations");

            bool fileAssociationStatus = EnsureAssociationsSet();

            if (!fileAssociationStatus)
            {
                // Files are not associated, prompt the user to fix them
                Notifications.Post(new SimpleNotification
                {
                    Icon = FontAwesome.Solid.ExclamationCircle,
                    Text = "File associations are not set up correctly! Do you want to fix them?",
                    Activated = () =>
                    {
                        InitializeFileAssociations();
                        Notifications.Post(new SimpleNotification
                        {
                            Icon = FontAwesome.Solid.CheckCircle,
                            Text = "File associations have been fixed!",
                        });
                        return true;
                    }
                });
            }
        }

        /// <summary>
        /// Initializes (creates) the file associations
        /// </summary>
        public void InitializeFileAssociations()
        {
            Logger.Log("Setting up file associations!");
            string programPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\osu!.exe";

            foreach (string extension in associated_extensions.Keys)
            {
                CreateFileAssociation(extension, associated_extensions[extension], programPath);
            }

            SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero); // Notify Explorer that the file associations have changed
        }

        /// <summary>
        /// Create an association for a specific extension
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="description"></param>
        /// <param name="programPath"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void CreateFileAssociation(string extension, string description, string programPath)
        {
            Logger.Log("Creating file association for " + extension);

            // Check if the extension has a file explorer override, if so, delete the override
            RegistryKey? fileExplorerKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + extension, true);

            if (fileExplorerKey != null)
            {
                fileExplorerKey.DeleteSubKey("UserChoice", false);
                fileExplorerKey.Close();
                Logger.Log("Deleted file explorer override for " + extension);
            }
            else
            {
                Logger.Log("File explorer override for " + extension + " does not exist!");
            }

            RegistryKey key = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{extension}");
            key.SetValue("", description);
            key.CreateSubKey("shell\\\\open\\\\command").SetValue("", $"\"{programPath}\" \"%1\"");
            key.Close();
            Logger.Log("Created file association for " + extension);
        }

        public void RemoveFileAssociation(string extension)
        {
            Logger.Log("Removing file association for " + extension);
            RegistryKey? key = Registry.CurrentUser.OpenSubKey($"Software\\Classes\\{extension}");

            if (key != null)
            {
                Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{extension}");
                Logger.Log("Removed file association for " + extension);
            }
            else
            {
                Logger.Log("File association for " + extension + " does not exist!");
            }
        }

        /// <summary>
        /// Removes all of the file associations
        /// </summary>
        public void RemoveFileAssociations()
        {
            foreach (string extension in associated_extensions.Keys)
            {
                RemoveFileAssociation(extension);
            }

            SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero); // Notify Explorer that the file associations have changed
        }

        public bool IsAssociated(string extension, string programPath)
        {
            RegistryKey? key = Registry.CurrentUser.OpenSubKey($"Software\\Classes\\{extension}");

            // Check if the value set is equal to the path of the executable
            return key?.OpenSubKey("shell\\\\open\\\\command")?.GetValue("")?.ToString() == $"\"{programPath}\" \"%1\"";
        }

        /// <summary>
        /// Ensures that the file associations are valid
        /// </summary>
        /// <returns>
        /// Whether the file associations are valid or not
        /// </returns>
        public bool EnsureAssociationsSet()
        {
            string programPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\osu!.exe";

            // Loop through all of the extensions and check if they are associated
            return associated_extensions.All(extension => IsAssociated(extension.Key, programPath));
        }
    }
}
