// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax.Inlines;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Cursor;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Wiki.Markdown
{
    public class WikiMarkdownImage : MarkdownImage, IHasTooltip
    {
        private readonly string url;

        public string TooltipText { get; }

        public WikiMarkdownImage(LinkInline linkInline)
            : base(linkInline.Url)
        {
            url = linkInline.Url;
            TooltipText = linkInline.Title;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            // The idea is replace "{api.WebsiteRootUrl}/wiki/{path-to-image}" to "{api.WebsiteRootUrl}/wiki/images/{path-to-image}"
            // "/wiki/images/*" is route to fetch wiki image from osu!web server (see: https://github.com/ppy/osu-web/blob/4205eb66a4da86bdee7835045e4bf28c35456e04/routes/web.php#L289)
            // Currently all image in dev server (https://dev.ppy.sh/wiki/image/*) is 404
            // So for now just replace "{api.WebsiteRootUrl}/wiki/*" to "https://osu.ppy.sh/wiki/images/*" for simplicity
            var imageUrl = url.Replace($"{api.WebsiteRootUrl}/wiki", "https://osu.ppy.sh/wiki/images");

            InternalChild = new DelayedLoadWrapper(CreateImageContainer(imageUrl));
        }
    }
}
