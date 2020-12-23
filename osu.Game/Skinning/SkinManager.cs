// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.IO.Archives;

namespace osu.Game.Skinning
{
    [ExcludeFromDynamicCompile]
    public class SkinManager : ArchiveModelManager<SkinInfo, SkinFileInfo>, ISkinSource, IStorageResourceProvider
    {
        private readonly AudioManager audio;

        private readonly GameHost host;

        private readonly IResourceStore<byte[]> legacyDefaultResources;

        public readonly Bindable<Skin> CurrentSkin = new Bindable<Skin>(new DefaultSkin());
        public readonly Bindable<SkinInfo> CurrentSkinInfo = new Bindable<SkinInfo>(SkinInfo.Default) { Default = SkinInfo.Default };

        public override IEnumerable<string> HandledExtensions => new[] { ".osk" };

        protected override string[] HashableFileTypes => new[] { ".ini" };

        protected override string ImportFromStablePath => "Skins";

        public SkinManager(Storage storage, DatabaseContextFactory contextFactory, GameHost host, AudioManager audio, IResourceStore<byte[]> legacyDefaultResources)
            : base(storage, contextFactory, new SkinStore(contextFactory, storage), host)
        {
            this.audio = audio;
            this.host = host;

            this.legacyDefaultResources = legacyDefaultResources;

            CurrentSkinInfo.ValueChanged += skin => CurrentSkin.Value = GetSkin(skin.NewValue);
            CurrentSkin.ValueChanged += skin =>
            {
                if (skin.NewValue.SkinInfo != CurrentSkinInfo.Value)
                    throw new InvalidOperationException($"Setting {nameof(CurrentSkin)}'s value directly is not supported. Use {nameof(CurrentSkinInfo)} instead.");

                SourceChanged?.Invoke();
            };
        }

        protected override bool ShouldDeleteArchive(string path) => Path.GetExtension(path)?.ToLowerInvariant() == ".osk";

        /// <summary>
        /// Returns a list of all usable <see cref="SkinInfo"/>s. Includes the special default skin plus all skins from <see cref="GetAllUserSkins"/>.
        /// </summary>
        /// <returns>A newly allocated list of available <see cref="SkinInfo"/>.</returns>
        public List<SkinInfo> GetAllUsableSkins()
        {
            var userSkins = GetAllUserSkins();
            userSkins.Insert(0, SkinInfo.Default);
            userSkins.Insert(1, DefaultLegacySkin.Info);
            return userSkins;
        }

        /// <summary>
        /// Returns a list of all usable <see cref="SkinInfo"/>s that have been loaded by the user.
        /// </summary>
        /// <returns>A newly allocated list of available <see cref="SkinInfo"/>.</returns>
        public List<SkinInfo> GetAllUserSkins() => ModelStore.ConsumableItems.Where(s => !s.DeletePending).ToList();

        public void SelectRandomSkin()
        {
            // choose from only user skins, removing the current selection to ensure a new one is chosen.
            var randomChoices = GetAllUsableSkins().Where(s => s.ID > 0 && s.ID != CurrentSkinInfo.Value.ID).ToArray();

            if (randomChoices.Length == 0)
            {
                CurrentSkinInfo.Value = SkinInfo.Default;
                return;
            }

            CurrentSkinInfo.Value = randomChoices.ElementAt(RNG.Next(0, randomChoices.Length));
        }

        protected override SkinInfo CreateModel(ArchiveReader archive) => new SkinInfo { Name = archive.Name };

        private const string unknown_creator_string = "Unknown";

        protected override string ComputeHash(SkinInfo item, ArchiveReader reader = null)
        {
            // we need to populate early to create a hash based off skin.ini contents
            if (item.Name?.Contains(".osk") == true)
                populateMetadata(item);

            if (item.Creator != null && item.Creator != unknown_creator_string)
            {
                // this is the optimal way to hash legacy skins, but will need to be reconsidered when we move forward with skin implementation.
                // likely, the skin should expose a real version (ie. the version of the skin, not the skin.ini version it's targeting).
                return item.ToString().ComputeSHA2Hash();
            }

            // if there was no creator, the ToString above would give the filename, which alone isn't really enough to base any decisions on.
            return base.ComputeHash(item, reader);
        }

        protected override async Task Populate(SkinInfo model, ArchiveReader archive, CancellationToken cancellationToken = default)
        {
            await base.Populate(model, archive, cancellationToken);

            if (model.Name?.Contains(".osk") == true)
                populateMetadata(model);
        }

        private void populateMetadata(SkinInfo item)
        {
            Skin reference = GetSkin(item);

            if (!string.IsNullOrEmpty(reference.Configuration.SkinInfo.Name))
            {
                item.Name = reference.Configuration.SkinInfo.Name;
                item.Creator = reference.Configuration.SkinInfo.Creator;
            }
            else
            {
                item.Name = item.Name.Replace(".osk", "");
                item.Creator ??= unknown_creator_string;
            }
        }

        /// <summary>
        /// Retrieve a <see cref="Skin"/> instance for the provided <see cref="SkinInfo"/>
        /// </summary>
        /// <param name="skinInfo">The skin to lookup.</param>
        /// <returns>A <see cref="Skin"/> instance correlating to the provided <see cref="SkinInfo"/>.</returns>
        public Skin GetSkin(SkinInfo skinInfo)
        {
            if (skinInfo == SkinInfo.Default)
                return new DefaultSkin();

            if (skinInfo == DefaultLegacySkin.Info)
                return new DefaultLegacySkin(legacyDefaultResources, this);

            return new LegacySkin(skinInfo, this);
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="SkinInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public SkinInfo Query(Expression<Func<SkinInfo, bool>> query) => ModelStore.ConsumableItems.AsNoTracking().FirstOrDefault(query);

        public event Action SourceChanged;

        public Drawable GetDrawableComponent(ISkinComponent component) => CurrentSkin.Value.GetDrawableComponent(component);

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => CurrentSkin.Value.GetTexture(componentName, wrapModeS, wrapModeT);

        public SampleChannel GetSample(ISampleInfo sampleInfo) => CurrentSkin.Value.GetSample(sampleInfo);

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => CurrentSkin.Value.GetConfig<TLookup, TValue>(lookup);

        #region IResourceStorageProvider

        AudioManager IStorageResourceProvider.AudioManager => audio;
        IResourceStore<byte[]> IStorageResourceProvider.Files => Files.Store;
        IResourceStore<TextureUpload> IStorageResourceProvider.CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => host.CreateTextureLoaderStore(underlyingStore);

        #endregion
    }
}
