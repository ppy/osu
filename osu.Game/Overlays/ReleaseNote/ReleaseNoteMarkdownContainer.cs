using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.ReleaseNote
{
    public class ReleaseNoteMarkdownContainer : OsuMarkdownContainer
    {
        public override SpriteText CreateSpriteText() => new OsuSpriteText
        {
            Font = OsuFont.GetFont(Typeface.Inter, weight: FontWeight.Regular, size: 18)
        };
    }
}
