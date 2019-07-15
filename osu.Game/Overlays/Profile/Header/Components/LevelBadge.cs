// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class LevelBadge : CompositeDrawable, IHasTooltip
    {
        public readonly Bindable<UserStatistics> Statistics = new Bindable<UserStatistics>();

        public string TooltipText { get; }

        private OsuSpriteText levelText;

        public LevelBadge()
        {
            TooltipText = "Level";
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

            Statistics.BindValueChanged(statistics => updateStatistics(statistics.NewValue));
        }

        private void updateStatistics(UserStatistics statistics)
        {
            levelText.Text = statistics?.Level.Current.ToString() ?? "0";
        }
    }
}
