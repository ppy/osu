// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Screens.Play.HUD;

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

        public IDictionary<SkinnableTarget, SkinnableInfo[]> DrawableComponentInfo => drawableComponentInfo;

        private readonly Dictionary<SkinnableTarget, SkinnableInfo[]> drawableComponentInfo = new Dictionary<SkinnableTarget, SkinnableInfo[]>();

        public abstract ISample? GetSample(ISampleInfo sampleInfo);

        public Texture? GetTexture(string componentName) => GetTexture(componentName, default, default);

        public abstract Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT);

        public abstract IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
            where TLookup : notnull
            where TValue : notnull;

        private readonly RealmBackedResourceStore<SkinInfo>? realmBackedStorage;

        /// <summary>
        /// Construct a new skin.
        /// </summary>
        /// <param name="skin">The skin's metadata. Usually a live realm object.</param>
        /// <param name="resources">Access to game-wide resources.</param>
        /// <param name="storage">An optional store which will *replace* all file lookups that are usually sourced from <paramref name="skin"/>.</param>
        /// <param name="configurationFilename">An optional filename to read the skin configuration from. If not provided, the configuration will be retrieved from the storage using "skin.ini".</param>
        protected Skin(SkinInfo skin, IStorageResourceProvider? resources, IResourceStore<byte[]>? storage = null, string configurationFilename = @"skin.ini")
        {
            if (resources != null)
            {
                SkinInfo = skin.ToLive(resources.RealmAccess);

                storage ??= realmBackedStorage = new RealmBackedResourceStore<SkinInfo>(SkinInfo, resources.Files, resources.RealmAccess);

                (storage as ResourceStore<byte[]>)?.AddExtension("ogg");

                var samples = resources.AudioManager?.GetSampleStore(storage);
                if (samples != null)
                    samples.PlaybackConcurrency = OsuGameBase.SAMPLE_CONCURRENCY;

                Samples = samples;
                Textures = new TextureStore(resources.CreateTextureLoaderStore(storage));
            }
            else
            {
                // Generally only used for tests.
                SkinInfo = skin.ToLiveUnmanaged();
            }

            var configurationStream = storage?.GetStream(configurationFilename);

            if (configurationStream != null)
            {
                // stream will be closed after use by LineBufferedReader.
                ParseConfigurationStream(configurationStream);
                Debug.Assert(Configuration != null);
            }
            else
                Configuration = new SkinConfiguration();

            // skininfo files may be null for default skin.
            foreach (SkinnableTarget skinnableTarget in Enum.GetValues(typeof(SkinnableTarget)))
            {
                string filename = $"{skinnableTarget}.json";

                byte[]? bytes = storage?.Get(filename);

                if (bytes == null)
                    continue;

                try
                {
                    string jsonContent = Encoding.UTF8.GetString(bytes);
                    var deserializedContent = JsonConvert.DeserializeObject<IEnumerable<SkinnableInfo>>(jsonContent);

                    if (deserializedContent == null)
                        continue;

                    DrawableComponentInfo[skinnableTarget] = deserializedContent.ToArray();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to load skin configuration.");
                }
            }
        }

        protected virtual void ParseConfigurationStream(Stream stream)
        {
            using (LineBufferedReader reader = new LineBufferedReader(stream, true))
                Configuration = new LegacySkinDecoder().Decode(reader);
        }

        /// <summary>
        /// Remove all stored customisations for the provided target.
        /// </summary>
        /// <param name="targetContainer">The target container to reset.</param>
        public void ResetDrawableTarget(ISkinnableTarget targetContainer)
        {
            DrawableComponentInfo.Remove(targetContainer.Target);
        }

        /// <summary>
        /// Update serialised information for the provided target.
        /// </summary>
        /// <param name="targetContainer">The target container to serialise to this skin.</param>
        public void UpdateDrawableTarget(ISkinnableTarget targetContainer)
        {
            DrawableComponentInfo[targetContainer.Target] = targetContainer.CreateSkinnableInfo().ToArray();
        }

        public virtual Drawable? GetDrawableComponent(ISkinComponent component)
        {
            switch (component)
            {
                case SkinnableTargetComponent target:
                    if (!DrawableComponentInfo.TryGetValue(target.Target, out var skinnableInfo))
                        return null;

                    var components = new List<Drawable>();

                    foreach (var i in skinnableInfo)
                        components.Add(i.CreateInstance());

                    return new SkinnableTargetComponentsContainer
                    {
                        Children = components,
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

            realmBackedStorage?.Dispose();
        }

        #endregion
    }
}
