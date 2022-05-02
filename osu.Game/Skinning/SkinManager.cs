﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Models;
using osu.Game.Overlays.Notifications;
using osu.Game.Utils;

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
    public class SkinManager : ISkinSource, IStorageResourceProvider, IModelImporter<SkinInfo>, IModelManager<SkinInfo>, IModelFileManager<SkinInfo, RealmNamedFileUsage>
    {
        private readonly AudioManager audio;

        private readonly Scheduler scheduler;

        private readonly GameHost host;

        private readonly IResourceStore<byte[]> resources;

        public readonly Bindable<Skin> CurrentSkin = new Bindable<Skin>();

        public readonly Bindable<Live<SkinInfo>> CurrentSkinInfo = new Bindable<Live<SkinInfo>>(Skinning.DefaultSkin.CreateInfo().ToLiveUnmanaged())
        {
            Default = Skinning.DefaultSkin.CreateInfo().ToLiveUnmanaged()
        };

        private readonly SkinModelManager skinModelManager;
        private readonly RealmAccess realm;

        private readonly IResourceStore<byte[]> userFiles;

        /// <summary>
        /// The default skin.
        /// </summary>
        public Skin DefaultSkin { get; }

        /// <summary>
        /// The default legacy skin.
        /// </summary>
        public Skin DefaultLegacySkin { get; }

        public SkinManager(Storage storage, RealmAccess realm, GameHost host, IResourceStore<byte[]> resources, AudioManager audio, Scheduler scheduler)
        {
            this.realm = realm;
            this.audio = audio;
            this.scheduler = scheduler;
            this.host = host;
            this.resources = resources;

            userFiles = new StorageBackedResourceStore(storage.GetStorageForDirectory("files"));

            skinModelManager = new SkinModelManager(storage, realm, this);

            var defaultSkins = new[]
            {
                DefaultLegacySkin = new DefaultLegacySkin(this),
                DefaultSkin = new DefaultSkin(this),
            };

            // Ensure the default entries are present.
            realm.Write(r =>
            {
                foreach (var skin in defaultSkins)
                {
                    if (r.Find<SkinInfo>(skin.SkinInfo.ID) == null)
                        r.Add(skin.SkinInfo.Value);
                }
            });

            CurrentSkinInfo.ValueChanged += skin =>
            {
                CurrentSkin.Value = skin.NewValue.PerformRead(GetSkin);
            };

            CurrentSkin.Value = DefaultSkin;
            CurrentSkin.ValueChanged += skin =>
            {
                if (!skin.NewValue.SkinInfo.Equals(CurrentSkinInfo.Value))
                    throw new InvalidOperationException($"Setting {nameof(CurrentSkin)}'s value directly is not supported. Use {nameof(CurrentSkinInfo)} instead.");

                SourceChanged?.Invoke();
            };
        }

        public void SelectRandomSkin()
        {
            realm.Run(r =>
            {
                // choose from only user skins, removing the current selection to ensure a new one is chosen.
                var randomChoices = r.All<SkinInfo>().Where(s => !s.DeletePending && s.ID != CurrentSkinInfo.Value.ID).ToArray();

                if (randomChoices.Length == 0)
                {
                    CurrentSkinInfo.Value = Skinning.DefaultSkin.CreateInfo().ToLiveUnmanaged();
                    return;
                }

                var chosen = randomChoices.ElementAt(RNG.Next(0, randomChoices.Length));

                CurrentSkinInfo.Value = chosen.ToLive(realm);
            });
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
            CurrentSkinInfo.Value.PerformRead(s =>
            {
                if (!s.Protected)
                    return;

                string[] existingSkinNames = realm.Run(r => r.All<SkinInfo>()
                                                             .Where(skin => !skin.DeletePending)
                                                             .AsEnumerable()
                                                             .Select(skin => skin.Name).ToArray());

                // if the user is attempting to save one of the default skin implementations, create a copy first.
                var skinInfo = new SkinInfo
                {
                    Creator = s.Creator,
                    InstantiationInfo = s.InstantiationInfo,
                    Name = NamingUtils.GetNextBestName(existingSkinNames, $"{s.Name} (modified)")
                };

                var result = skinModelManager.Import(skinInfo);

                if (result != null)
                {
                    // save once to ensure the required json content is populated.
                    // currently this only happens on save.
                    result.PerformRead(skin => Save(skin.CreateInstance(this)));
                    CurrentSkinInfo.Value = result;
                }
            });
        }

        public void Save(Skin skin)
        {
            if (!skin.SkinInfo.IsManaged)
                throw new InvalidOperationException($"Attempting to save a skin which is not yet tracked. Call {nameof(EnsureMutableSkin)} first.");

            skinModelManager.Save(skin);
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="SkinInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public Live<SkinInfo> Query(Expression<Func<SkinInfo, bool>> query)
        {
            return realm.Run(r => r.All<SkinInfo>().FirstOrDefault(query)?.ToLive(realm));
        }

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
        RealmAccess IStorageResourceProvider.RealmAccess => realm;
        IResourceStore<TextureUpload> IStorageResourceProvider.CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => host.CreateTextureLoaderStore(underlyingStore);

        #endregion

        #region Implementation of IModelImporter<SkinInfo>

        public Action<Notification> PostNotification
        {
            set => skinModelManager.PostNotification = value;
        }

        public Action<IEnumerable<Live<SkinInfo>>> PostImport
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

        public Task<IEnumerable<Live<SkinInfo>>> Import(ProgressNotification notification, params ImportTask[] tasks)
        {
            return skinModelManager.Import(notification, tasks);
        }

        public Task<Live<SkinInfo>> Import(ImportTask task, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return skinModelManager.Import(task, lowPriority, cancellationToken);
        }

        public Task<Live<SkinInfo>> Import(ArchiveReader archive, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return skinModelManager.Import(archive, lowPriority, cancellationToken);
        }

        public Live<SkinInfo> Import(SkinInfo item, ArchiveReader archive = null, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return skinModelManager.Import(item, archive, lowPriority, cancellationToken);
        }

        #endregion

        #region Implementation of IModelManager<SkinInfo>

        public void Delete([CanBeNull] Expression<Func<SkinInfo, bool>> filter = null, bool silent = false)
        {
            realm.Run(r =>
            {
                var items = r.All<SkinInfo>()
                             .Where(s => !s.Protected && !s.DeletePending);
                if (filter != null)
                    items = items.Where(filter);

                // check the removed skin is not the current user choice. if it is, switch back to default.
                Guid currentUserSkin = CurrentSkinInfo.Value.ID;

                if (items.Any(s => s.ID == currentUserSkin))
                    scheduler.Add(() => CurrentSkinInfo.Value = Skinning.DefaultSkin.CreateInfo().ToLiveUnmanaged());

                skinModelManager.Delete(items.ToList(), silent);
            });
        }

        #endregion

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

        public void ReplaceFile(SkinInfo model, RealmNamedFileUsage file, Stream contents)
        {
            skinModelManager.ReplaceFile(model, file, contents);
        }

        public void DeleteFile(SkinInfo model, RealmNamedFileUsage file)
        {
            skinModelManager.DeleteFile(model, file);
        }

        public void AddFile(SkinInfo model, Stream contents, string filename)
        {
            skinModelManager.AddFile(model, contents, filename);
        }
    }
}
