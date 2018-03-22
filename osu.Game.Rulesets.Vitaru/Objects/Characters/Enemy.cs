using OpenTK;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Vitaru.UI;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Vitaru.Objects.Drawables;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Framework.Platform;

namespace osu.Game.Rulesets.Vitaru.Objects.Characters
{
    public class Enemy : VitaruCharacter
    {
        private readonly GraphicsPresets currentSkin = VitaruSettings.VitaruConfigManager.GetBindable<GraphicsPresets>(VitaruSetting.GraphicsPresets);

        public static int EnemyCount;
        private readonly DrawablePattern drawablePattern;

        public Enemy(Container parent, Pattern pattern, DrawablePattern drawablePattern) : base(parent)
        {
            this.drawablePattern = drawablePattern;
            AlwaysPresent = true;
            CharacterName = "enemy";
            Team = 1;
            CharacterColor = drawablePattern.AccentColour;
            HitboxWidth = 27;
        }

        protected override void LoadComplete()
        {
            EnemyCount++;

            if (currentSkin == GraphicsPresets.StandardCompetitive)
                VisibleHitbox.Alpha = 0.2f;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (isDisposing)
                EnemyCount--;
        }

        protected override void MovementAnimations()
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
            LastX = Position.X;
        }

        protected override void LoadAnimationSprites(TextureStore textures, Storage storage)
        {
            base.LoadAnimationSprites(textures, storage);
            CharacterRightSprite.Texture = VitaruSkinElement.LoadSkinElement(CharacterName, storage);
            CharacterKiaiRightSprite.Texture = VitaruSkinElement.LoadSkinElement(CharacterName + "Kiai", storage);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            if (effectPoint.KiaiMode && CharacterSprite.Alpha == 1)
            {
                CharacterSprite.FadeOutFromOne(timingPoint.BeatLength / 4);
                CharacterKiai.FadeInFromZero(timingPoint.BeatLength / 4);
            }
            if (!effectPoint.KiaiMode && CharacterSprite.Alpha == 0)
            {
                CharacterSprite.FadeInFromZero(timingPoint.BeatLength);
                CharacterKiai.FadeOutFromOne(timingPoint.BeatLength);
            }
        }

        public override void Death()
        {
            Dead = true;
            drawablePattern.PrepPop();
            Hitbox.HitDetection = false;
        }
    }
}
