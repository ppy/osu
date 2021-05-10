// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        private readonly IStorageResourceProvider resources;

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

            // may be null for default skin.
            this.resources = resources;
        }

        public void UpdateDrawableTarget(SkinnableElementTargetContainer targetContainer)
        {
            DrawableComponentInfo[targetContainer.Target] = targetContainer.CreateSerialisedChildren().ToArray();
        }

        public virtual Drawable GetDrawableComponent(ISkinComponent component)
        {
            switch (component)
            {
                case SkinnableTargetComponent target:

                    var skinnableTarget = target.Target;

                    if (!DrawableComponentInfo.TryGetValue(skinnableTarget, out var skinnableInfo))
                    {
                        switch (skinnableTarget)
                        {
                            case SkinnableTarget.MainHUDComponents:

                                // skininfo files may be null for default skin.
                                var fileInfo = SkinInfo.Files?.FirstOrDefault(f => f.Filename == $"{skinnableTarget}.json");

                                if (fileInfo == null)
                                    return null;

                                var bytes = resources?.Files.Get(fileInfo.FileInfo.StoragePath);

                                if (bytes == null)
                                    return null;

                                string jsonContent = Encoding.UTF8.GetString(bytes);

                                DrawableComponentInfo[skinnableTarget] = skinnableInfo =
                                    JsonConvert.DeserializeObject<IEnumerable<SkinnableInfo>>(jsonContent).Where(i => i.Target == SkinnableTarget.MainHUDComponents).ToArray();
                                break;
                        }
                    }

                    if (skinnableInfo == null)
                        return null;

                    return new SkinnableTargetWrapper(skinnableTarget)
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

    public class SkinnableTargetWrapper : Container, IDefaultSkinnableTarget
    {
        // this is just here to implement the interface and allow a silly parent lookup to work (where the target is not the direct parent for reasons).
        // TODO: need to fix this.
        public SkinnableTarget Target { get; }

        public SkinnableTargetWrapper(SkinnableTarget target)
        {
            Target = target;
            RelativeSizeAxes = Axes.Both;
        }
    }
}
