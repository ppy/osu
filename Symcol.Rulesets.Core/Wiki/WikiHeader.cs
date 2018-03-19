using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Users;
using osu.Game.Graphics.Containers;
using OpenTK;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;
using Symcol.Rulesets.Core.Containers;

namespace Symcol.Rulesets.Core.Wiki
{
    public abstract class WikiHeader : Container
    {
        protected abstract Texture RulesetIcon { get; }

        protected abstract string RulesetName { get; }

        protected abstract string RulesetDescription { get; }

        protected virtual string RulesetUrl => $@"https://osu.ppy.sh/home";

        protected virtual User Creator => null;

        protected virtual User Maintainer => null;

        protected virtual string DiscordInvite => $@"https://discord.gg/ppy";

        protected virtual Texture HeaderBackground => null;

        private const float description_height = 150;
        private const float description_width = 220;
        private const float icon_size = 200;
        private const float header_margin = 50;
        private const float rulesetname_height = 60;

        public WikiHeader()
        {
            Masking = true;
            RelativeSizeAxes = Axes.X;
            Height = header_margin + icon_size + rulesetname_height;


            var user = Creator;
            bool maintainer = false;
            string userTitle = "Creator";
            if (Creator == null)
            {
                user = Maintainer;
                maintainer = true;
                userTitle = "Maintainer";
            }

            Children = new Drawable[]
            {
                new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode  = FillMode.Fill,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = HeaderBackground
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                new Sprite
                {
                    Size = new Vector2(icon_size),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Texture = RulesetIcon
                },
                new LinkText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(10, icon_size),
                    Text = RulesetName,
                    Url = RulesetUrl,
                    Font = @"Exo2.0-RegularItalic",
                    TextSize = rulesetname_height
                },
                new ProfileLink(user, maintainer)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(10, icon_size + rulesetname_height),
                },
                new LinkText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(10, icon_size + rulesetname_height + 20),
                    Text = userTitle + "'s Discord server",
                    Url = DiscordInvite,
                    Font = @"Exo2.0-RegularItalic",
                    TextSize = 16
                },
                new OsuTextFlowContainer(t => { t.TextSize = 20; })
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Size = new Vector2(description_width, description_height),
                    Text = RulesetDescription
                }
            };
        }
    }
}
