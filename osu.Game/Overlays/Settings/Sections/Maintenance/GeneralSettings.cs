// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class GeneralSettings : SettingsSubsection
    {
        private TriangleButton importButton;
        private TriangleButton deleteButton;
        private TriangleButton restoreButton;

        protected override string Header => "General";

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps)
        {
            Children = new Drawable[]
            {
                importButton = new SettingsButton
                {
                    Text = "Import beatmaps from stable",
                    Action = () =>
                    {
                        importButton.Enabled.Value = false;
                        Task.Factory.StartNew(beatmaps.ImportFromStable)
                            .ContinueWith(t => Schedule(() => importButton.Enabled.Value = true), TaskContinuationOptions.LongRunning);
                    }
                },
                deleteButton = new SettingsButton
                {
                    Text = "Delete ALL beatmaps",
                    Action = () =>
                    {
                        deleteButton.Enabled.Value = false;
                        Task.Run(() => beatmaps.DeleteAll()).ContinueWith(t => Schedule(() => deleteButton.Enabled.Value = true));
                    }
                },
                restoreButton = new SettingsButton
                {
                    Text = "Restore all hidden difficulties",
                    Action = () =>
                    {
                        restoreButton.Enabled.Value = false;
                        Task.Run(() =>
                        {
                            foreach (var b in beatmaps.QueryBeatmaps(b => b.Hidden).ToList())
                                beatmaps.Restore(b);
                        }).ContinueWith(t => Schedule(() => restoreButton.Enabled.Value = true));
                    }
                },
            };
        }
    }
}
