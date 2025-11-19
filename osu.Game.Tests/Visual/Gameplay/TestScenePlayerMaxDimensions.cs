// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
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
    [Ignore("This test is for visual testing, and has no value in being run in standard CI runs.")]
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

            public ISkin FindProvider(Func<ISkin, bool> lookupFunction) => this;
            public IEnumerable<ISkin> AllSources => new[] { this };

            protected override IResourceStore<TextureUpload> CreateTextureLoaderStore(IStorageResourceProvider resources, IResourceStore<byte[]> storage)
                => new UpscaledTextureLoaderStore(base.CreateTextureLoaderStore(resources, storage));

            private class UpscaledTextureLoaderStore : IResourceStore<TextureUpload>
            {
                private readonly IResourceStore<TextureUpload>? textureStore;

                public UpscaledTextureLoaderStore(IResourceStore<TextureUpload>? textureStore)
                {
                    this.textureStore = textureStore;
                }

                public void Dispose()
                {
                    textureStore?.Dispose();
                }

                public TextureUpload Get(string name)
                {
                    var textureUpload = textureStore?.Get(name);

                    // NRT not enabled on framework side classes (IResourceStore / TextureLoaderStore), welp.
                    if (textureUpload == null)
                        return null!;

                    return upscale(textureUpload);
                }

                public async Task<TextureUpload> GetAsync(string name, CancellationToken cancellationToken = new CancellationToken())
                {
                    // NRT not enabled on framework side classes (IResourceStore / TextureLoaderStore), welp.
                    if (textureStore == null)
                        return null!;

                    var textureUpload = await textureStore.GetAsync(name, cancellationToken).ConfigureAwait(false);

                    if (textureUpload == null)
                        return null!;

                    return await Task.Run(() => upscale(textureUpload), cancellationToken).ConfigureAwait(false);
                }

                private TextureUpload upscale(TextureUpload textureUpload)
                {
                    var image = Image.LoadPixelData(textureUpload.Data, textureUpload.Width, textureUpload.Height);

                    // The original texture upload will no longer be returned or used.
                    textureUpload.Dispose();

                    image.Mutate(i => i.Resize(new Size(textureUpload.Width, textureUpload.Height) * scale_factor));
                    return new TextureUpload(image);
                }

                public Stream? GetStream(string name) => textureStore?.GetStream(name);

                public IEnumerable<string> GetAvailableResources() => textureStore?.GetAvailableResources() ?? Array.Empty<string>();
            }
        }
    }
}
