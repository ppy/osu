// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public abstract partial class ManiaInputTestScene : OsuTestScene
    {
        private readonly Container<Drawable>? content;

        protected override Container<Drawable> Content => content ?? base.Content;

        protected ManiaInputTestScene(int keys)
        {
            base.Content.Add(content = new LocalInputManager(keys));
        }

        private partial class LocalInputManager : ManiaInputManager
        {
            public LocalInputManager(int variant)
                : base(new ManiaRuleset().RulesetInfo, variant)
            {
            }

            protected override KeyBindingContainer<ManiaAction> CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                => new LocalKeyBindingContainer(ruleset, variant, unique);

            private partial class LocalKeyBindingContainer : RulesetKeyBindingContainer
            {
                public LocalKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                    : base(ruleset, variant, unique)
                {
                }

                protected override void ReloadMappings(IQueryable<RealmKeyBinding> realmKeyBindings)
                {
                    KeyBindings = DefaultKeyBindings;
                }
            }
        }
    }
}
