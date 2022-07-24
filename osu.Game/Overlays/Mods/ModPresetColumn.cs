// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public class ModPresetColumn : ModSelectColumn
    {
        private IReadOnlyList<ModPreset> presets = Array.Empty<ModPreset>();

        /// <summary>
        /// Sets the collection of available mod presets.
        /// </summary>
        public IReadOnlyList<ModPreset> Presets
        {
            get => presets;
            set
            {
                presets = value;

                if (IsLoaded)
                    asyncLoadPanels();
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Orange1;
            HeaderText = ModSelectOverlayStrings.PersonalPresets;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            asyncLoadPanels();
        }

        private CancellationTokenSource? cancellationTokenSource;

        private Task? latestLoadTask;
        internal bool ItemsLoaded => latestLoadTask == null;

        private void asyncLoadPanels()
        {
            cancellationTokenSource?.Cancel();

            var panels = presets.Select(preset => new ModPresetPanel(preset)
            {
                Shear = Vector2.Zero
            });

            Task? loadTask;

            latestLoadTask = loadTask = LoadComponentsAsync(panels, loaded =>
            {
                ItemsFlow.ChildrenEnumerable = loaded;
            }, (cancellationTokenSource = new CancellationTokenSource()).Token);
            loadTask.ContinueWith(_ =>
            {
                if (loadTask == latestLoadTask)
                    latestLoadTask = null;
            });
        }
    }
}
