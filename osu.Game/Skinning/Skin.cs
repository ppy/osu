﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.IO;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Skinning
{
    public abstract class Skin : IDisposable, ISkin
    {
        public readonly SkinInfo SkinInfo;

        public SkinConfiguration Configuration { get; protected set; }

        public IDictionary<SkinnableTarget, SkinnableInfo[]> DrawableComponentInfo => drawableComponentInfo;

        private readonly Dictionary<SkinnableTarget, SkinnableInfo[]> drawableComponentInfo = new Dictionary<SkinnableTarget, SkinnableInfo[]>();

        public abstract ISample GetSample(ISampleInfo sampleInfo);

        public Texture GetTexture(string componentName) => GetTexture(componentName, default, default);

        public abstract Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT);

        public abstract IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup);

        protected Skin(SkinInfo skin, IStorageResourceProvider resources)
        {
            SkinInfo = skin;

            // we may want to move this to some kind of async operation in the future.
            foreach (SkinnableTarget skinnableTarget in Enum.GetValues(typeof(SkinnableTarget)))
            {
                string filename = $"{skinnableTarget}.json";

                // skininfo files may be null for default skin.
                var fileInfo = SkinInfo.Files?.FirstOrDefault(f => f.Filename == filename);

                if (fileInfo == null)
                    continue;

                var bytes = resources?.Files.Get(fileInfo.FileInfo.StoragePath);

                if (bytes == null)
                    continue;

                string jsonContent = Encoding.UTF8.GetString(bytes);
                var deserializedContent = JsonConvert.DeserializeObject<IEnumerable<SkinnableInfo>>(jsonContent);

                if (deserializedContent == null)
                    continue;

                DrawableComponentInfo[skinnableTarget] = deserializedContent.ToArray();
            }
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

        public virtual Drawable GetDrawableComponent(ISkinComponent component)
        {
            switch (component)
            {
                case SkinnableTargetComponent target:
                    if (!DrawableComponentInfo.TryGetValue(target.Target, out var skinnableInfo))
                        return null;

                    return new SkinnableTargetComponentsContainer
                    {
                        ChildrenEnumerable = skinnableInfo.Select(i => i.CreateInstance())
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
        }

        #endregion
    }
}
