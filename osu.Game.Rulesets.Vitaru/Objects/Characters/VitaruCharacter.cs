using OpenTK;
using osu.Framework.Graphics;
using OpenTK.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Vitaru.Objects.Drawables;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Vitaru.UI;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions.Color4Extensions;
using Container = osu.Framework.Graphics.Containers.Container;
using Symcol.Core.GameObjects;
using System.ComponentModel;
using osu.Framework.Platform;

namespace osu.Game.Rulesets.Vitaru.Objects.Characters
{
    public abstract class VitaruCharacter : BeatSyncedContainer
    {
        protected Sprite CharacterStillSprite;
        protected Sprite CharacterRightSprite;
        protected Sprite CharacterLeftSprite;
        protected Sprite CharacterKiaiStillSprite;
        protected Sprite CharacterKiaiRightSprite;
        protected Sprite CharacterKiaiLeftSprite;
        protected Sprite CharacterSign;
        protected Container CharacterKiai;
        protected Container CharacterSprite;
        public Color4 CharacterColor;
        protected string CharacterName = "null";
        public float HitboxWidth { get; set; } = 4;
        protected CircularContainer VisibleHitbox;
        public SymcolHitbox Hitbox;
        public bool CanHeal = false;
        protected float LastX;

        /// <summary>
        /// Should be assigned to only in ctor, and is essential for hit detection
        /// </summary>
        public new readonly Container Parent;

