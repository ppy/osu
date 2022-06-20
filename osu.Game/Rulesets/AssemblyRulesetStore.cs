// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Platform;

namespace osu.Game.Rulesets
{
    /// <summary>
    /// A ruleset store that populates from loaded assemblies (and optionally, assemblies in a storage).
    /// </summary>
    public class AssemblyRulesetStore : RulesetStore
    {
        public override IEnumerable<RulesetInfo> AvailableRulesets => availableRulesets;

        private readonly List<RulesetInfo> availableRulesets = new List<RulesetInfo>();

        /// <summary>
        /// Create an assembly ruleset store that populates from loaded assemblies and an external location.
        /// </summary>
        /// <param name="path">An path containing ruleset DLLs.</param>
        public AssemblyRulesetStore(string path)
            : this(new NativeStorage(path))
        {
        }

        /// <summary>
        /// Create an assembly ruleset store that populates from loaded assemblies and an optional storage source.
        /// </summary>
        /// <param name="storage">An optional storage containing ruleset DLLs.</param>
        public AssemblyRulesetStore(Storage? storage = null)
            : base(storage)

        {
            List<Ruleset> instances = LoadedAssemblies.Values
                                                      .Select(r => Activator.CreateInstance(r) as Ruleset)
                                                      .Where(r => r != null)
                                                      .Select(r => r.AsNonNull())
                                                      .ToList();

            // add all legacy rulesets first to ensure they have exclusive choice of primary key.
            foreach (var r in instances.Where(r => r is ILegacyRuleset))
                availableRulesets.Add(new RulesetInfo(r.RulesetInfo.ShortName, r.RulesetInfo.Name, r.RulesetInfo.InstantiationInfo, r.RulesetInfo.OnlineID));
        }
    }
}
