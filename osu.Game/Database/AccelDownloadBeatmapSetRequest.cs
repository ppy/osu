using System;
using System.Collections.Generic;
using osu.Framework.IO.Network;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Configuration.AccelUtils;
using osu.Game.Online.API;

namespace osu.Game.Database
{
    public class AccelDownloadBeatmapSetRequest : ArchiveDownloadRequest<IBeatmapSetInfo>
    {
        private readonly bool minimiseDownloadSize;

        public AccelDownloadBeatmapSetRequest(IBeatmapSetInfo set, bool minimiseDownloadSize)
            : base(set)
        {
            this.minimiseDownloadSize = minimiseDownloadSize;
            var config = MConfigManager.GetInstance();

            var dict = new Dictionary<string, object>
            {
                ["BID"] = Model.OnlineID,
                ["NOVIDEO"] = minimiseDownloadSize
            };

            if (!config.Get<string>(MSetting.AccelSource).TryParseAccelUrl(dict, out uri, out _))
                throw new ParseFailedException("加速地址解析失败, 请检查您的设置。");
        }

        private string getTarget() => $@"{(minimiseDownloadSize ? "novideo" : "full")}/{Model.OnlineID}";

        private readonly string uri;

        protected override string Target => getTarget();

        protected override string Uri => uri;

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Timeout = 60000;
            return req;
        }

        protected override string FileExtension => ".osz";
    }

    public class ParseFailedException : Exception
    {
        public ParseFailedException(string s)
            : base(s)
        {
        }
    }
}
