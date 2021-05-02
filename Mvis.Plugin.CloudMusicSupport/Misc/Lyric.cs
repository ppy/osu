namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class Lyric
    {
        public int Time;
        public string Content;
        public string TranslatedString;

        public override string ToString() => $"{Time}:{Content}({TranslatedString})";
    }
}
