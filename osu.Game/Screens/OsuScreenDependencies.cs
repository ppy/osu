// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens
{
    public class OsuScreenDependencies : DependencyContainer
    {
        public Bindable<WorkingBeatmap> Beatmap { get; }

        public Bindable<RulesetInfo> Ruleset { get; }

        public Bindable<IEnumerable<Mod>> SelectedMods { get; }

        public OsuScreenDependencies(bool requireLease, IReadOnlyDependencyContainer parent)
            : base(parent)
        {
            if (requireLease)
            {
                Beatmap = parent.Get<LeasedBindable<WorkingBeatmap>>()?.GetBoundCopy();
                if (Beatmap == null)
                    Cache(Beatmap = parent.Get<Bindable<WorkingBeatmap>>().BeginLease(false));

                Ruleset = parent.Get<LeasedBindable<RulesetInfo>>()?.GetBoundCopy();
                if (Ruleset == null)
                    Cache(Ruleset = parent.Get<Bindable<RulesetInfo>>().BeginLease(true));

                SelectedMods = parent.Get<LeasedBindable<IEnumerable<Mod>>>()?.GetBoundCopy();
                if (SelectedMods == null)
                    Cache(SelectedMods = parent.Get<Bindable<IEnumerable<Mod>>>().BeginLease(true));
            }
            else
            {
                Beatmap = (parent.Get<LeasedBindable<WorkingBeatmap>>() ?? parent.Get<Bindable<WorkingBeatmap>>()).GetBoundCopy();
                Ruleset = (parent.Get<LeasedBindable<RulesetInfo>>() ?? parent.Get<Bindable<RulesetInfo>>()).GetBoundCopy();
                SelectedMods = (parent.Get<LeasedBindable<IEnumerable<Mod>>>() ?? parent.Get<Bindable<IEnumerable<Mod>>>()).GetBoundCopy();
            }
        }
    }
}
