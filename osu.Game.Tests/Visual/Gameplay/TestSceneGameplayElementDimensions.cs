// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneGameplayElementDimensions : TestSceneAllRulesetPlayers
    {
        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            // for now this only applies to legacy skins, as modern skins don't have texture-based gameplay elements yet.
            dependencies.CacheAs<ISkinSource>(new UpscaledLegacySkin(dependencies.Get<SkinManager>()));

            return dependencies;
        }

        protected override void AddCheckSteps()
        {
        }

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            var player = base.CreatePlayer(ruleset);
            player.OnLoadComplete += _ =>
            {
                // this test scene focuses on gameplay elements, so let's hide the hud.
                var hudOverlay = player.ChildrenOfType<HUDOverlay>().Single();
                hudOverlay.ShowHud.Value = false;
                hudOverlay.ShowHud.Disabled = true;
            };
            return player;
        }

        private class UpscaledLegacySkin : DefaultLegacySkin, ISkinSource
        {
            public UpscaledLegacySkin(IStorageResourceProvider resources)
                : base(resources)
            {
            }

            public event Action? SourceChanged
            {
                add { }
                remove { }
            }

            public override Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            {
                var texture = base.GetTexture(componentName, wrapModeS, wrapModeT);

                if (texture != null)
                    texture.ScaleAdjust /= 5f;

                return texture;
            }

            public ISkin FindProvider(Func<ISkin, bool> lookupFunction) => this;
            public IEnumerable<ISkin> AllSources => new[] { this };
        }
    }
}
