// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.IO.Archives;

namespace osu.Game.Skinning
{
    public class SkinManager : ArchiveModelManager<SkinInfo, SkinFileInfo>
    {
        private readonly AudioManager audio;

        public Bindable<Skin> CurrentSkin = new Bindable<Skin>(new DefaultSkin());

        public override string[] HandledExtensions => new[] { ".osk" };

        /// <summary>
        /// Returns a list of all usable <see cref="SkinInfo"/>s.
        /// </summary>
        /// <returns>A list of available <see cref="SkinInfo"/>.</returns>
        public List<SkinInfo> GetAllUsableSkins() => ModelStore.ConsumableItems.Where(s => !s.DeletePending).ToList();

        protected override SkinInfo CreateModel(ArchiveReader archive) => new SkinInfo
        {
            Name = archive.Name
        };

        /// <summary>
        /// Retrieve a <see cref="Skin"/> instance for the provided <see cref="SkinInfo"/>
        /// </summary>
        /// <param name="skinInfo">The skin to lookup.</param>
        /// <returns>A <see cref="Skin"/> instance correlating to the provided <see cref="SkinInfo"/>.</returns>
        public Skin GetSkin(SkinInfo skinInfo)
        {
            return new LegacySkin(skinInfo, Files.Store, audio);
        }

        private SkinStore store;

        public SkinManager(Storage storage, DatabaseContextFactory contextFactory, IIpcHost importHost, AudioManager audio)
            : base(storage, contextFactory, new SkinStore(contextFactory, storage), importHost)
        {
            this.audio = audio;
        }
    }
}
