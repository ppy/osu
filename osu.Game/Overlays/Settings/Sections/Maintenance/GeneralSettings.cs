﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class GeneralSettings : SettingsSubsection
    {
        protected override string Header => "General";

        private TriangleButton importBeatmapsButton;
        private TriangleButton importSkinsButton;
        private TriangleButton deleteSkinsButton;
        private TriangleButton deleteBeatmapsButton;
        private TriangleButton restoreButton;
        private TriangleButton undeleteButton;

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps, SkinManager skins, DialogOverlay dialogOverlay)
        {
            Children = new Drawable[]
            {
                importBeatmapsButton = new SettingsButton
                {
                    Text = "Import beatmaps from stable",
                    Action = () =>
                    {
                        importBeatmapsButton.Enabled.Value = false;
                        beatmaps.ImportFromStableAsync().ContinueWith(t => Schedule(() => importBeatmapsButton.Enabled.Value = true));
                    }
                },
                deleteBeatmapsButton = new DangerousSettingsButton
                {
                    Text = "Delete ALL beatmaps",
                    Action = () =>
                    {
                        dialogOverlay?.Push(new DeleteAllBeatmapsDialog(() =>
                        {
                            deleteBeatmapsButton.Enabled.Value = false;
                            Task.Run(() => beatmaps.Delete(beatmaps.GetAllUsableBeatmapSets())).ContinueWith(t => Schedule(() => deleteBeatmapsButton.Enabled.Value = true));
                        }));
                    }
                },
                importSkinsButton = new SettingsButton
                {
                    Text = "Import skins from stable",
                    Action = () =>
                    {
                        importSkinsButton.Enabled.Value = false;
                        skins.ImportFromStableAsync().ContinueWith(t => Schedule(() => importSkinsButton.Enabled.Value = true));
                    }
                },
                deleteSkinsButton = new DangerousSettingsButton
                {
                    Text = "Delete ALL skins",
                    Action = () =>
                    {
                        dialogOverlay?.Push(new DeleteAllBeatmapsDialog(() =>
                        {
                            deleteSkinsButton.Enabled.Value = false;
                            Task.Run(() => skins.Delete(skins.GetAllUserSkins())).ContinueWith(t => Schedule(() => deleteSkinsButton.Enabled.Value = true));
                        }));
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
                undeleteButton = new SettingsButton
                {
                    Text = "Restore all recently deleted beatmaps",
                    Action = () =>
                    {
                        undeleteButton.Enabled.Value = false;
                        Task.Run(() => beatmaps.Undelete(beatmaps.QueryBeatmapSets(b => b.DeletePending).ToList())).ContinueWith(t => Schedule(() => undeleteButton.Enabled.Value = true));
                    }
                },
            };
        }
    }
}
