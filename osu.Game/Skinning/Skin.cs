// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Database;
using osu.Game.IO;

namespace osu.Game.Skinning
{
    public abstract class Skin : IDisposable, ISkin
    {
        /// <summary>
        /// A texture store which can be used to perform user file lookups for this skin.
        /// </summary>
        protected TextureStore? Textures { get; }

        /// <summary>
        /// A sample store which can be used to perform user file lookups for this skin.
        /// </summary>
        protected ISampleStore? Samples { get; }

        public readonly Live<SkinInfo> SkinInfo;

        public SkinConfiguration Configuration { get; set; }

        public IDictionary<SkinComponentsContainerLookup.TargetArea, SkinLayoutInfo> LayoutInfos => layoutInfos;

        private readonly Dictionary<SkinComponentsContainerLookup.TargetArea, SkinLayoutInfo> layoutInfos =
            new Dictionary<SkinComponentsContainerLookup.TargetArea, SkinLayoutInfo>();

        public abstract ISample? GetSample(ISampleInfo sampleInfo);

        public Texture? GetTexture(string componentName) => GetTexture(componentName, default, default);

        public abstract Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT);

        public abstract IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
            where TLookup : notnull
            where TValue : notnull;

        private readonly ResourceStore<byte[]> store = new ResourceStore<byte[]>();

        public string Name { get; }

        /// <summary>
        /// Construct a new skin.
        /// </summary>
        /// <param name="skin">The skin's metadata. Usually a live realm object.</param>
        /// <param name="resources">Access to game-wide resources.</param>
        /// <param name="fallbackStore">An optional fallback store which will be used for file lookups that are not serviced by realm user storage.</param>
        /// <param name="configurationFilename">An optional filename to read the skin configuration from. If not provided, the configuration will be retrieved from the storage using "skin.ini".</param>
        protected Skin(SkinInfo skin, IStorageResourceProvider? resources, IResourceStore<byte[]>? fallbackStore = null, string configurationFilename = @"skin.ini")
        {
            Name = skin.Name;

            if (resources != null)
            {
                SkinInfo = skin.ToLive(resources.RealmAccess);

                store.AddStore(new RealmBackedResourceStore<SkinInfo>(SkinInfo, resources.Files, resources.RealmAccess));

                var samples = resources.AudioManager?.GetSampleStore(store);

                if (samples != null)
                {
                    samples.PlaybackConcurrency = OsuGameBase.SAMPLE_CONCURRENCY;

                    // osu-stable performs audio lookups in order of wav -> mp3 -> ogg.
                    // The GetSampleStore() call above internally adds wav and mp3, so ogg is added at the end to ensure expected ordering.
                    samples.AddExtension(@"ogg");
                }

                Samples = samples;
                Textures = new TextureStore(resources.Renderer, CreateTextureLoaderStore(resources, store));
            }
            else
            {
                // Generally only used for tests.
                SkinInfo = skin.ToLiveUnmanaged();
            }

            if (fallbackStore != null)
                store.AddStore(fallbackStore);

            var configurationStream = store.GetStream(configurationFilename);

            if (configurationStream != null)
            {
                // stream will be closed after use by LineBufferedReader.
                ParseConfigurationStream(configurationStream);
                Debug.Assert(Configuration != null);
            }
            else
            {
                Configuration = new SkinConfiguration
                {
                    // generally won't be hit as we always write a `skin.ini` on import, but best be safe than sorry.
                    // see https://github.com/peppy/osu-stable-reference/blob/1531237b63392e82c003c712faa028406073aa8f/osu!/Graphics/Skinning/SkinManager.cs#L297-L298
                    LegacyVersion = SkinConfiguration.LATEST_VERSION,
                };
            }

            // skininfo files may be null for default skin.
            foreach (SkinComponentsContainerLookup.TargetArea skinnableTarget in Enum.GetValues<SkinComponentsContainerLookup.TargetArea>())
            {
                string filename = $"{skinnableTarget}.json";

                byte[]? bytes = store?.Get(filename);

                if (bytes == null)
                    continue;

                try
                {
                    string jsonContent = Encoding.UTF8.GetString(bytes);

                    SkinLayoutInfo? layoutInfo = null;

                    // handle namespace changes...
                    jsonContent = jsonContent.Replace(@"osu.Game.Screens.Play.SongProgress", @"osu.Game.Screens.Play.HUD.DefaultSongProgress");
                    jsonContent = jsonContent.Replace(@"osu.Game.Screens.Play.HUD.LegacyComboCounter", @"osu.Game.Skinning.LegacyComboCounter");
                    jsonContent = jsonContent.Replace(@"osu.Game.Screens.Play.HUD.PerformancePointsCounter", @"osu.Game.Skinning.Triangles.TrianglesPerformancePointsCounter");

                    try
                    {
                        // First attempt to deserialise using the new SkinLayoutInfo format
                        layoutInfo = JsonConvert.DeserializeObject<SkinLayoutInfo>(jsonContent);
                    }
                    catch
                    {
                    }

                    // Of note, the migration code below runs on read of skins, but there's nothing to
                    // force a rewrite after migration. Let's not remove these migration rules until we
                    // have something in place to ensure we don't end up breaking skins of users that haven't
                    // manually saved their skin since a change was implemented.

                    // If deserialisation using SkinLayoutInfo fails, attempt to deserialise using the old naked list.
                    if (layoutInfo == null)
                    {
                        var deserializedContent = JsonConvert.DeserializeObject<IEnumerable<SerialisedDrawableInfo>>(jsonContent);

                        if (deserializedContent == null)
                            continue;

                        layoutInfo = new SkinLayoutInfo();
                        layoutInfo.Update(null, deserializedContent.ToArray());

                        Logger.Log($"Ferrying {deserializedContent.Count()} components in {skinnableTarget} to global section of new {nameof(SkinLayoutInfo)} format");
                    }

                    LayoutInfos[skinnableTarget] = layoutInfo;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to load skin configuration.");
                }
            }
        }

