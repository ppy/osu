using osu.Core.Wiki.Included.Home;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Menu;
using OpenTK;
using OpenTK.Graphics;
using Symcol.Base.Graphics.Containers;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Core.Wiki.Header
{
    public class WikiHeader : Container
    {
        public readonly Bindable<WikiSet> CurrentWikiSet = new Bindable<WikiSet>();

        public readonly HomeWikiSet Home = new HomeWikiSet();

        private readonly BufferedContainer backgroundBlurContainer;

        private readonly OsuSpriteText name;
        private readonly Sprite background;
        private readonly Sprite icon;
        private readonly DeadContainer logo;
        private readonly OsuTextFlowContainer description;

        private readonly BreadcrumbControl<BreadCrumbState> breadcrumbs;
        private readonly WikiIndex index;

        public WikiHeader()
        {
            Masking = true;
            RelativeSizeAxes = Axes.X;
            Height = 310;

            Children = new Drawable[]
            {
                backgroundBlurContainer = new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,

                    Child = background = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode  = FillMode.Fill,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                icon = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode  = FillMode.Fit,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                logo = new DeadContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode  = FillMode.Fit,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Child = new OsuLogo
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(0.6f)
                    }
                },
                name = new OsuSpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    TextSize = 48
                },
                breadcrumbs = new BreadcrumbControl<BreadCrumbState>
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,

                    Position = new Vector2(0, 48),
                    RelativeSizeAxes = Axes.X,
                    Width = 0.5f
                },
                index = new WikiIndex(),
                description = new OsuTextFlowContainer(t => { t.TextSize = 32; })
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,

                    RelativeSizeAxes = Axes.Both,
                    Width = 0.25f
                }
            };

            breadcrumbs.Current.ValueChanged += value =>
            {
                switch (value)
                {
                    case BreadCrumbState.Home:
                        CurrentWikiSet.Value = Home;
                        break;
                }
            };

            CurrentWikiSet.BindTo(index.CurrentWikiSet);

            CurrentWikiSet.ValueChanged += value =>
            {
                name.Text = value.Name;
                description.Text = value.Description;

                if (value.HeaderBackground != null)
                    background.Texture = value.HeaderBackground;

                backgroundBlurContainer.BlurTo(value.HeaderBackgroundBlur, 500);

                if (value.Icon != null)
                    icon.Texture = value.Icon;

                if (value != Home)
                {
                    breadcrumbs.Current.Value = BreadCrumbState.Wiki;
                    icon.Show();
                    logo.Hide();
                }
                else
                {
                    icon.Hide();
                    logo.Show();
                }
            };
        }
    }

    public enum BreadCrumbState
    {
        Home,
        Wiki
    }
}
