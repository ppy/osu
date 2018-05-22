using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using OpenTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class ExternalLinkButton : CompositeDrawable, IHasTooltip
    {
        public string Link { get; set; }

        private Color4 hoverColour;

        public ExternalLinkButton(string link = null)
        {
            Link = link;
            InternalChild = new SpriteIcon
            {
                Icon = FontAwesome.fa_external_link,
                RelativeSizeAxes = Axes.Both
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            hoverColour = colours.Yellow;
        }

        protected override bool OnHover(InputState state)
        {
            InternalChild.FadeColour(hoverColour, 500, Easing.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            InternalChild.FadeColour(Color4.White, 500, Easing.OutQuint);
            base.OnHoverLost(state);
        }

        protected override bool OnClick(InputState state)
        {
            if(Link != null)
                Process.Start(new ProcessStartInfo
                {
                    FileName = Link,
                    UseShellExecute = true //see https://github.com/dotnet/corefx/issues/10361
                });
            return true;
        }

        public string TooltipText => "View in browser";
    }
}
