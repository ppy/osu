// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Framework.Bindables;

namespace osu.Game.Online.API.Requests
{
    public class DownloadBeatmapSetRequest : ArchiveDownloadRequest<BeatmapSetInfo>
    {
        private readonly bool noVideo;
        private Bindable<bool> useSayobot = new Bindable<bool>();

        [Resolved]
        private OsuConfigManager config { get; set; }

        public DownloadBeatmapSetRequest(BeatmapSetInfo set, bool noVideo)
            : base(set)
        {
            this.noVideo = noVideo;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.DownloadFromSayobot, useSayobot);
        }

        private void UpdateBindValues()
        {
            useSayobot = config.GetBindable<bool>(OsuSetting.DownloadFromSayobot);
        }

        private string CalcSayoUri()
        {
            UpdateBindValues();
            int IdFull = (int)Model.OnlineBeatmapSetID;
            int IdHead = (int)Math.Floor(IdFull / 10000f);
            int IdTail = (int)IdFull - (IdHead * 10000);

            var SayoTarget = $@"{IdHead}/{IdTail}/{( noVideo? "novideo" : "full")}?filename=b-{IdFull}";
            var ppyTarget =  $@"{Model.OnlineBeatmapSetID}/download{(noVideo ? "?noVideo=1" : "")}";

            var Target = $"{( useSayobot.Value == true? $"{SayoTarget}" : $"{ppyTarget}" )}";
            return Target;
        }

        private string UpdateUriRoot()
        {
            UpdateBindValues();
            var uri = $"{( useSayobot.Value == true? $"https://b2.sayobot.cn:25225/beatmaps/{Target}" : $"{API.Endpoint}/api/v2/beatmapsets/{Target}" )}";
            return uri;
        }

        protected override string Target => $@"{CalcSayoUri()}";

        protected override string Uri => $@"{UpdateUriRoot()}";
    }
}
