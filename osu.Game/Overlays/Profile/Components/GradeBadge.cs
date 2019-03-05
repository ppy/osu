// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Profile.Components
{
    public class GradeBadge : Container
    {
        private const float width = 50;
        private readonly string grade;
        private readonly Sprite badge;
        private readonly SpriteText numberText;

        public int DisplayCount
        {
            set => numberText.Text = value.ToString(@"#,0");
        }

        public GradeBadge(string grade)
        {
            this.grade = grade;
            Width = width;
            Height = 41;
            Add(badge = new Sprite
            {
                Width = width,
                Height = 26
            });
            Add(numberText = new OsuSpriteText
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold)
            });
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            badge.Texture = textures.Get($"Grades/{grade}");
        }
    }
}