        protected virtual IResourceStore<TextureUpload> CreateTextureLoaderStore(IStorageResourceProvider resources, IResourceStore<byte[]> storage)
            => new MaxDimensionLimitedTextureLoaderStore(resources.CreateTextureLoaderStore(storage));

        protected virtual void ParseConfigurationStream(Stream stream)
        {
            using (LineBufferedReader reader = new LineBufferedReader(stream, true))
                Configuration = new LegacySkinDecoder().Decode(reader);
        }

        /// <summary>
        /// Remove all stored customisations for the provided target.
        /// </summary>
        /// <param name="targetContainer">The target container to reset.</param>
        public void ResetDrawableTarget(SkinComponentsContainer targetContainer)
        {
            LayoutInfos.Remove(targetContainer.Lookup.Target);
        }

        /// <summary>
        /// Update serialised information for the provided target.
        /// </summary>
        /// <param name="targetContainer">The target container to serialise to this skin.</param>
        public void UpdateDrawableTarget(SkinComponentsContainer targetContainer)
        {
            if (!LayoutInfos.TryGetValue(targetContainer.Lookup.Target, out var layoutInfo))
                layoutInfos[targetContainer.Lookup.Target] = layoutInfo = new SkinLayoutInfo();

            layoutInfo.Update(targetContainer.Lookup.Ruleset, ((ISerialisableDrawableContainer)targetContainer).CreateSerialisedInfo().ToArray());
        }

        public virtual Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                // This fallback is important for user skins which use SkinnableSprites.
                case SkinnableSprite.SpriteComponentLookup sprite:
                    return this.GetAnimation(sprite.LookupName, false, false, maxSize: sprite.MaxSize);

                case SkinComponentsContainerLookup containerLookup:

                    // It is important to return null if the user has not configured this yet.
                    // This allows skin transformers the opportunity to provide default components.
                    if (!LayoutInfos.TryGetValue(containerLookup.Target, out var layoutInfo)) return null;
                    if (!layoutInfo.TryGetDrawableInfo(containerLookup.Ruleset, out var drawableInfos)) return null;

                    return new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        ChildrenEnumerable = drawableInfos.Select(i => i.CreateInstance())
                    };
            }

            return null;
        }

        #region Disposal

        ~Skin()
        {
            // required to potentially clean up sample store from audio hierarchy.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed)
                return;

            isDisposed = true;

            Textures?.Dispose();
            Samples?.Dispose();

            store.Dispose();
        }

        #endregion

        public override string ToString() => $"{GetType().ReadableName()} {{ Name: {Name} }}";

        private static readonly ThreadLocal<int> nested_level = new ThreadLocal<int>(() => 0);

        [Conditional("SKIN_LOOKUP_DEBUG")]
        internal static void LogLookupDebug(object callingClass, object lookup, LookupDebugType type, [CallerMemberName] string callerMethod = "")
        {
            string icon = string.Empty;
            int level = nested_level.Value;

            switch (type)
            {
                case LookupDebugType.Hit:
                    icon = "🟢 hit";
                    break;

                case LookupDebugType.Miss:
                    icon = "🔴 miss";
                    break;

                case LookupDebugType.Enter:
                    nested_level.Value++;
                    break;

                case LookupDebugType.Exit:
                    nested_level.Value--;
                    if (nested_level.Value == 0)
                        Logger.Log(string.Empty);
                    return;
            }

            string lookupString = lookup.ToString() ?? string.Empty;
            string callingClassString = callingClass.ToString() ?? string.Empty;

            Logger.Log($"{string.Join(null, Enumerable.Repeat("|-", level))}{callingClassString}.{callerMethod}(lookup: {lookupString}) {icon}");
        }

        internal enum LookupDebugType
        {
            Hit,
            Miss,
            Enter,
            Exit
        }
    }
}