        protected VitaruCharacter(Container parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// Does animations to better give the illusion of movement (could likely be cleaned up)
        /// </summary>
        protected virtual void MovementAnimations()
        {
            if (CharacterLeftSprite.Texture == null && CharacterRightSprite != null)
            {
                CharacterLeftSprite.Texture = CharacterRightSprite.Texture;
                CharacterLeftSprite.Size = new Vector2(-CharacterLeftSprite.Size.X, CharacterLeftSprite.Size.Y);
            }
            if (CharacterKiaiLeftSprite.Texture == null && CharacterKiaiRightSprite != null)
            {
                CharacterKiaiLeftSprite.Texture = CharacterKiaiRightSprite.Texture;
                CharacterKiaiLeftSprite.Size = new Vector2(-CharacterKiaiLeftSprite.Size.X, CharacterKiaiLeftSprite.Size.Y);
            }
            if (Position.X > LastX)
            {
                if (CharacterLeftSprite.Texture != null)
                    CharacterLeftSprite.Alpha = 0;
                if (CharacterRightSprite?.Texture != null)
                    CharacterRightSprite.Alpha = 1;
                if (CharacterStillSprite.Texture != null)
                    CharacterStillSprite.Alpha = 0;
                if (CharacterKiaiLeftSprite.Texture != null)
                    CharacterKiaiLeftSprite.Alpha = 0;
                if (CharacterKiaiRightSprite?.Texture != null)
                    CharacterKiaiRightSprite.Alpha = 1;
                if (CharacterKiaiStillSprite.Texture != null)
                    CharacterKiaiStillSprite.Alpha = 0;
            }
            else if (Position.X < LastX)
            {
                if (CharacterLeftSprite.Texture != null)
                    CharacterLeftSprite.Alpha = 1;
                if (CharacterRightSprite?.Texture != null)
                    CharacterRightSprite.Alpha = 0;
                if (CharacterStillSprite.Texture != null)
                    CharacterStillSprite.Alpha = 0;
                if (CharacterKiaiLeftSprite.Texture != null)
                    CharacterKiaiLeftSprite.Alpha = 1;
                if (CharacterKiaiRightSprite?.Texture != null)
                    CharacterKiaiRightSprite.Alpha = 0;
                if (CharacterKiaiStillSprite.Texture != null)
                    CharacterKiaiStillSprite.Alpha = 0;
            }
            else
            {
                if (CharacterLeftSprite.Texture != null)
                    CharacterLeftSprite.Alpha = 0;
                if (CharacterRightSprite?.Texture != null)
                    CharacterRightSprite.Alpha = 0;
                if (CharacterStillSprite.Texture != null)
                    CharacterStillSprite.Alpha = 1;
                if (CharacterKiaiLeftSprite.Texture != null)
                    CharacterKiaiLeftSprite.Alpha = 0;
                if (CharacterKiaiRightSprite?.Texture != null)
                    CharacterKiaiRightSprite.Alpha = 0;
                if (CharacterKiaiStillSprite.Texture != null)
                    CharacterKiaiStillSprite.Alpha = 1;
            }
            LastX = Position.X;
        }

        protected override void Update()
        {
            base.Update();

            if (Health <= 0 && !Dead)
                Death();

            foreach (Drawable draw in Parent)
            {
                DrawableBullet bullet = draw as DrawableBullet;
                if (bullet?.Hitbox != null)
                {
                    ParseBullet(bullet);
                    if (Hitbox.HitDetect(Hitbox, bullet.Hitbox))
                    {
                        Damage(bullet.Bullet.BulletDamage);
                        bullet.Bullet.BulletDamage = 0;
                        bullet.Hit = true;
                    }
                }

                DrawableSeekingBullet seekingBullet = draw as DrawableSeekingBullet;
                if (seekingBullet?.Hitbox != null)
                {
                    if (Hitbox.HitDetect(Hitbox, seekingBullet.Hitbox))
                    {
                        Damage(seekingBullet.SeekingBullet.BulletDamage);
                        seekingBullet.SeekingBullet.BulletDamage = 0;
                        seekingBullet.Hit = true;
                    }
                }

                DrawableLaser laser = draw as DrawableLaser;
                    if (laser?.Hitbox != null)
                    {
                        if (Hitbox.HitDetect(Hitbox, laser.Hitbox))
                        {
                            Damage(laser.Laser.LaserDamage * (1000 / (float)Clock.ElapsedFrameTime));
                            laser.Hit = true;
                        }
                    }
                }

            MovementAnimations();
        }

        /// <summary>
        /// Gets called just before hit detection
        /// </summary>
        protected virtual void ParseBullet(DrawableBullet bullet) { }

        protected virtual void LoadAnimationSprites(TextureStore textures, Storage storage) { }

        /// <summary>
        /// Child loading for all Characters (Enemies, Player, Bosses)
        /// </summary>
        [BackgroundDependencyLoader]
        private void load(TextureStore textures, Storage storage)
        {
            Health = MaxHealth;
            //Drawable stuff loading
            Origin = Anchor.Centre;
            Anchor = Anchor.TopLeft;
            Children = new Drawable[]
            {
                CharacterSign = new Sprite
                {
                    Alpha = 0,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = CharacterColor,
                },
                CharacterSprite = new Container
                {
                    Colour = CharacterColor,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 1,
                    Children = new Drawable[]
                    {
                        CharacterStillSprite = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 1,
                        },
                        CharacterRightSprite = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                        },
                        CharacterLeftSprite = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                        },
                    }
                },
                CharacterKiai = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        CharacterKiaiStillSprite = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 1,
                        },
                        CharacterKiaiRightSprite = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                        },
                        CharacterKiaiLeftSprite = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                        },
                    }
                },
                VisibleHitbox = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Size = new Vector2(HitboxWidth),
                    BorderColour = CharacterColor,
                    BorderThickness = HitboxWidth / 3,
                    Masking = true,

                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    EdgeEffect = new EdgeEffectParameters
                    {
                        
                        Radius = HitboxWidth,
                        Type = EdgeEffectType.Shadow,
                        Colour = CharacterColor.Opacity(0.5f)
                    }
                }
            };

            Add(Hitbox = new SymcolHitbox(new Vector2(HitboxWidth)) { Team = Team });

            if (CharacterName == "player" || CharacterName == "enemy")
                CharacterKiai.Colour = CharacterColor;

            CharacterStillSprite.Texture = VitaruSkinElement.LoadSkinElement(CharacterName, storage);
            CharacterKiaiStillSprite.Texture = VitaruSkinElement.LoadSkinElement(CharacterName + "Kiai", storage);
            CharacterSign.Texture = VitaruSkinElement.LoadSkinElement("sign", storage);
            LoadAnimationSprites(textures, storage);
        }

        #region eden.Game.GamePieces.Character.cs
        /// <summary>
        /// Maximum health this charcter can have
        /// </summary>
        public float MaxHealth = 100;

        /// <summary>
        /// The team this character is on, used mostly for Hitbox
        /// </summary>
        public int Team { get; set; }

        /// <summary>
        /// If this character has hit 0 health
        /// </summary>
        public bool Dead;

        /// <summary>
        /// the amount of health this character has
        /// </summary>
        public float Health;

        /// <summary>
        /// Removes "damage"
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        public virtual float Damage(float damage)
        {
            Health -= damage;

            if (Health < 0)
            {
                Health = 0;
                Death();
            }

            return Health;
        }

        /// <summary>
        /// Adds "health"
        /// </summary>
        /// <param name="health"></param>
        /// <returns></returns>
        public virtual float Heal(float health)
        {
            if (Health <= 0 && health > 0)
                Revive();

            Health += health;

            if (Health > MaxHealth)
                Health = MaxHealth;

            return Health;
        }

        /// <summary>
        /// Called when this character runs out of health
        /// </summary>
        public virtual void Death()
        {
            Dead = true;
            Expire();
        }

        public virtual void Revive()
        {
            Dead = false;
        }
        #endregion
    }
}
