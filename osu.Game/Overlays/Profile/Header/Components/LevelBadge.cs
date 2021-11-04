// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class LevelBadge : CompositeDrawable, IHasTooltip
    {
        public readonly Bindable<APIUser> User = new Bindable<APIUser>();

        public LocalisableString TooltipText { get; private set; }

        private OsuSpriteText levelText;

        public LevelBadge()
        {
            TooltipText = UsersStrings.ShowStatsLevel("0");
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures)
        {
            InternalChildren = new Drawable[]
            {
                new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = textures.Get("Profile/levelbadge"),
                    Colour = colours.Yellow,
                },
                levelText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(size: 20)
                }
            };

            User.BindValueChanged(user => updateLevel(user.NewValue));
        }

        private void updateLevel(APIUser user)
        {
            levelText.Text = user?.Statistics?.Level.Current.ToString() ?? "0";
            TooltipText = UsersStrings.ShowStatsLevel(user?.Statistics?.Level.Current.ToString());
        }
    }
}
