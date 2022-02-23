// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Logging;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public static class StringExtensions
    {
        public static int toMS(this string src)
        {
            int result;
            string[] source = src.Split('.');

            try
            {
                result = int.Parse(source.ElementAtOrDefault(0) ?? "0") * 60000
                         + int.Parse(source.ElementAtOrDefault(1) ?? "0") * 1000
                         + int.Parse(source.ElementAtOrDefault(2) ?? "0");
            }
            catch (Exception e)
            {
                string reason = e.Message;

                if (e is FormatException)
                    reason = "格式有误, 请检查原歌词是否正确";

                Logger.Error(e, $"无法将\"{src}\"转换为歌词时间: {reason}");
                result = int.MaxValue;
            }

            return result;
        }
    }
}
