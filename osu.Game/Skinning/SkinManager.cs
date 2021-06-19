﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.IO.Archives;

namespace osu.Game.Skinning
{
    [ExcludeFromDynamicCompile]
    public class SkinManager : ArchiveModelManager<SkinInfo, SkinFileInfo>, ISkinSource, IStorageResourceProvider
    {
        private readonly AudioManager audio;

        private readonly GameHost host;

        private readonly IResourceStore<byte[]> resources;

        public readonly Bindable<Skin> CurrentSkin = new Bindable<Skin>();
        public readonly Bindable<SkinInfo> CurrentSkinInfo = new Bindable<SkinInfo>(SkinInfo.Default) { Default = SkinInfo.Default };

        public override IEnumerable<string> HandledExtensions => new[] { ".osk" };

        protected override string[] HashableFileTypes => new[] { ".ini" };

        protected override string ImportFromStablePath => "Skins";

        private readonly Skin defaultLegacySkin;

        private readonly Skin defaultSkin;

        public SkinManager(Storage storage, DatabaseContextFactory contextFactory, GameHost host, IResourceStore<byte[]> resources, AudioManager audio)
            : base(storage, contextFactory, new SkinStore(contextFactory, storage), host)
        {
            this.audio = audio;
            this.host = host;
            this.resources = resources;

            defaultLegacySkin = new DefaultLegacySkin(this);
            defaultSkin = new DefaultSkin(this);

            CurrentSkinInfo.ValueChanged += skin => CurrentSkin.Value = GetSkin(skin.NewValue);

            CurrentSkin.Value = defaultSkin;
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
            var randomChoices = GetAllUsableSkins().Where(s => s.ID != CurrentSkinInfo.Value.ID).ToArray();

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
            if (item.Name?.Contains(".osk", StringComparison.OrdinalIgnoreCase) == true)
                populateMetadata(item, GetSkin(item));

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
            await base.Populate(model, archive, cancellationToken).ConfigureAwait(false);

            var instance = GetSkin(model);

            model.InstantiationInfo ??= instance.GetType().GetInvariantInstantiationInfo();

            if (model.Name?.Contains(".osk", StringComparison.OrdinalIgnoreCase) == true)
                populateMetadata(model, instance);
        }

        private void populateMetadata(SkinInfo item, Skin instance)
        {
            if (!string.IsNullOrEmpty(instance.Configuration.SkinInfo.Name))
            {
                item.Name = instance.Configuration.SkinInfo.Name;
                item.Creator = instance.Configuration.SkinInfo.Creator;
            }
            else
            {
                item.Name = item.Name.Replace(".osk", "", StringComparison.OrdinalIgnoreCase);
                item.Creator ??= unknown_creator_string;
            }
        }

        /// <summary>
        /// Retrieve a <see cref="Skin"/> instance for the provided <see cref="SkinInfo"/>
        /// </summary>
        /// <param name="skinInfo">The skin to lookup.</param>
        /// <returns>A <see cref="Skin"/> instance correlating to the provided <see cref="SkinInfo"/>.</returns>
        public Skin GetSkin(SkinInfo skinInfo) => skinInfo.CreateInstance(this);

        /// <summary>
        /// Ensure that the current skin is in a state it can accept user modifications.
        /// This will create a copy of any internal skin and being tracking in the database if not already.
        /// </summary>
        public void EnsureMutableSkin()
        {
            if (CurrentSkinInfo.Value.ID >= 1) return;

            var skin = CurrentSkin.Value;

            // if the user is attempting to save one of the default skin implementations, create a copy first.
            CurrentSkinInfo.Value = Import(new SkinInfo
            {
                Name = skin.SkinInfo.Name + " (modified)",
                Creator = skin.SkinInfo.Creator,
                InstantiationInfo = skin.SkinInfo.InstantiationInfo,
            }).Result;
        }

        public void Save(Skin skin)
        {
            if (skin.SkinInfo.ID <= 0)
                throw new InvalidOperationException($"Attempting to save a skin which is not yet tracked. Call {nameof(EnsureMutableSkin)} first.");

            foreach (var drawableInfo in skin.DrawableComponentInfo)
            {
                string json = JsonConvert.SerializeObject(drawableInfo.Value, new JsonSerializerSettings { Formatting = Formatting.Indented });

                using (var streamContent = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    string filename = $"{drawableInfo.Key}.json";

                    var oldFile = skin.SkinInfo.Files.FirstOrDefault(f => f.Filename == filename);

                    if (oldFile != null)
                        ReplaceFile(skin.SkinInfo, oldFile, streamContent, oldFile.Filename);
                    else
                        AddFile(skin.SkinInfo, streamContent, filename);
                }
            }
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="SkinInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public SkinInfo Query(Expression<Func<SkinInfo, bool>> query) => ModelStore.ConsumableItems.AsNoTracking().FirstOrDefault(query);

        public event Action SourceChanged;

        public Drawable GetDrawableComponent(ISkinComponent component) => lookupWithFallback(s => s.GetDrawableComponent(component));

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => lookupWithFallback(s => s.GetTexture(componentName, wrapModeS, wrapModeT));

        public ISample GetSample(ISampleInfo sampleInfo) => lookupWithFallback(s => s.GetSample(sampleInfo));

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => lookupWithFallback(s => s.GetConfig<TLookup, TValue>(lookup));

        public ISkin FindProvider(Func<ISkin, bool> lookupFunction)
        {
            if (lookupFunction(CurrentSkin.Value))
                return CurrentSkin.Value;

            if (CurrentSkin.Value is LegacySkin && lookupFunction(defaultLegacySkin))
                return defaultLegacySkin;

            if (lookupFunction(defaultSkin))
                return defaultSkin;

            return null;
        }

        private T lookupWithFallback<T>(Func<ISkin, T> lookupFunction)
            where T : class
        {
            if (lookupFunction(CurrentSkin.Value) is T skinSourced)
                return skinSourced;

            // TODO: we also want to return a DefaultLegacySkin here if the current *beatmap* is providing any skinned elements.
            // When attempting to address this, we may want to move the full DefaultLegacySkin fallback logic to within Player itself (to better allow
            // for beatmap skin visibility).
            if (CurrentSkin.Value is LegacySkin && lookupFunction(defaultLegacySkin) is T legacySourced)
                return legacySourced;

            // Finally fall back to the (non-legacy) default.
            return lookupFunction(defaultSkin);
        }

        #region IResourceStorageProvider

        AudioManager IStorageResourceProvider.AudioManager => audio;
        IResourceStore<byte[]> IStorageResourceProvider.Resources => resources;
        IResourceStore<byte[]> IStorageResourceProvider.Files => Files.Store;
        IResourceStore<TextureUpload> IStorageResourceProvider.CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => host.CreateTextureLoaderStore(underlyingStore);

        #endregion
    }
}
