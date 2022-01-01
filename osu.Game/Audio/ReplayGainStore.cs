using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Beatmaps;
using osu.Framework.Audio;
using osu.Framework.Audio.Mixing;
using ManagedBass;
using System.Diagnostics;

namespace osu.Game.Audio
{
    public class ReplayGainStore : MutableDatabaseBackedStore<ReplayGainInfo>
    {
        /*private const string storage_directory = "replayGain";
        private readonly Storage replayGainStorage;*/
        private ReplayGainInfo curr;
        private AudioMixer audioMixer;
        public ReplayGainStore(IDatabaseContextFactory factory, AudioManager audioManager, Storage storage = null) :
            base(factory, storage)
        {
            audioMixer = audioManager.TrackMixer;
            //replayGainStorage = storage?.GetStorageForDirectory(storage_directory);
            ItemUpdated += UpdateReplay;
        }

        public void AddFx(IEffectParameter effectParameter)
        {
            IEffectParameter effect = audioMixer.Effects.SingleOrDefault(e => e.FXType == effectParameter.FXType);
            if (effect != null)
            {
                int i = audioMixer.Effects.IndexOf(effect);
                audioMixer.Effects[i] = effectParameter;
            }
            else
            {
                audioMixer.Effects.Add(effectParameter);
            }
            
        }

        public void UpdateReplay(ReplayGainInfo replayGainInfo)
        {
            curr = replayGainInfo;
        }

        public void Add(ReplayGainInfo info, BeatmapInfo beatmap)
        {
            Add(info);
            beatmap.ReplayGainInfoID = curr != null ? curr.ID : 0;
            beatmap.ReplayGainInfo = info;
        }

        public IQueryable<ReplayGainInfo> replayGainInfos => ContextFactory.Get().ReplayGainInfo;
        /*protected override IQueryable<ReplayGainInfo> AddIncludesForConsumption(IQueryable<ReplayGainInfo> query)
            => base.AddIncludesForConsumption(query)
                .Include(s => s.BeatmapInfo)
                .Include(s => s.BeatmapInfo).ThenInclude(b => b.Metadata)
                .Include(s => s.BeatmapInfo).ThenInclude(b => b.BeatmapSet).ThenInclude(s => s.Metadata);*/
    }
}
