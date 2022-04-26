// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.DebugSettings
{
    public class MemorySettings : SettingsSubsection
    {
        protected override LocalisableString Header => DebugSettingsStrings.MemoryHeader;

        [BackgroundDependencyLoader]
        private void load(GameHost host, RealmAccess realm)
        {
            SettingsButton blockAction;
            SettingsButton unblockAction;

            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = DebugSettingsStrings.ClearAllCaches,
                    Action = host.Collect
                },
                new SettingsButton
                {
                    Text = DebugSettingsStrings.CompactRealm,
                    Action = () =>
                    {
                        // Blocking operations implicitly causes a Compact().
                        using (realm.BlockAllOperations())
                        {
                        }
                    }
                },
                blockAction = new SettingsButton
                {
                    Text = "Block realm",
                },
                unblockAction = new SettingsButton
                {
                    Text = "Unblock realm",
                },
            };

            blockAction.Action = () =>
            {
                try
                {
                    var token = realm.BlockAllOperations();

                    blockAction.Enabled.Value = false;

                    // As a safety measure, unblock after 10 seconds.
                    // This is to handle the case where a dev may block, but then something on the update thread
                    // accesses realm and blocks for eternity.
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(10000);
                        unblock();
                    });

                    unblockAction.Action = unblock;

                    void unblock()
                    {
                        if (token == null)
                            return;

                        token?.Dispose();
                        token = null;

                        Scheduler.Add(() =>
                        {
                            blockAction.Enabled.Value = true;
                            unblockAction.Action = null;
                        });
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Blocking realm failed");
                }
            };
        }
    }
}
