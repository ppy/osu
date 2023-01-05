using Mvis.Plugin.CloudMusicSupport.Misc;
using osu.Game.Online.API;

namespace Mvis.Plugin.CloudMusicSupport.Helper
{
    public class APILyricRequest : OsuJsonWebRequest<APILyricResponseRoot>
    {
        public APILyricRequest(int id)
        {
            Url = $"https://music.163.com/api/song/lyric?os=pc&id={id}&lv=-1&kv=-1&tv=-1";
        }
    }
}
