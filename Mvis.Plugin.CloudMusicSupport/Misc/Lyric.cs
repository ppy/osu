using System;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class Lyric : IEquatable<Lyric>
    {
        public double Time;
        public string Content = string.Empty;
        public string TranslatedString = string.Empty;

        public override string ToString() => $"{Time}:{Content}({TranslatedString})";

        public Lyric GetCopy()
        {
            return new Lyric
            {
                Time = Time,
                Content = Content,
                TranslatedString = TranslatedString
            };
        }

        public bool Equals(Lyric other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Time == other.Time && Content == other.Content && TranslatedString == other.TranslatedString;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            return Equals((Lyric)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Time, Content, TranslatedString);
        }
    }
}
