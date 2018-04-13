// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.IO.Archives;

namespace osu.Game.Skinning
{
    public class SkinManager : ArchiveModelManager<SkinInfo, SkinFileInfo>, ISkinSource
    {
        private readonly AudioManager audio;

        public readonly Bindable<Skin> CurrentSkin = new Bindable<Skin>(new DefaultSkin());
        public readonly Bindable<SkinInfo> CurrentSkinInfo = new Bindable<SkinInfo>(SkinInfo.Default) { Default = SkinInfo.Default };

        public override string[] HandledExtensions => new[] { ".osk" };

        /// <summary>
        /// Returns a list of all usable <see cref="SkinInfo"/>s.
        /// </summary>
        /// <returns>A list of available <see cref="SkinInfo"/>.</returns>
        public List<SkinInfo> GetAllUsableSkins()
        {
            var userSkins = ModelStore.ConsumableItems.Where(s => !s.DeletePending).ToList();
            userSkins.Insert(0, SkinInfo.Default);
            return userSkins;
        }

        protected override SkinInfo CreateModel(ArchiveReader archive) => new SkinInfo
        {
            Name = archive.Name
        };

        protected override void Populate(SkinInfo model, ArchiveReader archive)
        {
            base.Populate(model, archive);
            populate(model);
        }

        /// <summary>
        /// Populate a <see cref="SkinInfo"/> from its <see cref="SkinConfiguration"/> (if possible).
        /// </summary>
        /// <param name="model"></param>
        private void populate(SkinInfo model)
        {
            Skin reference = GetSkin(model);
            if (!string.IsNullOrEmpty(reference.Configuration.SkinInfo.Name))
            {
                model.Name = reference.Configuration.SkinInfo.Name;
                model.Creator = reference.Configuration.SkinInfo.Creator;
            }
            else
            {
                model.Name = model.Name.Replace(".osk", "");
                model.Creator = "Unknown";
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

            return new LegacySkin(skinInfo, Files.Store, audio);
        }

        public SkinManager(Storage storage, DatabaseContextFactory contextFactory, IIpcHost importHost, AudioManager audio)
            : base(storage, contextFactory, new SkinStore(contextFactory, storage), importHost)
        {
            this.audio = audio;

            CurrentSkinInfo.ValueChanged += info => CurrentSkin.Value = GetSkin(info);
            CurrentSkin.ValueChanged += skin =>
            {
                if (skin.SkinInfo != CurrentSkinInfo.Value)
                    throw new InvalidOperationException($"Setting {nameof(CurrentSkin)}'s value directly is not supported. Use {nameof(CurrentSkinInfo)} instead.");

                SourceChanged?.Invoke();
            };

            // migrate older imports which didn't have access to skin.ini
            using (ContextFactory.GetForWrite())
            {
                foreach (var skinInfo in ModelStore.ConsumableItems.Where(s => s.Name.EndsWith(".osk")))
                {
                    populate(skinInfo);
                    Update(skinInfo);
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

        public Drawable GetDrawableComponent(string componentName) => CurrentSkin.Value.GetDrawableComponent(componentName);

        public Texture GetTexture(string componentName) => CurrentSkin.Value.GetTexture(componentName);

        public SampleChannel GetSample(string sampleName) => CurrentSkin.Value.GetSample(sampleName);

        public TValue GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue> query) where TConfiguration : SkinConfiguration where TValue : class => CurrentSkin.Value.GetValue(query);

        public TValue? GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue?> query) where TConfiguration : SkinConfiguration where TValue : struct => CurrentSkin.Value.GetValue(query);
    }
}
