// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Overlays.Settings.Sections.DebugSettings
{
    public partial class MemorySettings : SettingsSubsection
    {
        protected override LocalisableString Header => @"Memory";

        [BackgroundDependencyLoader]
        private void load(GameHost host, RealmAccess realm)
        {
            SettingsButton blockAction;
            SettingsButton unblockAction;

            Add(new SettingsButton
            {
                Text = @"Clear all caches",
                Action = () =>
                {
                    host.Collect();

                    // host.Collect() uses GCCollectionMode.Optimized, but we should be as aggressive as possible here.
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
                }
            });

            SettingsEnumDropdown<GCLatencyMode> latencyModeDropdown;
            Add(latencyModeDropdown = new SettingsEnumDropdown<GCLatencyMode>
            {
                LabelText = "GC mode",
            });

            latencyModeDropdown.Current.BindValueChanged(mode =>
            {
                Logger.Log($"Changing latency mode: {mode.NewValue}");

                switch (mode.NewValue)
                {
                    case GCLatencyMode.Default:
                        // https://github.com/ppy/osu-framework/blob/1d5301018dfed1a28702be56e1d53c4835b199f2/osu.Framework/Platform/GameHost.cs#L703
                        GCSettings.LatencyMode = System.Runtime.GCLatencyMode.SustainedLowLatency;
                        break;

                    case GCLatencyMode.Interactive:
                        GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Interactive;
                        break;
                }
            });

            if (DebugUtils.IsDebugBuild)
            {
                AddRange(new Drawable[]
                {
                    new SettingsButton
                    {
                        Text = @"Compact realm",
                        Action = () =>
                        {
                            // Blocking operations implicitly causes a Compact().
                            using (realm.BlockAllOperations(@"compact"))
                            {
                            }
                        }
                    },
                    blockAction = new SettingsButton
                    {
                        Text = @"Block realm",
                    },
                    unblockAction = new SettingsButton
                    {
                        Text = @"Unblock realm",
                    }
                });

                blockAction.Action = () =>
                {
                    try
                    {
                        IDisposable? token = realm.BlockAllOperations(@"maintenance");

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
                            if (token.IsNull())
                                return;

                            token.Dispose();
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
                        Logger.Error(e, @"Blocking realm failed");
                    }
                };
            }
        }

        private enum GCLatencyMode
        {
            Default,
            Interactive,
        }
    }
}
