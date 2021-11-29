// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
using osu.Game.Overlays.Notifications;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Handles the storage and retrieval of <see cref="Skin"/>s.
    /// </summary>
    /// <remarks>
    /// This is also exposed and cached as <see cref="ISkinSource"/> to allow for any component to potentially have skinning support.
    /// For gameplay components, see <see cref="RulesetSkinProvidingContainer"/> which adds extra legacy and toggle logic that may affect the lookup process.
    /// </remarks>
    [ExcludeFromDynamicCompile]
    public class SkinManager : ISkinSource, IStorageResourceProvider, IModelImporter<SkinInfo>, IModelManager<SkinInfo>
    {
        private readonly AudioManager audio;

        private readonly GameHost host;

        private readonly IResourceStore<byte[]> resources;

        public readonly Bindable<Skin> CurrentSkin = new Bindable<Skin>();
        public readonly Bindable<ILive<SkinInfo>> CurrentSkinInfo = new Bindable<ILive<SkinInfo>>(SkinInfo.Default.ToLive()) { Default = SkinInfo.Default.ToLive() };

        private readonly SkinModelManager skinModelManager;

        private readonly SkinStore skinStore;

        private readonly IResourceStore<byte[]> userFiles;

        /// <summary>
        /// The default skin.
        /// </summary>
        public Skin DefaultSkin { get; }

        /// <summary>
        /// The default legacy skin.
        /// </summary>
        public Skin DefaultLegacySkin { get; }

        public SkinManager(Storage storage, DatabaseContextFactory contextFactory, GameHost host, IResourceStore<byte[]> resources, AudioManager audio)
        {
            this.audio = audio;
            this.host = host;
            this.resources = resources;

            skinStore = new SkinStore(contextFactory, storage);
            userFiles = new FileStore(contextFactory, storage).Store;

            skinModelManager = new SkinModelManager(storage, contextFactory, skinStore, host, this);

            DefaultLegacySkin = new DefaultLegacySkin(this);
            DefaultSkin = new DefaultSkin(this);

            CurrentSkinInfo.ValueChanged += skin => CurrentSkin.Value = GetSkin(skin.NewValue);

            CurrentSkin.Value = DefaultSkin;
            CurrentSkin.ValueChanged += skin =>
            {
                if (skin.NewValue.SkinInfo != CurrentSkinInfo.Value)
                    throw new InvalidOperationException($"Setting {nameof(CurrentSkin)}'s value directly is not supported. Use {nameof(CurrentSkinInfo)} instead.");

                SourceChanged?.Invoke();
            };
        }

        /// <summary>
        /// Returns a list of all usable <see cref="SkinInfo"/>s. Includes the special default skin plus all skins from <see cref="GetAllUserSkins"/>.
        /// </summary>
        /// <returns>A newly allocated list of available <see cref="SkinInfo"/>.</returns>
        public List<SkinInfo> GetAllUsableSkins()
        {
            var userSkins = GetAllUserSkins();
            userSkins.Insert(0, DefaultSkin.SkinInfo);
            userSkins.Insert(1, DefaultLegacySkin.SkinInfo);
            return userSkins;
        }

        /// <summary>
        /// Returns a list of all usable <see cref="SkinInfo"/>s that have been loaded by the user.
        /// </summary>
        /// <returns>A newly allocated list of available <see cref="SkinInfo"/>.</returns>
        public List<SkinInfo> GetAllUserSkins(bool includeFiles = false)
        {
            if (includeFiles)
                return skinStore.ConsumableItems.Where(s => !s.DeletePending).ToList();

            return skinStore.Items.Where(s => !s.DeletePending).ToList();
        }

        public void SelectRandomSkin()
        {
            // choose from only user skins, removing the current selection to ensure a new one is chosen.
            var randomChoices = skinStore.Items.Where(s => !s.DeletePending && s.ID != CurrentSkinInfo.Value.ID).ToArray();

            if (randomChoices.Length == 0)
            {
                CurrentSkinInfo.Value = SkinInfo.Default;
                return;
            }

            var chosen = randomChoices.ElementAt(RNG.Next(0, randomChoices.Length));
            CurrentSkinInfo.Value = skinStore.ConsumableItems.Single(i => i.ID == chosen.ID);
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
            CurrentSkinInfo.Value = skinModelManager.Import(new SkinInfo
            {
                Name = skin.SkinInfo.Name + @" (modified)",
                Creator = skin.SkinInfo.Creator,
                InstantiationInfo = skin.SkinInfo.InstantiationInfo,
            }).Result.Value;
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
                    string filename = @$"{drawableInfo.Key}.json";

                    var oldFile = skin.SkinInfo.Files.FirstOrDefault(f => f.Filename == filename);

                    if (oldFile != null)
                        skinModelManager.ReplaceFile(skin.SkinInfo, oldFile, streamContent);
                    else
                        skinModelManager.AddFile(skin.SkinInfo, streamContent, filename);
                }
            }
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="SkinInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public SkinInfo Query(Expression<Func<SkinInfo, bool>> query) => skinStore.ConsumableItems.AsNoTracking().FirstOrDefault(query);

        public event Action SourceChanged;

        public Drawable GetDrawableComponent(ISkinComponent component) => lookupWithFallback(s => s.GetDrawableComponent(component));

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => lookupWithFallback(s => s.GetTexture(componentName, wrapModeS, wrapModeT));

        public ISample GetSample(ISampleInfo sampleInfo) => lookupWithFallback(s => s.GetSample(sampleInfo));

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => lookupWithFallback(s => s.GetConfig<TLookup, TValue>(lookup));

        public ISkin FindProvider(Func<ISkin, bool> lookupFunction)
        {
            foreach (var source in AllSources)
            {
                if (lookupFunction(source))
                    return source;
            }

            return null;
        }

        public IEnumerable<ISkin> AllSources
        {
            get
            {
                yield return CurrentSkin.Value;

                if (CurrentSkin.Value is LegacySkin && CurrentSkin.Value != DefaultLegacySkin)
                    yield return DefaultLegacySkin;

                if (CurrentSkin.Value != DefaultSkin)
                    yield return DefaultSkin;
            }
        }

        private T lookupWithFallback<T>(Func<ISkin, T> lookupFunction)
            where T : class
        {
            foreach (var source in AllSources)
            {
                if (lookupFunction(source) is T skinSourced)
                    return skinSourced;
            }

            return null;
        }

        #region IResourceStorageProvider

        AudioManager IStorageResourceProvider.AudioManager => audio;
        IResourceStore<byte[]> IStorageResourceProvider.Resources => resources;
        IResourceStore<byte[]> IStorageResourceProvider.Files => userFiles;
        IResourceStore<TextureUpload> IStorageResourceProvider.CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => host.CreateTextureLoaderStore(underlyingStore);

        #endregion

        #region Implementation of IModelImporter<SkinInfo>

        public Action<Notification> PostNotification
        {
            set => skinModelManager.PostNotification = value;
        }

        public Action<IEnumerable<ILive<SkinInfo>>> PostImport
        {
            set => skinModelManager.PostImport = value;
        }

        public Task Import(params string[] paths)
        {
            return skinModelManager.Import(paths);
        }

        public Task Import(params ImportTask[] tasks)
        {
            return skinModelManager.Import(tasks);
        }

        public IEnumerable<string> HandledExtensions => skinModelManager.HandledExtensions;

        public Task<IEnumerable<ILive<SkinInfo>>> Import(ProgressNotification notification, params ImportTask[] tasks)
        {
            return skinModelManager.Import(notification, tasks);
        }

        public Task<ILive<SkinInfo>> Import(ImportTask task, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return skinModelManager.Import(task, lowPriority, cancellationToken);
        }

        public Task<ILive<SkinInfo>> Import(ArchiveReader archive, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return skinModelManager.Import(archive, lowPriority, cancellationToken);
        }

        public Task<ILive<SkinInfo>> Import(SkinInfo item, ArchiveReader archive = null, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return skinModelManager.Import(item, archive, lowPriority, cancellationToken);
        }

        #endregion

        #region Implementation of IModelManager<SkinInfo>

        public event Action<SkinInfo> ItemUpdated
        {
            add => skinModelManager.ItemUpdated += value;
            remove => skinModelManager.ItemUpdated -= value;
        }

        public event Action<SkinInfo> ItemRemoved
        {
            add => skinModelManager.ItemRemoved += value;
            remove => skinModelManager.ItemRemoved -= value;
        }

        public void Update(SkinInfo item)
        {
            skinModelManager.Update(item);
        }

        public bool Delete(SkinInfo item)
        {
            return skinModelManager.Delete(item);
        }

        public void Delete(List<SkinInfo> items, bool silent = false)
        {
            skinModelManager.Delete(items, silent);
        }

        public void Undelete(List<SkinInfo> items, bool silent = false)
        {
            skinModelManager.Undelete(items, silent);
        }

        public void Undelete(SkinInfo item)
        {
            skinModelManager.Undelete(item);
        }

        public bool IsAvailableLocally(SkinInfo model)
        {
            return skinModelManager.IsAvailableLocally(model);
        }

        #endregion
    }
}
