using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Toolbar;

namespace osu.Core.Containers.SymcolToolbar
{
    public class SymcolModdedToolbar : Toolbar
    {
        private ToolbarUserArea userArea;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            restart:
            foreach (Drawable draw in Children)
                if (draw is FillFlowContainer flow)
                {
                    Remove(flow);
                    goto restart;
                }

            AddRange(new Drawable[]
            {
                new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new ToolbarSettingsButton(),
                        new ToolbarHomeButton
                        {
                            Action = () => OnHome?.Invoke()
                        },
                        new ToolbarRulesetSelector(), 
                    }
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new ToolbarSystemClock(),
                        new ToolBarWikiButton(), 
                        new ToolbarDirectButton(),
                        new ToolbarChatButton(),
                        new ToolbarSocialButton(),
                        new ToolbarMusicButton(),
                        //new ToolbarButton
                        //{
                        //    Icon = FontAwesome.fa_search
                        //},
                        userArea = new ToolbarUserArea(),
                        new ToolbarNotificationButton(),
                    }
                }
            });
        }

        protected override void PopOut()
        {
            base.PopOut();
            userArea?.LoginOverlay.Hide();
        }
    }
}
