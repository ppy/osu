using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Vitaru.Objects.Characters;
using osu.Game.Modes.Vitaru.Objects.Drawables.Pieces;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Modes.Vitaru.Objects.Drawables
{
    class DrawableEnemy : Container
    {

        private EnemySprite enemySprite;
        private Container enemyRipple;
        private Sprite eRipple;

        public DrawableEnemy()
        {
            Children = new Drawable[]
            {
                enemySprite = new EnemySprite()
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                },
                
                //(Kiai animation) Tried to replicate how the osu logo acts on title screen (pulses to bpm)
                enemyRipple = new Container()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        eRipple = new Sprite()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            BlendingMode = BlendingMode.Additive,
                            Scale = new Vector2(0.5f),
                            Alpha = 0.15f
                        }
                    }
                }
            };
        }

        public override bool Contains(Vector2 screenSpacePos) => true;



        protected override void Update()
        {

            base.Update();
        }

        public void setKiai(bool visible)
        {
            enemyRipple.Alpha = visible ? 1 : 0;
            eRipple.ScaleTo(eRipple.Scale * 1.1f, 500);
            eRipple.FadeOut(500);
            eRipple.Loop(300);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            eRipple.Texture = textures.Get(@"Play/Vitaru/enemy");
        }
    }
}
