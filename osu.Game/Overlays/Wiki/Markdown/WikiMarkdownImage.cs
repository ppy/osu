// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax.Inlines;
using osu.Game.Graphics.Containers.Markdown;

namespace osu.Game.Overlays.Wiki.Markdown
{
    public partial class WikiMarkdownImage : OsuMarkdownImage
    {
        public WikiMarkdownImage(LinkInline linkInline)
            : base(linkInline)
        {
        }

        protected override ImageContainer CreateImageContainer(string url)
        {
            // The idea is replace "https://website.url/wiki/{path-to-image}" to "https://website.url/wiki/images/{path-to-image}"
            // "/wiki/images/*" is route to fetch wiki image from osu!web server (see: https://github.com/ppy/osu-web/blob/4205eb66a4da86bdee7835045e4bf28c35456e04/routes/web.php#L289)
            url = url.Replace("/wiki/", "/wiki/images/");

            return base.CreateImageContainer(url);
        }
    }
}
