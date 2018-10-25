using osu.Core.Wiki;
using osu.Core.Wiki.Sections;

namespace osu.Mods.MapMixer
{
    public class MapMixerWikiSet : WikiSet
    {
        public override string Name => "Map Mixer";

        public override string Description => "The map mixer is a screen dedicated to just messing around with hitsounds while listening to music, also allows you to change songe speed and pitch.";

        public override string IndexTooltip => "\"how to mix maps\"";

        public override WikiSection[] GetSections() => null;
    }
}
