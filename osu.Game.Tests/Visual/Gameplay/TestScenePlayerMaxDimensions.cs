// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace osu.Game.Tests.Visual.Gameplay
{
    /// <summary>
    /// Upscales all gameplay sprites by a huge amount, to aid in manually checking skin texture size limits
    /// on individual elements.
    /// </summary>
    /// <remarks>
    /// The HUD is hidden as it does't really affect game balance if HUD elements are larger than they should be.
    /// </remarks>
    public partial class TestScenePlayerMaxDimensions : TestSceneAllRulesetPlayers
    {
        // scale textures to 4 times their size.
        private const int scale_factor = 4;

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
                    texture.ScaleAdjust /= scale_factor;

                return texture;
            }

            public ISkin FindProvider(Func<ISkin, bool> lookupFunction) => this;
            public IEnumerable<ISkin> AllSources => new[] { this };
        }
    }
}
