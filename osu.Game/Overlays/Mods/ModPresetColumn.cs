// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;
using Realms;

namespace osu.Game.Overlays.Mods
{
    public class ModPresetColumn : ModSelectColumn
    {
        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Orange1;
            HeaderText = ModSelectOverlayStrings.PersonalPresets;

            AddPresetButton addPresetButton;
            ItemsFlow.Add(addPresetButton = new AddPresetButton());
            ItemsFlow.SetLayoutPosition(addPresetButton, float.PositiveInfinity);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(_ => rulesetChanged(), true);
        }

        private IDisposable? presetSubscription;

        private void rulesetChanged()
        {
            presetSubscription?.Dispose();
            presetSubscription = realm.RegisterForNotifications(r =>
                    r.All<ModPreset>()
                     .Filter($"{nameof(ModPreset.Ruleset)}.{nameof(RulesetInfo.ShortName)} == $0"
                             + $" && {nameof(ModPreset.DeletePending)} == false", ruleset.Value.ShortName)
                     .OrderBy(preset => preset.Name),
                (presets, _, _) => asyncLoadPanels(presets));
        }

        private CancellationTokenSource? cancellationTokenSource;

        private Task? latestLoadTask;
        internal bool ItemsLoaded => latestLoadTask == null;

        private void asyncLoadPanels(IReadOnlyList<ModPreset> presets)
        {
            cancellationTokenSource?.Cancel();

            if (!presets.Any())
            {
                ItemsFlow.RemoveAll(panel => panel is ModPresetPanel);
                return;
            }

            var panels = presets.Select(preset => new ModPresetPanel(preset.ToLive(realm))
            {
                Shear = Vector2.Zero
            });

            Task? loadTask;

            latestLoadTask = loadTask = LoadComponentsAsync(panels, loaded =>
            {
                ItemsFlow.RemoveAll(panel => panel is ModPresetPanel);
                ItemsFlow.AddRange(loaded);
            }, (cancellationTokenSource = new CancellationTokenSource()).Token);
            loadTask.ContinueWith(_ =>
            {
                if (loadTask == latestLoadTask)
                    latestLoadTask = null;
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            presetSubscription?.Dispose();
        }
    }
}
