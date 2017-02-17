using System;
using osu.Framework.Input;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Vitaru.Objects.Drawables.Pieces;

namespace osu.Game.Modes.Vitaru.Objects.Drawables
{
    class DrawableBoss : Container
    {
        private BossSprite bossSprite;
        private SpellPiece bossSpell;

        public DrawableBoss()
        {
            Children = new Drawable[]
            {
                bossSprite = new BossSprite()
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                },
                bossSpell = new SpellPiece()
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 0,
                },
            };
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override void Update()
        {
            base.Update();
        }
        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {

        }
    }
}