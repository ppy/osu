// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Database;
using osu.Game.IO;
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
    public class SkinManager : ModelManager<SkinInfo>, ISkinSource, IStorageResourceProvider, IModelImporter<SkinInfo>
    {
        /// <summary>
        /// The default "classic" skin.
        /// </summary>
        public Skin DefaultClassicSkin { get; }

        private readonly AudioManager audio;

        private readonly Scheduler scheduler;

        private readonly GameHost host;

        private readonly IResourceStore<byte[]> resources;

        public readonly Bindable<Skin> CurrentSkin = new Bindable<Skin>();

        public readonly Bindable<Live<SkinInfo>> CurrentSkinInfo = new Bindable<Live<SkinInfo>>(ArgonSkin.CreateInfo().ToLiveUnmanaged());

        private readonly SkinImporter skinImporter;

        private readonly LegacySkinExporter skinExporter;

        private readonly IResourceStore<byte[]> userFiles;

        private Skin argonSkin { get; }

        private Skin trianglesSkin { get; }

        public override bool PauseImports
        {
            get => base.PauseImports;
            set
            {
                base.PauseImports = value;
                skinImporter.PauseImports = value;
            }
        }

        public SkinManager(Storage storage, RealmAccess realm, GameHost host, IResourceStore<byte[]> resources, AudioManager audio, Scheduler scheduler)
            : base(storage, realm)
        {
            this.audio = audio;
            this.scheduler = scheduler;
            this.host = host;
            this.resources = resources;

            userFiles = new StorageBackedResourceStore(storage.GetStorageForDirectory("files"));

            skinImporter = new SkinImporter(storage, realm, this)
            {
                PostNotification = obj => PostNotification?.Invoke(obj),
            };

            var defaultSkins = new[]
            {
                DefaultClassicSkin = new DefaultLegacySkin(this),
                trianglesSkin = new TrianglesSkin(this),
                argonSkin = new ArgonSkin(this),
                new ArgonProSkin(this),
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

            CurrentSkin.Value = argonSkin;
            CurrentSkin.ValueChanged += skin =>
            {
                if (!skin.NewValue.SkinInfo.Equals(CurrentSkinInfo.Value))
                    throw new InvalidOperationException($"Setting {nameof(CurrentSkin)}'s value directly is not supported. Use {nameof(CurrentSkinInfo)} instead.");

                SourceChanged?.Invoke();
            };

            skinExporter = new LegacySkinExporter(storage)
            {
                PostNotification = obj => PostNotification?.Invoke(obj)
            };
        }

        public void SelectRandomSkin()
        {
            Realm.Run(r =>
            {
                // choose from only user skins, removing the current selection to ensure a new one is chosen.
                var randomChoices = r.All<SkinInfo>()
                                     .Where(s => !s.DeletePending && s.ID != CurrentSkinInfo.Value.ID)
                                     .ToArray();

                if (randomChoices.Length == 0)
                {
                    CurrentSkinInfo.Value = ArgonSkin.CreateInfo().ToLiveUnmanaged();
                    return;
                }

                var chosen = randomChoices.ElementAt(RNG.Next(0, randomChoices.Length));

                CurrentSkinInfo.Value = chosen.ToLive(Realm);
            });
        }

        /// <summary>
        /// Retrieve a <see cref="Skin"/> instance for the provided <see cref="SkinInfo"/>
        /// </summary>
        /// <param name="skinInfo">The skin to lookup.</param>
        /// <returns>A <see cref="Skin"/> instance correlating to the provided <see cref="SkinInfo"/>.</returns>
        public Skin GetSkin(SkinInfo skinInfo) => skinInfo.CreateInstance(this);

        /// <summary>
        /// Returns a skin instance representing the current skin but in a state it can accept user modifications.
        /// This will create a copy of any internal skin and being tracking in the database if not already.
        /// </summary>
        /// <returns>A mutable version of the current skin, represented by a live instance of its <see cref="SkinInfo"/>.</returns>
        public Live<SkinInfo> GetMutableSkin()
        {
            return CurrentSkinInfo.Value.PerformRead(s =>
            {
                if (!s.Protected)
                    return CurrentSkinInfo.Value;

                string[] existingSkinNames = Realm.Run(r => r.All<SkinInfo>()
                                                             .Where(skin => !skin.DeletePending)
                                                             .AsEnumerable()
                                                             .Select(skin => skin.Name).ToArray());

                // if the user is attempting to save one of the default skin implementations, create a copy first.
                var skinInfo = new SkinInfo
                {
                    Creator = s.Creator,
                    InstantiationInfo = s.InstantiationInfo,
                    Name = NamingUtils.GetNextBestName(existingSkinNames, $@"{s.Name} (modified)")
                };

                var result = skinImporter.ImportModel(skinInfo, parameters: new ImportParameters
                {
                    ImportImmediately = true // to avoid possible deadlocks when editing skin during gameplay.
                });

                if (result != null)
                {
                    // save once to ensure the required json content is populated.
                    // currently this only happens on save.
                    result.PerformRead(skin => Save(skin.CreateInstance(this)));
                    return result;
                }

                return CurrentSkinInfo.Value;
            });
        }

        /// <summary>
        /// Save a skin, serialising any changes to skin layouts to relevant JSON structures.
        /// </summary>
        /// <returns>Whether any change actually occurred.</returns>
        public bool Save(Skin skin)
        {
            if (!skin.SkinInfo.IsManaged)
                throw new InvalidOperationException($"Attempting to save a skin which is not yet tracked. Use {nameof(GetMutableSkin)} first.");

            return skinImporter.Save(skin);
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="SkinInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public Live<SkinInfo> Query(Expression<Func<SkinInfo, bool>> query)
        {
            return Realm.Run(r => r.All<SkinInfo>().FirstOrDefault(query)?.ToLive(Realm));
        }

        public event Action SourceChanged;

        public Drawable GetDrawableComponent(ISkinComponentLookup lookup) => lookupWithFallback(s => s.GetDrawableComponent(lookup));

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

                // Skin manager provides default fallbacks.
                // This handles cases where a user skin doesn't have the required resources for complete display of
                // certain elements.

                if (CurrentSkin.Value is LegacySkin && CurrentSkin.Value != DefaultClassicSkin)
                    yield return DefaultClassicSkin;

                if (CurrentSkin.Value != trianglesSkin)
                    yield return trianglesSkin;
            }
        }

        private T lookupWithFallback<T>(Func<ISkin, T> lookupFunction)
            where T : class
        {
            try
            {
                Skin.LogLookupDebug(this, lookupFunction, Skin.LookupDebugType.Enter);

                foreach (var source in AllSources)
                {
                    if (lookupFunction(source) is T skinSourced)
                        return skinSourced;
                }

                return null;
            }
            finally
            {
                Skin.LogLookupDebug(this, lookupFunction, Skin.LookupDebugType.Exit);
            }
        }

        #region IResourceStorageProvider

        IRenderer IStorageResourceProvider.Renderer => host.Renderer;
        AudioManager IStorageResourceProvider.AudioManager => audio;
        IResourceStore<byte[]> IStorageResourceProvider.Resources => resources;
        IResourceStore<byte[]> IStorageResourceProvider.Files => userFiles;
        RealmAccess IStorageResourceProvider.RealmAccess => Realm;
        IResourceStore<TextureUpload> IStorageResourceProvider.CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => host.CreateTextureLoaderStore(underlyingStore);

        #endregion

        #region Implementation of IModelImporter<SkinInfo>

        public Action<IEnumerable<Live<SkinInfo>>> PresentImport
        {
            set => skinImporter.PresentImport = value;
        }

        public Task Import(params string[] paths) => skinImporter.Import(paths);

        public Task Import(ImportTask[] imports, ImportParameters parameters = default) => skinImporter.Import(imports, parameters);

        public IEnumerable<string> HandledExtensions => skinImporter.HandledExtensions;

        public Task<IEnumerable<Live<SkinInfo>>> Import(ProgressNotification notification, ImportTask[] tasks, ImportParameters parameters = default) =>
            skinImporter.Import(notification, tasks, parameters);

        public Task<Live<SkinInfo>> ImportAsUpdate(ProgressNotification notification, ImportTask task, SkinInfo original) =>
            skinImporter.ImportAsUpdate(notification, task, original);

        public Task<Live<SkinInfo>> Import(ImportTask task, ImportParameters parameters = default, CancellationToken cancellationToken = default) =>
            skinImporter.Import(task, parameters, cancellationToken);

        public Task ExportCurrentSkin() => ExportSkin(CurrentSkinInfo.Value);

        public Task ExportSkin(Live<SkinInfo> skin) => skinExporter.ExportAsync(skin);

        #endregion

        public void Delete([CanBeNull] Expression<Func<SkinInfo, bool>> filter = null, bool silent = false)
        {
            Realm.Run(r =>
            {
                var items = r.All<SkinInfo>()
                             .Where(s => !s.Protected && !s.DeletePending);
                if (filter != null)
                    items = items.Where(filter);

                // check the removed skin is not the current user choice. if it is, switch back to default.
                Guid currentUserSkin = CurrentSkinInfo.Value.ID;

                if (items.Any(s => s.ID == currentUserSkin))
                    scheduler.Add(() => CurrentSkinInfo.Value = ArgonSkin.CreateInfo().ToLiveUnmanaged());

                Delete(items.ToList(), silent);
            });
        }

        public void SetSkinFromConfiguration(string guidString)
        {
            Live<SkinInfo> skinInfo = null;

            if (Guid.TryParse(guidString, out var guid))
                skinInfo = Query(s => s.ID == guid);

            if (skinInfo == null)
            {
                if (guid == SkinInfo.CLASSIC_SKIN)
                    skinInfo = DefaultClassicSkin.SkinInfo;
            }

            CurrentSkinInfo.Value = skinInfo ?? trianglesSkin.SkinInfo;
        }
    }
}
