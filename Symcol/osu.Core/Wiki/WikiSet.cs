using osu.Core.Wiki.Sections;
using osu.Framework.Graphics.Textures;
using OpenTK;

namespace osu.Core.Wiki
{
    public abstract class WikiSet
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public virtual string IndexTooltip => "";

        public virtual Texture Icon => null;

        public virtual Texture HeaderBackground => null;

        public virtual Vector2 HeaderBackgroundBlur => new Vector2(5);

        public abstract WikiSection[] GetSections();
    }
}
