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
using osu.Framework.Logging;
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
    /// <summary>
    /// Handles the storage and retrieval of <see cref="Skin"/>s.
    /// </summary>
    /// <remarks>
    /// This is also exposed and cached as <see cref="ISkinSource"/> to allow for any component to potentially have skinning support.
    /// For gameplay components, see <see cref="RulesetSkinProvidingContainer"/> which adds extra legacy and toggle logic that may affect the lookup process.
    /// </remarks>
    [ExcludeFromDynamicCompile]
    public class SkinManager : ArchiveModelManager<SkinInfo, SkinFileInfo>, ISkinSource, IStorageResourceProvider
    {
        private readonly AudioManager audio;

        private readonly GameHost host;

        private readonly IResourceStore<byte[]> resources;

        public readonly Bindable<Skin> CurrentSkin = new Bindable<Skin>();
        public readonly Bindable<SkinInfo> CurrentSkinInfo = new Bindable<SkinInfo>(SkinInfo.Default) { Default = SkinInfo.Default };

        public override IEnumerable<string> HandledExtensions => new[] { ".osk" };

        protected override string[] HashableFileTypes => new[] { ".ini", ".json" };

        protected override string ImportFromStablePath => "Skins";

        /// <summary>
        /// The default skin.
        /// </summary>
        public Skin DefaultSkin { get; }

        /// <summary>
        /// The default legacy skin.
        /// </summary>
        public Skin DefaultLegacySkin { get; }

        public SkinManager(Storage storage, DatabaseContextFactory contextFactory, GameHost host, IResourceStore<byte[]> resources, AudioManager audio)
            : base(storage, contextFactory, new SkinStore(contextFactory, storage), host)
        {
            this.audio = audio;
            this.host = host;
            this.resources = resources;

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

            // can be removed 20220420.
            populateMissingHashes();
        }

        private void populateMissingHashes()
        {
            var skinsWithoutHashes = ModelStore.ConsumableItems.Where(i => i.Hash == null).ToArray();

            foreach (SkinInfo skin in skinsWithoutHashes)
            {
                try
                {
                    Update(skin);
                }
                catch (Exception e)
                {
                    Delete(skin);
                    Logger.Error(e, $"Existing skin {skin} has been deleted during hash recomputation due to being invalid");
                }
            }
        }

        protected override bool ShouldDeleteArchive(string path) => Path.GetExtension(path)?.ToLowerInvariant() == @".osk";

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
                return ModelStore.ConsumableItems.Where(s => !s.DeletePending).ToList();

            return ModelStore.Items.Where(s => !s.DeletePending).ToList();
        }

        public void SelectRandomSkin()
        {
            // choose from only user skins, removing the current selection to ensure a new one is chosen.
            var randomChoices = ModelStore.Items.Where(s => !s.DeletePending && s.ID != CurrentSkinInfo.Value.ID).ToArray();

            if (randomChoices.Length == 0)
            {
                CurrentSkinInfo.Value = SkinInfo.Default;
                return;
            }

            var chosen = randomChoices.ElementAt(RNG.Next(0, randomChoices.Length));
            CurrentSkinInfo.Value = ModelStore.ConsumableItems.Single(i => i.ID == chosen.ID);
        }

        protected override SkinInfo CreateModel(ArchiveReader archive) => new SkinInfo { Name = archive.Name ?? @"No name" };

        private const string unknown_creator_string = @"Unknown";

        protected override bool HasCustomHashFunction => true;

        protected override string ComputeHash(SkinInfo item)
        {
            var instance = GetSkin(item);

            // This function can be run on fresh import or save. The logic here ensures a skin.ini file is in a good state for both operations.

            // `Skin` will parse the skin.ini and populate `Skin.Configuration` during construction above.
            string skinIniSourcedName = instance.Configuration.SkinInfo.Name;
            string skinIniSourcedCreator = instance.Configuration.SkinInfo.Creator;
            string archiveName = item.Name.Replace(@".osk", string.Empty, StringComparison.OrdinalIgnoreCase);

            bool isImport = item.ID == 0;

            if (isImport)
            {
                item.Name = !string.IsNullOrEmpty(skinIniSourcedName) ? skinIniSourcedName : archiveName;
                item.Creator = !string.IsNullOrEmpty(skinIniSourcedCreator) ? skinIniSourcedCreator : unknown_creator_string;

                // For imports, we want to use the archive or folder name as part of the metadata, in addition to any existing skin.ini metadata.
                // In an ideal world, skin.ini would be the only source of metadata, but a lot of skin creators and users don't update it when making modifications.
                // In both of these cases, the expectation from the user is that the filename or folder name is displayed somewhere to identify the skin.
                if (archiveName != item.Name)
                    item.Name = @$"{item.Name} [{archiveName}]";
            }

            // By this point, the metadata in SkinInfo will be correct.
            // Regardless of whether this is an import or not, let's write the skin.ini if non-existing or non-matching.
            // This is (weirdly) done inside ComputeHash to avoid adding a new method to handle this case. After switching to realm it can be moved into another place.
            if (skinIniSourcedName != item.Name)
                updateSkinIniMetadata(item);

            return base.ComputeHash(item);
        }

        private void updateSkinIniMetadata(SkinInfo item)
        {
            string nameLine = @$"Name: {item.Name}";
            string authorLine = @$"Author: {item.Creator}";

            string[] newLines =
            {
                @"// The following content was automatically added by osu! during import, based on filename / folder metadata.",
                @"[General]",
                nameLine,
                authorLine,
            };

            var existingFile = item.Files.SingleOrDefault(f => f.Filename.Equals(@"skin.ini", StringComparison.OrdinalIgnoreCase));

            if (existingFile == null)
            {
                // In the case a skin doesn't have a skin.ini yet, let's create one.
                writeNewSkinIni();
                return;
            }

            using (Stream stream = new MemoryStream())
            {
                using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                {
                    using (var existingStream = Files.Storage.GetStream(existingFile.FileInfo.GetStoragePath()))
                    using (var sr = new StreamReader(existingStream))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                            sw.WriteLine(line);
                    }

                    sw.WriteLine();

                    foreach (string line in newLines)
                        sw.WriteLine(line);
                }

                ReplaceFile(item, existingFile, stream);

                // can be removed 20220502.
                if (!ensureIniWasUpdated(item))
                {
                    Logger.Log($"Skin {item}'s skin.ini had issues and has been removed. Please report this and provide the problematic skin.", LoggingTarget.Database, LogLevel.Important);

                    DeleteFile(item, item.Files.SingleOrDefault(f => f.Filename.Equals(@"skin.ini", StringComparison.OrdinalIgnoreCase)));
                    writeNewSkinIni();
                }
            }

            void writeNewSkinIni()
            {
                using (Stream stream = new MemoryStream())
                {
                    using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                    {
                        foreach (string line in newLines)
                            sw.WriteLine(line);
                    }

                    AddFile(item, stream, @"skin.ini");
                }
            }
        }

        private bool ensureIniWasUpdated(SkinInfo item)
        {
            // This is a final consistency check to ensure that hash computation doesn't enter an infinite loop.
            // With other changes to the surrounding code this should never be hit, but until we are 101% sure that there
            // are no other cases let's avoid a hard startup crash by bailing and alerting.

            var instance = GetSkin(item);

            return instance.Configuration.SkinInfo.Name == item.Name;
        }

        protected override Task Populate(SkinInfo model, ArchiveReader archive, CancellationToken cancellationToken = default)
        {
            var instance = GetSkin(model);

            model.InstantiationInfo ??= instance.GetType().GetInvariantInstantiationInfo();

            model.Name = instance.Configuration.SkinInfo.Name;
            model.Creator = instance.Configuration.SkinInfo.Creator;

            return Task.CompletedTask;
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
        IResourceStore<byte[]> IStorageResourceProvider.Files => Files.Store;
        IResourceStore<TextureUpload> IStorageResourceProvider.CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => host.CreateTextureLoaderStore(underlyingStore);

        #endregion
    }
}
