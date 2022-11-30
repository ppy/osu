// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;

namespace osu.Game.Input
{
    public class RulesetKeyBinderChecker : Container
    {
        public Action<Notification>? PostNotification { get; set; }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesetStore, RealmAccess realm)
        {
            foreach (var rulesetInfo in rulesetStore.AvailableRulesets)
            {
                var r = rulesetInfo.CreateInstance();

                foreach (var variant in r.AvailableVariants)
                {
                    var defaults = r.GetDefaultKeyBindings(variant).GroupBy(d => d.Action).SelectMany(g => g).ToList();

                    var bindings = realm.Run(r => r.All<RealmKeyBinding>()
                                                   .Where(b => b.RulesetName == rulesetInfo.ShortName && b.Variant == variant)
                                                   .Detach()
                                                   .OrderBy(b => defaults.FindIndex(d => (int)d.Action == b.ActionInt)).ToList());

                    int keyBindingsUniqueCount = bindings.Select(b => b.KeyCombination).Distinct().Count();
                    string variantName = r.GetVariantName(variant).ToString();

                    if (bindings.Count() != keyBindingsUniqueCount)
                    {
                        post(rulesetInfo.Name, r.GetVariantName(variant).ToString());

                        int i = 0;

                        foreach (var d in defaults.Select(d => d.KeyCombination))
                        {
                            var b = bindings[i++];

                            b.KeyCombination = d;
                            realm.WriteAsync(r => r.Find<RealmKeyBinding>(b.ID).KeyCombinationString = b.KeyCombinationString);
                        }
                    }
                }
            }
        }

        private void post(string rulesetName, string variantName)
        {
            variantName = string.IsNullOrEmpty(variantName) ? "" : variantName + " ";

            Schedule(() => PostNotification?.Invoke(new SimpleErrorNotification
            {
                Icon = FontAwesome.Solid.ExclamationCircle,
                Text = $"{rulesetName} {variantName}bindings set to default.",
            }));
        }
    }
}