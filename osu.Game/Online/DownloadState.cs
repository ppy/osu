// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Online
{
    public enum DownloadState
    {
        [Description(@"下载未开始")]
        NotDownloaded,
        [Description(@"正在下载")]
        Downloading,
        [Description(@"已下载")]
        Downloaded,
        [Description(@"本地可用")]
        LocallyAvailable
    }
}
