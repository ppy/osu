using OpenTK;
using osu.Framework.Graphics;
using System.Collections.Generic;
using OpenTK.Graphics;
using System;
using osu.Game.Rulesets.Vitaru.Objects.Drawables;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Game.Rulesets.Vitaru.Scoring;
using osu.Framework.Audio;
using osu.Game.Rulesets.Vitaru.UI;
using osu.Framework.Timing;
using static osu.Game.Rulesets.Vitaru.UI.Cursor.GameplayCursor;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.Input.Bindings;
using osu.Game.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Rulesets.Vitaru.Objects.Characters.Pieces;
using osu.Game.Rulesets.Vitaru.Multi;
using Symcol.Core.Networking;
using System.ComponentModel;

namespace osu.Game.Rulesets.Vitaru.Objects.Characters
{
    public class VitaruPlayer : VitaruCharacter, IKeyBindingHandler<VitaruAction>
    {
        #region Fields
        private readonly Characters currentCharacter;
        private readonly GraphicsPresets currentSkin = VitaruSettings.VitaruConfigManager.GetBindable<GraphicsPresets>(VitaruSetting.GraphicsPresets);
        private readonly ScoringMetric currentScoringMetric = VitaruSettings.VitaruConfigManager.GetBindable<ScoringMetric>(VitaruSetting.ScoringMetric);
        private readonly VitaruGamemode currentGameMode = VitaruSettings.VitaruConfigManager.GetBindable<VitaruGamemode>(VitaruSetting.GameMode);

        public int ScoreZone = 100;

        public Dictionary<VitaruAction, bool> Actions = new Dictionary<VitaruAction, bool>();

        public VitaruNetworkingClientHandler VitaruNetworkingClientHandler { get; set; }

        public string PlayerID;

        //(MinX,MaxX,MinY,MaxY)
        private Vector4 playerBounds = new Vector4(0, 512, 0, 820);

        private const float player_speed = 0.25f;

        public bool Invert { get; set; }

        //Is not Human
        public bool Bot { get; set; }

        //Has a parent Player?
        public bool Clone { get; set; }

        /// <summary>
        /// Are we a slave online?
        /// </summary>
        public bool Puppet { get; set; }

        private readonly static List<VitaruPlayer> playerList = new List<VitaruPlayer>();

        private readonly Bindable<WorkingBeatmap> workingBeatmap = new Bindable<WorkingBeatmap>();
        private List<VitaruPlayer> cloneList = new List<VitaruPlayer>();
        private readonly List<Crystal> crystalList = new List<Crystal>();
        private VitaruPlayer parentPlayer;
        private const float field_of_view = 60;
        public float SpeedMultiplier = 1;
        private OsuTextFlowContainer textContainer;

        private List<UFO> ufoList = new List<UFO>();
        private Framework.Graphics.Containers.Container ufoContainer;
        private UFO ufoMark;
        private UFO ufoHealth;
        private UFO ufoEnergy;
        private UFO ufoDamage;
        private readonly float originalMaxHealth;

        private bool riftActive;
        private Rift riftStart;
        private Rift riftEnd;
        private double warpTime = double.MinValue;

        private bool vampuric;

        private DrawableLaser drawableLaser;

        private Totem leftTotem;
        private Totem rightTotem;

        private Metranome metranome;
        public int Combo;
        private float damageMultiplier = 1;
        private const float hitwindow = 40;

        //Automatic play, ignores player input
        public bool Auto { get; set; }

        private double packetTime = double.MinValue;
        private double lastQuarterBeat = -1;
        private double nextHalfBeat = -1;
        private double nextQuarterBeat = -1;
        private double beatLength = 1000;
        private bool leader;
        private double reFrozenTime = double.MaxValue;
        private double timeFreezeEndTime = double.MinValue;
        private double reFreezeTime = double.MaxValue;
        private float originalRate;
        public float SetRate = 0.2f;
        private float currentRate = 1;
        private bool timeFreezeActive;
        private bool tabooActive;
        private bool ghastlytActive;
        private bool shattered;
        private readonly float energyRequired = 50;
        private readonly float energyRequiredPerSecond;
        private readonly float maxEnergy = 100;
        private float healingMultiplier = 1;
        private readonly float energyGainMultiplier = 1;
        public float Energy;

        //For debug ui only
        public static float Energystored;
        #endregion

        #region Loading Stuff
        public VitaruPlayer(Framework.Graphics.Containers.Container parent, Characters characterOverride, VitaruPlayer parentPlayer = null) : base(parent)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            playerList.Add(this);

            currentCharacter = characterOverride;

            if (parentPlayer != null)
                this.parentPlayer = parentPlayer;

            Actions[VitaruAction.Up] = false;
            Actions[VitaruAction.Down] = false;
            Actions[VitaruAction.Left] = false;
            Actions[VitaruAction.Right] = false;
            Actions[VitaruAction.Slow] = false;
            Actions[VitaruAction.Fast] = false;
            Actions[VitaruAction.Shoot] = false;

            CharacterName = "player";
            Team = 0;
            MaxHealth = 100;
            Position = new Vector2(256, 700);

            switch (currentCharacter)
            {
                default:
                    CharacterColor = Color4.White;
                    break;
                /*
            case Characters.Alex:
                energyRequired = 20;
                maxEnergy = 40;
                CharacterColor = Color4.Gold;
                //CharacterName = "arysa";
                break;
                */
                case Characters.ReimuHakurei:
                    CharacterColor = Color4.Red;
                    CharacterName = "reimu";
                    break;
                case Characters.MarisaKirisame:
                    CharacterColor = Color4.Black;
                    CharacterName = "marisa";
                    energyRequired = 10;
                    break;
                case Characters.SakuyaIzayoi:
                    CharacterColor = Color4.Navy;
                    energyRequired = 2;
                    energyRequiredPerSecond = 4;
                    maxEnergy = 24;
                    CharacterName = "sakuya";
                    break;
                case Characters.HongMeiling:

                    if (!resurrected)
                        MaxHealth = 0;
                    else
                        MaxHealth = 20;

                    maxEnergy = 36;
                    leader = true;
                    CharacterColor = Color4.Orange;
                    break;
                case Characters.FlandreScarlet:
                    maxEnergy = 80;
                    energyRequired = 40;
                    CharacterColor = Color4.Red;
                    break;
                case Characters.RemiliaScarlet:
                    CharacterColor = Color4.Pink;
                    vampuric = true;
                    maxEnergy = 60;
                    MaxHealth = 60;
                    energyRequired = 1;
                    break;
                case Characters.Cirno:
                    MaxHealth = 80;
                    maxEnergy = 40;
                    energyRequired = 40;
                    CharacterColor = Color4.Blue;
                    break;
                case Characters.TenshiHinanai:
                    CharacterColor = Color4.DarkBlue;
                    break;
                case Characters.YuyukoSaigyouji:
                    CharacterColor = Color4.LightBlue;
                    maxEnergy = 24;
                    energyRequired = 2;
                    energyRequiredPerSecond = 2;
                    break;
                case Characters.YukariYakumo:
                    CharacterColor = Color4.DarkViolet;
                    maxEnergy = 24;
                    energyRequiredPerSecond = 4;
                    MaxHealth = 80;
                    energyRequired = 4;
                    break;
                case Characters.Chen:
                    CharacterColor = Color4.Green;
                    CharacterName = "chen";
                    break;
                case Characters.KokoroHatano:
                    CharacterColor = Color4.Cyan;
                    maxEnergy = 36;
                    break;
                case Characters.Kaguya:
                    CharacterColor = Color4.DarkRed;
                    CharacterName = "kaguya";
                    break;
                case Characters.IbarakiKasen:
                    CharacterColor = Color4.YellowGreen;
                    maxEnergy = 8;
                    energyRequired = 2;
                    MaxHealth = 40;
                    break;
                case Characters.NueHoujuu:
                    CharacterColor = Color4.DarkGray;
                    CharacterName = "nue";
                    MaxHealth = 80;
                    maxEnergy = 24;
                    energyRequired = 0;
                    break;
                case Characters.AliceMuyart:
                    MaxHealth = 200;
                    healingMultiplier = 2;
                    energyGainMultiplier = 2;
                    maxEnergy = 200;
                    energyRequired = 10;
                    energyRequiredPerSecond = 4;
                    CharacterColor = Color4.SkyBlue;
                    break;
                case Characters.ArysaMuyart:
                    break;
            }

            originalMaxHealth = MaxHealth;

            if (currentGameMode == VitaruGamemode.Dodge)
                playerBounds = new Vector4(0, 512, 0, 384);
        }

        protected override void LoadAnimationSprites(TextureStore textures, Storage storage)
        {
            base.LoadAnimationSprites(textures, storage);

            CharacterRightSprite.Texture = VitaruSkinElement.LoadSkinElement(CharacterName + "Right", storage);
            CharacterKiaiRightSprite.Texture = VitaruSkinElement.LoadSkinElement(CharacterName + "KiaiRight", storage);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (VitaruNetworkingClientHandler != null)
                VitaruNetworkingClientHandler.OnPacketReceive += (p) => packetReceived(p);

            if (Invert)
                Rotation += 180;

            if (currentSkin == GraphicsPresets.StandardCompetitive || currentSkin == GraphicsPresets.HighPerformanceCompetitive)
                VisibleHitbox.Alpha = 1;

            if (currentGameMode == VitaruGamemode.Touhosu)
            {
                if (currentCharacter == Characters.ReimuHakurei | currentCharacter == Characters.MarisaKirisame)
                {
                    AddRange(new Drawable[]
                    {
                        leftTotem = new Totem(this)
                        {
                            Position = new Vector2(-20, -30),
                            StartAngle = -20,
                        },
                        rightTotem = new Totem(this)
                        {
                            Position = new Vector2(20, -30),
                            StartAngle = 20,
                        }
                    });
                }

                Add(textContainer = new OsuTextFlowContainer(t => { t.TextSize = 24; })
                {
                    Alpha = 0,
                    Position = new Vector2(0, 48),
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                    Width = 100,
                    AutoSizeAxes = Axes.Both,
                    Text = ""
                });

                if (currentCharacter == Characters.Cirno)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        Crystal c = new Crystal { Position = new Vector2((float)RNG.NextDouble(-20, 20), (float)RNG.NextDouble(-40, 40)) };
                        crystalList.Add(c);
                        Add(c);
                    }
                }

                if (currentCharacter == Characters.YukariYakumo)
                {
                    Parent.AddRange(new Drawable[]
                    {
                        riftStart = new Rift(Color4.DarkViolet),
                        riftEnd = new Rift(Color4.DarkRed)
                    });

                    riftStart.LinkedRift = riftEnd;
                    riftEnd.LinkedRift = riftStart;
                }

                if (currentCharacter == Characters.KokoroHatano)
                {
                    Add(metranome = new Metranome());
                    Remove(CharacterSign);
                }

                Add(ufoContainer = new Framework.Graphics.Containers.Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                });

                if (currentCharacter == Characters.NueHoujuu)
                {
                    ufoMark = new UFO(this, UFOType.Mark) { Position = new Vector2(0, -60) };
                    ufoHealth = new UFO(this, UFOType.Health) { Position = new Vector2(-60, 0) };
                    ufoEnergy = new UFO(this, UFOType.Energy) { Position = new Vector2(60, 0) };
                    ufoDamage = new UFO(this, UFOType.Damage) { Position = new Vector2(0, 60) };

                    ufoList.Add(ufoMark);
                    ufoList.Add(ufoHealth);
                    ufoList.Add(ufoEnergy);
                    ufoList.Add(ufoDamage);

                    foreach (UFO ufo in ufoList)
                        ufoContainer.Add(ufo);
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
                playerList.Remove(this);
            base.Dispose(isDisposing);
        }
        #endregion

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            float amplitudeAdjust = Math.Min(1, 0.4f + amplitudes.Maximum);

            beatLength = timingPoint.BeatLength;

            if (!Clone && Bot && currentGameMode == VitaruGamemode.Touhosu)
                spell();

            if (Actions[VitaruAction.Shoot] && currentGameMode != VitaruGamemode.Dodge && currentCharacter == Characters.MarisaKirisame | currentCharacter == Characters.ReimuHakurei)
            {
                leftTotem.Shoot();
                rightTotem.Shoot();
            }

            onHalfBeat();
            lastQuarterBeat = Time.Current;
            nextHalfBeat = Time.Current + timingPoint.BeatLength / 2;
            nextQuarterBeat = Time.Current + timingPoint.BeatLength / 4;

            const double beat_in_time = 60;

            CharacterSign.ScaleTo(1 - 0.02f * amplitudeAdjust, beat_in_time, Easing.Out);
            using (CharacterSign.BeginDelayedSequence(beat_in_time))
                CharacterSign.ScaleTo(1, beatLength * 2, Easing.OutQuint);

            if (effectPoint.KiaiMode && currentGameMode != VitaruGamemode.Touhosu)
            {
                CharacterSign.FadeTo(0.25f * amplitudeAdjust, beat_in_time, Easing.Out);
                using (CharacterSign.BeginDelayedSequence(beat_in_time))
                    CharacterSign.FadeOut(beatLength);
            }

            if (effectPoint.KiaiMode && CharacterSprite.Alpha == 1)
            {
                if (!Dead)
                {
                    CharacterKiai.FadeInFromZero(timingPoint.BeatLength / 4);
                    CharacterSprite.FadeOutFromOne(timingPoint.BeatLength / 4);
                }

                if (currentGameMode != VitaruGamemode.Touhosu)
                    CharacterSign.FadeTo(0.15f , timingPoint.BeatLength / 4);
            }
            if(!effectPoint.KiaiMode && CharacterKiai.Alpha == 1)
            {
                if (!Dead)
                {
                    CharacterSprite.FadeInFromZero(timingPoint.BeatLength);
                    CharacterKiai.FadeOutFromOne(timingPoint.BeatLength);
                }

                if (currentGameMode != VitaruGamemode.Touhosu)
                    CharacterSign.FadeTo(0f, timingPoint.BeatLength);
            }
        }

        private void onHalfBeat()
        {
            nextHalfBeat = -1;

            if (Actions[VitaruAction.Shoot] && currentGameMode != VitaruGamemode.Dodge && currentCharacter == Characters.Cirno && !shattered)
                patternWave();
            else if (Actions[VitaruAction.Shoot] && currentGameMode != VitaruGamemode.Dodge && currentCharacter != Characters.Cirno)
                patternWave();

            if (CanHeal)
            {
                CanHeal = false;

                Heal(1 * healingMultiplier);

                if (currentGameMode != VitaruGamemode.Touhosu)
                {
                    CharacterSign.Alpha = 0.2f;
                    CharacterSign.FadeOut(beatLength / 2);
                }
            }
        }

        private void onQuarterBeat()
        {
            lastQuarterBeat = nextQuarterBeat;
            nextQuarterBeat += beatLength / 4;
        }

        protected override void Update()
        {
            base.Update();

            if (currentGameMode == VitaruGamemode.Touhosu)
            {
                speakingUpdate();
                spellUpdate();
            }

            playerInput();
            checkScoreZone();

            if (nextHalfBeat <= Time.Current && nextHalfBeat != -1)
                onHalfBeat();

            if (nextQuarterBeat <= Time.Current && nextQuarterBeat != -1)
                onQuarterBeat();

            if (CharacterSign.Alpha > 0)
                CharacterSign.RotateTo((float)(Clock.CurrentTime / 1000 * 90));

            if (VitaruNetworkingClientHandler != null && packetTime + 250 <= Time.Current)
            {
                packetTime = Time.Current;
                sendPacket();
            }
        }

        protected override void ParseBullet(DrawableBullet bullet)
        {
            base.ParseBullet(bullet);

            //Not sure why this offset is needed atm
            Vector2 object2Pos = bullet.ToSpaceOfOtherDrawable(Vector2.Zero, this) + new Vector2(6);
            float distance = (float)Math.Sqrt(Math.Pow(object2Pos.X, 2) + Math.Pow(object2Pos.Y, 2));
            float edgeDistance = distance - (bullet.Width / 2 + Hitbox.Width / 2);

            if (currentCharacter == Characters.YuyukoSaigyouji && ghastlytActive)
            {
                Hitbox.HitDetection = true;
                if (Hitbox.HitDetect(Hitbox, bullet.Hitbox) && bullet.Bullet.Ghost)
                {
                    Damage(bullet.Bullet.BulletDamage);
                    bullet.Bullet.BulletDamage = 0;
                    bullet.Hit = true;
                }
                Hitbox.HitDetection = false;
            }

            if (edgeDistance < 48 && bullet.Bullet.Team != Team)
                    CanHeal = true;
                
            if (currentScoringMetric == ScoringMetric.Graze)
            {
                if (currentGameMode == VitaruGamemode.Dodge)
                    distance *= 1.5f;
                if (distance <= 64 && bullet.ScoreZone < 300)
                    bullet.ScoreZone = 300;
                else if (distance <= 128 && bullet.ScoreZone < 200)
                    bullet.ScoreZone = 200;
                else if (distance <= 256 && bullet.ScoreZone < 100)
                    bullet.ScoreZone = 100;
                else if (bullet.ScoreZone < 50)
                    bullet.ScoreZone = 50;
            }
        }

        /// <summary>
        /// Check to see what kinda points we should award the player
        /// </summary>
        private void checkScoreZone()
        {
            if (currentScoringMetric != ScoringMetric.Graze)
            {
                var scoreZone = new Vector2(256, -512);
                var distance = (float)Math.Sqrt(Math.Pow(Position.X - scoreZone.X, 2) + Math.Pow(Position.Y - scoreZone.Y, 2));

                if (distance <= 1024 - 256 - 128)
                    ScoreZone = 0;
                else if (distance <= 1024 - 256)
                    ScoreZone = 100;
                else if (distance <= 1024 - 128)
                    ScoreZone = 200;
                else if (distance <= 1024)
                    ScoreZone = 300;
                else if (distance <= 1024 + 256)
                    ScoreZone = 200;
                else
                    ScoreZone = 100;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            workingBeatmap.BindTo(game.Beatmap);
        }

        #region Spell Stuff
        private void spell(bool keyUp = false, VitaruAction action = VitaruAction.Spell)
        {
            if (Energy >= energyRequired && currentGameMode == VitaruGamemode.Touhosu && !keyUp || currentCharacter == Characters.AliceMuyart && currentGameMode == VitaruGamemode.Touhosu && !keyUp)
            {
                //if (currentCharacter == Characters.Alex && action == VitaruAction.Spell)
                //alexSpell();
                if (currentCharacter == Characters.ReimuHakurei && action == VitaruAction.Spell)
                    reimuaSpell();
                else if (currentCharacter == Characters.MarisaKirisame && action == VitaruAction.Spell)
                    marisaSpell();
                else if (currentCharacter == Characters.SakuyaIzayoi && action == VitaruAction.Spell)
                    sakuyaSpell();
                else if (currentCharacter == Characters.FlandreScarlet && !Clone && !tabooActive && action == VitaruAction.Spell)
                    flandereSpell();
                else if (currentCharacter == Characters.RemiliaScarlet && action == VitaruAction.Spell)
                {

                }
                else if (currentCharacter == Characters.YuyukoSaigyouji && action == VitaruAction.Spell)
                {

                }
                else if (currentCharacter == Characters.YuyukoSaigyouji && !Clone && !ghastlytActive && action == VitaruAction.Spell)
                    yuyukoSpell();
                else if (currentCharacter == Characters.YukariYakumo && action == VitaruAction.Spell)
                    yukariSpell();
                else if (currentCharacter == Characters.Chen && action == VitaruAction.Spell)
                {

                }
                else if (currentCharacter == Characters.IbarakiKasen && action == VitaruAction.Spell)
                    ibarakiSpell();
                else if (currentCharacter == Characters.NueHoujuu && action == VitaruAction.Spell | action == VitaruAction.Spell2 | action == VitaruAction.Spell3 | action == VitaruAction.Spell4)
                    nueSpell(action);
                else if (currentCharacter == Characters.AliceMuyart && !Clone)
                {
                    switch (action)
                    {
                        case VitaruAction.Spell when Energystored > 2:
                            ibarakiSpell(2);
                            patternCircle();
                            break;
                        case VitaruAction.Spell3 when SetRate != 1 && Energystored > 6:
                            sakuyaSpell(6);
                            break;
                        case VitaruAction.Spell2 when !ghastlytActive && Energystored > 8:
                            yuyukoSpell(8);
                            break;
                    }
                }
            }
            else if (keyUp)
            {
                switch (currentCharacter)
                {
                    case Characters.SakuyaIzayoi when action == VitaruAction.Spell:
                        timeFreezeActive = false;
                        break;
                    case Characters.YukariYakumo when action == VitaruAction.Spell:
                        riftActive = false;
                        break;
                    case Characters.YuyukoSaigyouji when action == VitaruAction.Spell:
                        ghastlytActive = false;
                        break;
                    case Characters.AliceMuyart:
                        if (action == VitaruAction.Spell3)
                            timeFreezeActive = false;
                        else if (action == VitaruAction.Spell2)
                            ghastlytActive = false;
                        break;
                }
            }
        }

        private void reimuaSpell(float energyOverride = -1)
        {

        }

        private void marisaSpell(float energyOverride = -1)
        {
            if (energyOverride == -1)
                Energy -= energyRequired;
            else
                Energy -= energyOverride;

            Parent.Add(drawableLaser = new DrawableLaser(Parent, new Laser
            {
                LaserSize = new Vector2(80, 400),
                Team = Team,
                ComboColour = CharacterColor,
                StartTime = Time.Current,
                EndTime = Time.Current + 2000
            }));
            drawableLaser.Position = Position;
        }

        private void sakuyaSpell(float energyOverride = -1)
        {
            if (energyOverride == -1)
                Energy -= energyRequired;
            else
                Energy -= energyOverride;

            timeFreezeActive = true;

            if (originalRate == 0)
                originalRate = (float)workingBeatmap.Value.Track.Rate;

            currentRate = originalRate * SetRate;
            applyToClock(workingBeatmap.Value.Track, currentRate);

            timeFreezeEndTime = Time.Current + 1000;
        }

        private void flandereSpell(float energyOverride = -1)
        {
            if (energyOverride == -1)
                Energy -= energyRequired;
            else
                Energy -= energyOverride;

            tabooActive = true;
            for (int i = 1; i < 4; i++)
            {
                Vector2 position = new Vector2(-40, -20);
                if (i == 2)
                {
                    position = new Vector2(40, 0);
                }
                else if (i == 3)
                {
                    position = new Vector2(80, -20);
                }

                VitaruPlayer player;
                Parent.Add(player = new VitaruPlayer(Parent, currentCharacter, this)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Bot = true,
                    Auto = true,
                    Position = Position + position
                });
                cloneList.Add(player);
            }
        }

        private void remiliaSpell()
        {

        }

        private void yukariSpell()
        {
            riftActive = true;
            riftStart.FadeInFromZero(beatLength / 2);
            riftStart.Position = new Vector2(Position.X, Position.Y - 64);
            riftEnd.FadeInFromZero(beatLength / 2);
            riftEnd.Position = VitaruCursor.CenterCircle.ToSpaceOfOtherDrawable(Vector2.Zero, Parent);
        }

        private void kokoroSpell()
        {
            if (currentCharacter == Characters.KokoroHatano)
            {
                if (Time.Current <= lastQuarterBeat + hitwindow | Time.Current >= nextQuarterBeat - hitwindow)
                    Combo++;
                else
                    Combo = Math.Max(Combo -= 30, 0);

                if (Combo >= 10)
                    metranome.Alpha = Math.Min((Combo - 10) / 100, 0.5f);
                else
                    metranome.Alpha = 0;

                Energystored += 0.01f * Combo;
                damageMultiplier = Combo * 0.01f + 1;
                healingMultiplier = Combo * 0.01f + 1;
            }
        }

        private void yuyukoSpell(float energyOverride = -1)
        {
            if (energyOverride == -1)
                Energy -= energyRequired;
            else
                Energy -= energyOverride;

            ghastlytActive = true;
            this.FadeTo(0.5f, beatLength / 2);
            Hitbox.HitDetection = false;

            VitaruPlayer player;
            Parent.Add(player = new VitaruPlayer(Parent, currentCharacter, this)
            {
                Alpha = 0,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = Position,
                Auto = true,
                Bot = true,
                Clone = true
            });
            player.FadeIn(beatLength / 2);
            cloneList.Add(player);
        }

        private void ibarakiSpell(float energyOverride = -1)
        {
            if (energyOverride == -1)
                Energy -= energyRequired;
            else
                Energy -= energyOverride;

            Position = VitaruCursor.CenterCircle.ToSpaceOfOtherDrawable(Vector2.Zero, Hitbox);
        }

        private void nueSpell(VitaruAction action)
        {
            Energy -= energyRequired;

            VitaruPlayer closestPlayer = null;
            float closestPlayerDistance = 80;

            foreach (VitaruPlayer player in playerList)
            {
                Vector2 playerPos = VitaruCursor.CenterCircle.ToSpaceOfOtherDrawable(Vector2.Zero, player) + new Vector2(6);
                float distance = (float)Math.Sqrt(Math.Pow(playerPos.X, 2) + Math.Pow(playerPos.Y, 2));

                if (closestPlayerDistance >= distance)
                {
                    closestPlayerDistance = distance;
                    closestPlayer = player;
                }
            }

            if (closestPlayer != null)
            {
                switch (action)
                {
                    case VitaruAction.Spell:
                        ufoHealth.AttachedPlayer.ufoList.Remove(ufoHealth);
                        ufoHealth.AttachedPlayer.ufoContainer.Remove(ufoHealth);
                        closestPlayer.ufoList.Add(ufoHealth);
                        closestPlayer.ufoContainer.Add(ufoHealth);
                        ufoHealth.AttachedPlayer = closestPlayer;
                        break;
                    case VitaruAction.Spell2:
                        ufoEnergy.AttachedPlayer.ufoList.Remove(ufoEnergy);
                        ufoEnergy.AttachedPlayer.ufoContainer.Remove(ufoEnergy);
                        closestPlayer.ufoList.Add(ufoEnergy);
                        closestPlayer.ufoContainer.Add(ufoEnergy);
                        ufoEnergy.AttachedPlayer = closestPlayer;
                        break;
                    case VitaruAction.Spell3:
                        ufoDamage.AttachedPlayer.ufoList.Remove(ufoDamage);
                        ufoDamage.AttachedPlayer.ufoContainer.Remove(ufoDamage);
                        closestPlayer.ufoList.Add(ufoDamage);
                        closestPlayer.ufoContainer.Add(ufoDamage);
                        ufoDamage.AttachedPlayer = closestPlayer;
                        break;
                    case VitaruAction.Spell4:
                        ufoMark.AttachedPlayer.ufoList.Remove(ufoMark);
                        ufoMark.AttachedPlayer.ufoContainer.Remove(ufoMark);
                        closestPlayer.ufoList.Add(ufoMark);
                        closestPlayer.ufoContainer.Add(ufoMark);
                        ufoMark.AttachedPlayer = closestPlayer;
                        break;
                }
            }
            else
            {
                switch (action)
                {
                    case VitaruAction.Spell:
                        ufoHealth.AttachedPlayer.ufoList.Remove(ufoHealth);
                        ufoHealth.AttachedPlayer.ufoContainer.Remove(ufoHealth);
                        ufoList.Add(ufoHealth);
                        ufoContainer.Add(ufoHealth);
                        ufoHealth.AttachedPlayer = this;
                        break;
                    case VitaruAction.Spell2:
                        ufoEnergy.AttachedPlayer.ufoList.Remove(ufoEnergy);
                        ufoEnergy.AttachedPlayer.ufoContainer.Remove(ufoEnergy);
                        ufoList.Add(ufoEnergy);
                        ufoContainer.Add(ufoEnergy);
                        ufoEnergy.AttachedPlayer = this;
                        break;
                    case VitaruAction.Spell3:
                        ufoDamage.AttachedPlayer.ufoList.Remove(ufoDamage);
                        ufoDamage.AttachedPlayer.ufoContainer.Remove(ufoDamage);
                        ufoList.Add(ufoDamage);
                        ufoContainer.Add(ufoDamage);
                        ufoDamage.AttachedPlayer = this;
                        break;
                    case VitaruAction.Spell4:
                        ufoMark.AttachedPlayer.ufoList.Remove(ufoMark);
                        ufoMark.AttachedPlayer.ufoContainer.Remove(ufoMark);
                        ufoList.Add(ufoMark);
                        ufoContainer.Add(ufoMark);
                        ufoMark.AttachedPlayer = this;
                        break;
                }
            }
        }

        private void spellUpdate()
        {
            if (currentCharacter != Characters.NueHoujuu)
            {
                ufoMark = null;
                ufoHealth = null;
                ufoEnergy = null;
                ufoDamage = null;

                foreach (UFO ufo in ufoList)
                {
                    switch (ufo.UFOType)
                    {
                        case UFOType.Mark:
                            ufoMark = ufo;
                            break;
                        case UFOType.Health:
                            ufoHealth = ufo;
                            break;
                        case UFOType.Energy:
                            ufoEnergy = ufo;
                            break;
                        case UFOType.Damage:
                            ufoDamage = ufo;
                            break;
                    }
                }
            }

            if (ufoHealth != null && ufoHealth.ParentNue.Energy >= (float)Clock.ElapsedFrameTime / 1000)
            {
                MaxHealth = originalMaxHealth + 10;
                ufoHealth.ParentNue.Energy -= (float)Clock.ElapsedFrameTime / 1000;
                Heal((float)Clock.ElapsedFrameTime / 1000);
            }
            else
            {
                MaxHealth = originalMaxHealth;
                if (Health > MaxHealth)
                    Health = MaxHealth;
            }

            if (ufoEnergy != null && ufoEnergy.ParentNue.Energy >= (float)Clock.ElapsedFrameTime / 500 && maxEnergy - Energy >= (float)Clock.ElapsedFrameTime / 250)
            {
                Energy = Math.Min((float)Clock.ElapsedFrameTime / 250 + Energy, maxEnergy);
                ufoEnergy.ParentNue.Energy -= (float)Clock.ElapsedFrameTime / 500;
            }
            else if (CanHeal)
                Energy = Math.Min((float)Clock.ElapsedFrameTime / 500 * energyGainMultiplier + Energy, maxEnergy);

            CharacterSign.Alpha = Energy / (maxEnergy * 2);

            if (ghastlytActive)
                Energy -= (float)Clock.ElapsedFrameTime / 1000 * energyRequiredPerSecond;

            if (riftActive)
                Energy -= (float)Clock.ElapsedFrameTime / 1000 * energyRequiredPerSecond;

            if (Energy <= 0)
            {
                Energy = 0;
                ghastlytActive = false;
                timeFreezeActive = false;
                riftActive = false;
            }

            CharacterSign.Alpha = Energy / (maxEnergy * 2);

            foreach (Drawable child in Parent.Children)
                if (child is Rift rift)
                {
                    Vector2 riftPos = rift.ToSpaceOfOtherDrawable(Vector2.Zero, Hitbox);
                    float distance = (float)Math.Sqrt(Math.Pow(riftPos.X + 20, 2) + Math.Pow(riftPos.Y + 20, 2));

                    if (distance <= 32 && warpTime <= Time.Current && rift.Alpha > 0)
                    {
                        warpTime = Time.Current + beatLength;
                        Position = rift.LinkedRift.ToSpaceOfOtherDrawable(Vector2.Zero, Parent);
                    }
                }

            if (timeFreezeEndTime >= Time.Current)
            {
                if (!timeFreezeActive)
                {
                    currentRate += (float)Clock.ElapsedFrameTime / 100;
                    if (currentRate > originalRate)
                        currentRate = originalRate;
                    applyToClock(workingBeatmap.Value.Track, currentRate);
                    if (currentRate > 0 && timeFreezeEndTime - 500 <= Time.Current)
                    {
                        currentRate = originalRate;
                        applyToClock(workingBeatmap.Value.Track, currentRate);
                    }
                    else if (currentRate < 0 && timeFreezeEndTime + 500 >= Time.Current)
                    {
                            currentRate = originalRate;
                            applyToClock(workingBeatmap.Value.Track, currentRate);
                    }
                }
                else
                {
                    float energyDrainMultiplier = 0;
                    if (currentRate < 1)
                        energyDrainMultiplier = 1 - currentRate;
                    else if (currentRate >= 1)
                        energyDrainMultiplier = currentRate - 1;

                    Energy -= (float)Clock.ElapsedFrameTime / 1000 * (1 / currentRate) * energyRequiredPerSecond * energyDrainMultiplier;

                    if (currentRate > 0)
                        timeFreezeEndTime = Time.Current + 2000;
                    else
                        timeFreezeEndTime = Time.Current - 2000;

                    currentRate = originalRate * SetRate;
                    applyToClock(workingBeatmap.Value.Track, currentRate);
                }
            }

            if (leader && Health > 0)
            {
                foreach(VitaruPlayer player in playerList)
                {
                    Vector2 otherPlayerPos = player.ToSpaceOfOtherDrawable(Vector2.Zero, this) + new Vector2(6);
                    float distance = (float)Math.Sqrt(Math.Pow(otherPlayerPos.X, 2) + Math.Pow(otherPlayerPos.Y, 2));

                    if (player.Hitbox.Team == Hitbox.Team && distance <= 128)
                    {
                        player.Heal(2 * (float)Clock.ElapsedFrameTime);
                    }
                }
            }

            if (tabooActive)
            {
                if (cloneList.Count == 0)
                    tabooActive = false;
            }

            if (Time.Current >= reFreezeTime)
            {
                reFreezeTime = double.MaxValue;

                foreach (Crystal crystal in crystalList)
                    crystal.ReCollect(1000);

                CharacterKiai.Delay(900)
                             .FadeIn(100);
                CharacterSprite.Delay(900)
                               .FadeIn(100);
            }

            if (Time.Current >= reFrozenTime)
            {
                reFrozenTime = double.MaxValue;
                Dead = false;
                shattered = false;
                Hitbox.HitDetection = true;
            }

            if (!riftActive && riftStart != null && riftStart.Alpha == 1)
            {
                riftEnd.FadeOut(beatLength / 4);
                riftStart.FadeOut(beatLength / 4);
            }

            if (!ghastlytActive && Alpha == 0.5f)
            {
                Hitbox.HitDetection = true;
                foreach (VitaruPlayer clone in cloneList)
                {
                    clone.FadeOut(beatLength / 2)
                         .Delay(beatLength / 2)
                         .Expire();
                    cloneList.Remove(clone);
                    break;
                }

                this.FadeIn(beatLength / 2);
            }

            if (ufoList.Count > 0)
                ufoContainer.RotateTo((float)(Clock.CurrentTime / -1000 * 90));

            //just for debugging
            Energystored = Energy;
        }

        public override float Damage(float damage)
        {
            if (currentCharacter == Characters.Cirno)
            {
                Health -= damage;

                if (Health <= 0 && energyRequired <= Energy)
                {
                    Energy -= energyRequired;
                    shattered = true;
                    reFreezeTime = Time.Current + beatLength;
                    reFrozenTime = Time.Current + beatLength * 2;
                    Hitbox.HitDetection = false;

                    foreach (Crystal crystal in crystalList)
                        crystal.Pop(1000);
                    CharacterKiai.FadeOut(100);
                    CharacterSprite.FadeOut(100);

                    return Health = MaxHealth;
                }
                return Health;
            }
            return base.Damage(damage);
        }

        private void applyToClock(IAdjustableClock clock, float speed)
        {
            if (clock is IHasPitchAdjust pitchAdjust)
                pitchAdjust.PitchAdjust = speed;
            SpeedMultiplier = 1 / speed;
            foreach (Drawable draw in Parent)
            {
                VitaruPlayer player = draw as VitaruPlayer;
                if (player?.Team == Team && player != this)
                    player.SpeedMultiplier = SpeedMultiplier / 2 + 0.5f;
            }
        }
        #endregion

        #region Shooting Stuff
        private void bulletAddRad(float speed, float angle, Color4 color)
        {
            DrawableBullet drawableBullet;

            if (Invert)
                angle += (float)Math.PI;

            Parent.Add(drawableBullet = new DrawableBullet(Parent,
            new Bullet
            {
                StartTime = Time.Current,
                Cs = 1.2f,
                DummyMode = true,
                ComboColour = color,
                BulletAngleRadian = angle,
                BulletSpeed = speed,
                BulletDiameter = 16,
                BulletDamage = 20 * damageMultiplier,
                Team = Team,
                Ghost = currentCharacter == Characters.YuyukoSaigyouji | currentCharacter == Characters.AliceMuyart
            }));
            if (vampuric)
                drawableBullet.OnHit = () => Heal(0.5f);
            drawableBullet.MoveTo(Position);
        }

        private void patternWave()
        {
            const int numberbullets = 3;
            float directionModifier = -0.1F;
            Color4 color = CharacterColor;
            for (int i = 1; i <= numberbullets; i++)
            {
                if (currentCharacter == Characters.NueHoujuu)
                {
                    if (i == 1)
                        color = Color4.Red;
                    else if (i == 2)
                        color = Color4.Black;
                    else
                        color = Color4.Blue;
                }
                //-90 = up
                bulletAddRad(1, MathHelper.DegreesToRadians(-90) + directionModifier, color);
                directionModifier += 0.1f;
            }
        }

        private void patternCircle()
        {
            int numberbullets = 8;
            float directionModifier = (360f / numberbullets);
            float direction = MathHelper.DegreesToRadians(-90);
            directionModifier = MathHelper.DegreesToRadians(directionModifier);
            for (int i = 1; i <= numberbullets; i++)
            {
                bulletAddRad(1, direction, CharacterColor);
                direction += directionModifier;
            }
        }
        #endregion

        public override void Death()
        {
            if (Bot && Clone)
            {
                parentPlayer.cloneList.Remove(this);
                Expire();
            }
            else if (cloneList.Count > 0)
            {
                foreach(VitaruPlayer player in cloneList)
                {
                    player.Bot = Bot;
                    player.Auto = Auto;
                    player.Clone = Clone;
                    player.Invert = Invert;
                    cloneList.Remove(player);
                    player.Actions = Actions;
                    player.cloneList = cloneList;

                    foreach (VitaruPlayer clone in player.cloneList)
                        clone.parentPlayer = player;

                    if (!Bot)
                        VitaruPlayfield.VitaruPlayer = player;

                    Expire();
                    break;
                }
            }
        }

        #region Player Input Stuff
        /// <summary>
        /// Moves the player based on player input
        /// </summary>
        private void playerInput()
        {
            //Handles Player Speed
            float yTranslationDistance = player_speed * (float)Clock.ElapsedFrameTime * SpeedMultiplier;
            float xTranslationDistance = player_speed * (float)Clock.ElapsedFrameTime * SpeedMultiplier;
            Vector2 playerPosition = Position;

            if (Auto)
            {
                Actions[VitaruAction.Up] = false;
                Actions[VitaruAction.Down] = false;
                Actions[VitaruAction.Left] = false;
                Actions[VitaruAction.Right] = false;
                Actions[VitaruAction.Slow] = false;
                Actions[VitaruAction.Fast] = false;
                Actions[VitaruAction.Shoot] = false;
                VisibleHitbox.Alpha = 0;

                bool bulletClose = false;
                DrawableBullet closestBullet = null;
                float closestBulletEdgeDitance = float.MaxValue;
                float closestBulletAngle = 0;

                VitaruPlayer closestPlayerLatterally = null;
                float closestPlayerLatteralDistance = float.MaxValue;


                //bool bulletBehind = false;
                float behindBulletEdgeDitance = float.MaxValue;
                float behindBulletAngle = 0;

                foreach (Drawable draw in Parent)
                    if (draw is DrawableBullet)
                    {
                        DrawableBullet bullet = draw as DrawableBullet;
                        if (bullet.Bullet.Team != Team)
                        {
                            Vector2 pos = bullet.ToSpaceOfOtherDrawable(Vector2.Zero, this) + new Vector2(6);
                            float distance = (float)Math.Sqrt(Math.Pow(pos.X, 2) + Math.Pow(pos.Y, 2));
                            float edgeDistance = distance - (bullet.Width / 2 + Hitbox.Width / 2);
                            float angleToBullet = MathHelper.RadiansToDegrees((float)Math.Atan2((bullet.Position.Y - Position.Y), (bullet.Position.X - Position.X))) + 90 + Rotation;

                            if (closestBulletAngle < 360 - field_of_view | closestBulletAngle < -field_of_view && closestBulletAngle > field_of_view | closestBulletAngle > 360 + field_of_view)
                                if (closestBullet.Position.X > Position.X && bullet.Position.X < Position.X || closestBullet.Position.X < Position.X && bullet.Position.X > Position.X)
                                {
                                    //bulletBehind = true;
                                    behindBulletEdgeDitance = edgeDistance;
                                    behindBulletAngle = angleToBullet;
                                }

                            if (edgeDistance < closestBulletEdgeDitance)
                            {
                                closestBulletEdgeDitance = edgeDistance;
                                closestBullet = bullet;
                                closestBulletAngle = angleToBullet;
                            }
                        }
                    }
                    //Lets go after enemy players if possible
                    else if (draw is VitaruPlayer)
                    {
                        VitaruPlayer player = draw as VitaruPlayer;
                        if (player.Team != Team)
                        {
                            float latteralDistance = Position.X - player.Position.X;

                            if (latteralDistance < 0)
                                latteralDistance *= -1;

                            if (latteralDistance < closestPlayerLatteralDistance)
                            {
                                closestPlayerLatterally = player;
                                closestPlayerLatteralDistance = latteralDistance;
                            }
                        }
                    }

                if (closestBulletEdgeDitance <= 50)
                {
                    bulletClose = true;
                    if (closestBulletEdgeDitance <= 30)
                    {
                        if (!Invert)
                            Actions[VitaruAction.Down] = true;
                        else
                            Actions[VitaruAction.Up] = true;

                        Actions[VitaruAction.Slow] = true;
                    }

                    if (closestBulletAngle > 360 - field_of_view | closestBulletAngle > -field_of_view && closestBulletAngle < field_of_view | closestBulletAngle < 360 + field_of_view)
                    {
                        if (closestBullet.X < Position.X)
                            Actions[VitaruAction.Right] = true;
                        else
                            Actions[VitaruAction.Left] = true;
                    }
                }
                else if (!bulletClose)
                {
                    if (Position.X > 512 - 100)
                        Actions[VitaruAction.Left] = true;
                    else if (Position.X < 100)
                        Actions[VitaruAction.Right] = true;
                    else if (closestPlayerLatterally != null)
                    {
                        if (Position.X > closestPlayerLatterally.Position.X)
                            Actions[VitaruAction.Left] = true;
                        else
                            Actions[VitaruAction.Right] = true;
                    }

                    Actions[VitaruAction.Slow] = true;

                    if (Position.Y < 400 && !Invert || Position.Y < 300 && Invert)
                        Actions[VitaruAction.Down] = true;
                    else if (Position.Y > 500 && !Invert || Position.Y > 400 && Invert)
                        Actions[VitaruAction.Up] = true;
                }

                Actions[VitaruAction.Shoot] = true;

                if (Actions[VitaruAction.Slow])
                {
                    xTranslationDistance /= 2;
                    yTranslationDistance /= 2;
                    VisibleHitbox.Alpha = 1;
                }
                if (Actions[VitaruAction.Fast])
                {
                    xTranslationDistance *= 2;
                    yTranslationDistance *= 2;
                }

                if (Actions[VitaruAction.Up])
                    playerPosition.Y -= yTranslationDistance;
                if (Actions[VitaruAction.Left])
                    playerPosition.X -= xTranslationDistance;
                if (Actions[VitaruAction.Down])
                    playerPosition.Y += yTranslationDistance;
                if (Actions[VitaruAction.Right])
                    playerPosition.X += xTranslationDistance;

                playerPosition = Vector2.ComponentMin(playerPosition, playerBounds.Yw);
                playerPosition = Vector2.ComponentMax(playerPosition, playerBounds.Xz);
            }
            else
            {
                if (Actions[VitaruAction.Slow])
                {
                    xTranslationDistance /= 2;
                    yTranslationDistance /= 2;
                }
                if (Actions[VitaruAction.Fast])
                {
                    xTranslationDistance *= 2;
                    yTranslationDistance *= 2;
                }

                if (Actions[VitaruAction.Up])
                    playerPosition.Y -= yTranslationDistance;
                if (Actions[VitaruAction.Left])
                    playerPosition.X -= xTranslationDistance;
                if (Actions[VitaruAction.Down])
                    playerPosition.Y += yTranslationDistance;
                if (Actions[VitaruAction.Right])
                    playerPosition.X += xTranslationDistance;

                playerPosition = Vector2.ComponentMin(playerPosition, playerBounds.Yw);
                playerPosition = Vector2.ComponentMax(playerPosition, playerBounds.Xz);
            }
            Position = playerPosition;
        }

        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => true;

        public bool OnPressed(VitaruAction action)
        {
            if (!Bot && !Puppet)
            {
                //Keyboard Stuff
                if (currentCharacter == Characters.AliceMuyart)
                {
                    if (action == VitaruAction.Increase)
                        SetRate = Math.Min((float)Math.Round(SetRate + 0.1f, 1), 1.5f);
                    if (action == VitaruAction.Decrease)
                        SetRate = Math.Max((float)Math.Round(SetRate - 0.1f, 1), -1f);
                }
                else if (currentCharacter == Characters.SakuyaIzayoi)
                {
                    if (action == VitaruAction.Increase)
                        SetRate = Math.Min((float)Math.Round(SetRate + 0.2f, 1), 0.8f);
                    if (action == VitaruAction.Decrease)
                        SetRate = Math.Max((float)Math.Round(SetRate - 0.2f, 1), 0.2f);
                }

                if (action == VitaruAction.Up)
                    Actions[VitaruAction.Up] = true;
                if (action == VitaruAction.Down)
                    Actions[VitaruAction.Down] = true;
                if (action == VitaruAction.Left)
                    Actions[VitaruAction.Left] = true;
                if (action == VitaruAction.Right)
                    Actions[VitaruAction.Right] = true;
                if (action == VitaruAction.Fast && currentCharacter != Characters.IbarakiKasen)
                    Actions[VitaruAction.Fast] = true;
                if (action == VitaruAction.Slow)
                {
                    if (currentSkin != GraphicsPresets.StandardCompetitive && currentSkin != GraphicsPresets.HighPerformanceCompetitive)
                        VisibleHitbox.Alpha = 1;

                    Actions[VitaruAction.Slow] = true;
                }
                if (action == VitaruAction.LeftShoot | action == VitaruAction.RightShoot | action == VitaruAction.Shoot | action == VitaruAction.Spell && currentCharacter == Characters.KokoroHatano)
                {
                    kokoroSpell();

                    if (Time.Current <= lastQuarterBeat + hitwindow | Time.Current >= nextHalfBeat - hitwindow)
                        patternWave();
                }

                //Mouse Stuff
                if (action == VitaruAction.Shoot && currentCharacter != Characters.KokoroHatano)
                    Actions[VitaruAction.Shoot] = true;

                spell(false, action);
                sendPacket();

                return true;
            }
            return false;
        }

        public bool OnReleased(VitaruAction action)
        {
            if (!Bot && !Puppet)
            {
                //Keyboard Stuff
                if (action == VitaruAction.Up)
                    Actions[VitaruAction.Up] = false;
                if (action == VitaruAction.Down)
                    Actions[VitaruAction.Down] = false;
                if (action == VitaruAction.Left)
                    Actions[VitaruAction.Left] = false;
                if (action == VitaruAction.Right)
                    Actions[VitaruAction.Right] = false;
                if (action == VitaruAction.Fast)
                    Actions[VitaruAction.Fast] = false;
                if (action == VitaruAction.Slow)
                {
                    if (currentSkin != GraphicsPresets.StandardCompetitive && currentSkin != GraphicsPresets.HighPerformanceCompetitive)
                        VisibleHitbox.Alpha = 0;

                    Actions[VitaruAction.Slow] = false;
                }

                //Mouse Stuff
                if (action == VitaruAction.Shoot)
                    Actions[VitaruAction.Shoot] = false;
                spell(true, action);
                sendPacket();

                return true;
            }
            return false;
        }
        #endregion

        #region Networking
        private void sendPacket()
        {
            if (VitaruNetworkingClientHandler != null && !Puppet)
            {
                VitaruPlayerInformation playerInformation = new VitaruPlayerInformation
                {
                    Character = currentCharacter,
                    PlayerX = Position.X,
                    PlayerY = Position.Y,
                    PlayerID = PlayerID,
                    Actions = Actions,
                    ClockSpeed = currentRate
                };

                ClientInfo clientInfo = new ClientInfo
                {
                    IP = VitaruNetworkingClientHandler.ClientInfo.IP,
                    Port = VitaruNetworkingClientHandler.ClientInfo.Port
                };

                VitaruInMatchPacket packet = new VitaruInMatchPacket(clientInfo) { PlayerInformation = playerInformation };

                VitaruNetworkingClientHandler.SendToHost(packet);
                VitaruNetworkingClientHandler.SendToInGameClients(packet);
            }
        }

        private void packetReceived(Packet p)
        {
            if (p is VitaruInMatchPacket packet)
            {
                if (packet.PlayerInformation.Character == Characters.SakuyaIzayoi | packet.PlayerInformation.Character == Characters.AliceMuyart)
                    applyToClock(workingBeatmap.Value.Track, packet.PlayerInformation.ClockSpeed);

                if (packet.PlayerInformation.PlayerID == PlayerID && Puppet)
                {
                    Actions = packet.PlayerInformation.Actions;
                    Position = new Vector2(packet.PlayerInformation.PlayerX, packet.PlayerInformation.PlayerY);
                }

                VitaruNetworkingClientHandler.ShareWithOtherPeers(packet);
            }
        }
        #endregion

        #region Touhosu Story
        private double startSpeaking = double.MaxValue;
        private double lengthOfSpeaking;

        private readonly Bindable<bool> familiar = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.Familiar);
        private int familiarity;

        private readonly Bindable<bool> lastDance = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.LastDance);
        private int dance;

        private readonly Bindable<bool> insane = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.Insane);
        private int insanity;

        private readonly Bindable<bool> awoken = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.Awoken);
        private int awakening;

        private readonly Bindable<bool> sacred = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.Sacred);
        private int tresspassing;

        private readonly Bindable<bool> resurrected = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.Resurrected);
        private int resurrection;

        public void Speak(string text)
        {
            textContainer.FadeTo(0.5f, 200);
            textContainer.Text = text;
            startSpeaking = Time.Current;
            lengthOfSpeaking = 0;

            int y = 150;
            foreach (char i in text)
            {
                lengthOfSpeaking += y;
                y++;
            }
        }

        private void speakingUpdate()
        {
            if (Time.Current > startSpeaking + lengthOfSpeaking)
                textContainer.FadeOut(200);

            if (workingBeatmap.Value.BeatmapInfo.OnlineBeatmapID == 1371893 && currentCharacter == Characters.ReimuHakurei && !familiar)
            {
                if (Time.Current >= 5200 && familiarity == 0)
                {
                    Speak("This place. . .");
                    familiarity++;
                }
                if (Time.Current >= 59100 && familiarity == 1)
                {
                    Speak("It seems familiar. . .");
                    familiarity++;
                }
                if (Time.Current >= 93920 && familiarity == 2)
                {
                    Speak("Yes, this is where I got into my first fight!");
                    familiarity++;
                }
                if (Time.Current >= 149572 && familiarity == 3)
                {
                    Speak("Fairies were mad for seemingly no reason,");
                    familiarity++;
                }
                if (Time.Current >= 177398 && familiarity == 4)
                {
                    Speak("I had found out later Marisa had trespassed without even knowing,");
                    familiarity++;
                }
                if (Time.Current >= 205224 && familiarity == 5)
                {
                    Speak("Thankfully she came to help, we fought hard,");
                    familiarity++;
                }
                if (Time.Current >= 233050 && familiarity == 6)
                {
                    Speak("Then the Scarlet sisters came,");
                    familiarity++;
                }
                if (Time.Current >= 246963 && familiarity == 7)
                {
                    Speak("I tried to resolve this, but too much blood had been shed,");
                    familiarity++;
                }
                if (Time.Current >= 274789 && familiarity == 8)
                {
                    Speak("We fled, planning to meet at their mansion later that week,");
                    familiarity++;
                }
                if (Time.Current >= 302615 && familiarity == 9)
                {
                    Speak("That was a mistake.");
                    familiarity++;
                    familiar.Value = true;
                }
            }

            if (false)//workingBeatmap.Value.BeatmapInfo.OnlineBeatmapID == 1548917 && currentCharacter == Characters.KokoroHatano && !lastDance)
            {
                if (Time.Current >= 1430 && dance == 0)
                {
                    Speak("This is it,");
                    dance++;
                }
                if (Time.Current >= 23760 && dance == 1)
                {
                    Speak("My final act,");
                    dance++;
                }
                if (Time.Current >= 43300 && dance == 2)
                {
                    Speak("My Last Dance.");
                    dance++;
                    lastDance.Value = true;
                }
            }

            if (false)//workingBeatmap.Value.BeatmapInfo.OnlineBeatmapID == 114716 && currentCharacter == Characters.FlandreScarlet && insane)
            {
                if (Time.Current >= 760 && insanity == 0)
                {
                    Speak("That piano. . .");
                    insanity++;
                }
                if (Time.Current >= 12340 && insanity == 1)
                {
                    Speak("It is driving me insane!");
                    insanity++;
                }
                if (Time.Current >= 28600 && insanity == 2)
                {
                    Speak("Missy please, I am trying to think.");
                    insanity++;
                    insane.Value = false;
                }
            }

            if (false)//workingBeatmap.Value.BeatmapInfo.OnlineBeatmapID == 114716 && currentCharacter == Characters.FlandreScarlet && !insane)
            {
                if (Time.Current >= 760 && insanity == 0)
                {
                    Speak("That piano really needs to stop. . .");
                    insanity++;
                }
            }
            /*
            if (workingBeatmap.Value.BeatmapInfo.OnlineBeatmapID == 114716 && currentCharacter == Characters.RemiliaScarlet && !insane)
            {
                if (Time.Current >= 760 && awakening == 0)
                {
                    Speak("Flandre, what happened to you?");
                    awakening++;
                }
                if (Time.Current >= 12340 && awakening == 1)
                {
                    Speak("Flan, are you there?");
                    awakening++;
                }
                if (Time.Current >= 28600 && awakening == 2)
                {
                    Speak("Its me, your sister Remilia,");
                    awakening++;
                }
                if (Time.Current >= 0 && awakening == 3)
                {
                    Speak("Flan? I know you can hear me.");
                    awakening++;
                }
                if (Time.Current >= 0 && awakening == 4)
                {
                    Speak("Please? I need to talk.");
                    awakening++;
                }
                if (Time.Current >= 0 && awakening == 5)
                {
                    Speak("I know you're upset,");
                    awakening++;
                }
                if (Time.Current >= 0 && awakening == 6)
                {
                    Speak("But I need my sister back.");
                    awakening++;
                }
                if (Time.Current >= 0 && awakening == 7)
                {
                    Speak("What would Hong say if she knew you were this lazy?");
                    awakening++;
                }
                if (Time.Current >= 0 && awakening == 8)
                {
                    //Flandre.Speak("\"Get off your ass\"?");
                    awakening++;
                }
            }
            */

            if (false)//workingBeatmap.Value.BeatmapInfo.OnlineBeatmapID == 148000 && currentCharacter == Characters.Kaguya)
            {
                if (Time.Current >= 1280 && tresspassing == 0)
                {
                    Speak("What a lovely night it is for a walk.");
                    tresspassing++;
                }
                if (Time.Current >= 20860 && tresspassing == 1)
                {
                    Speak("Oh?");
                    tresspassing++;
                }
                if (Time.Current >= 22120 && tresspassing == 2)
                {
                    Speak("Someone has been here already. . .");
                    tresspassing++;
                }
                if (Time.Current >= 37280 && tresspassing == 3)
                {
                    Speak("Thats them over there.");
                    tresspassing++;
                }
                if (Time.Current >= 41060 && tresspassing == 4)
                {
                    Speak("Whaaa-");
                    tresspassing++;
                }
                if (Time.Current >= 82740 && tresspassing == 5)
                {
                    Speak("Why are we fighting? What did I do to you?");
                    tresspassing++;
                }
            }
        }
        #endregion
    }

    public enum Characters
    {
        //Alex,
        [Description("Reimu Hakurei")]
        ReimuHakurei = 1,
        [Description("Marisa Kirisame")]
        MarisaKirisame,
        [Description("Sakuya Izayoi")]
        SakuyaIzayoi,
        [Description("Hong Meiling")]
        HongMeiling,
        [Description("Flandre Scarlet")]
        FlandreScarlet,
        [Description("Remilia Scarlet")]
        RemiliaScarlet,
        [Description("Cirno")]
        Cirno,
        [Description("Tenshi Hinanai")]
        TenshiHinanai,
        [Description("Yuyuko Saigyouji")]
        YuyukoSaigyouji,
        [Description("Yukari Yakumo")]
        YukariYakumo,
        [Description("Ran Yakumo")]
        RanYakumo,
        [Description("Chen")]
        Chen,
        [Description("Alice Margatroid")]
        AliceMargatroid,
        [Description("Komachi Onozuka")]
        KomachiOnozuka,
        [Description("Byakuren Hijiri")]
        ByakurenHijiri,
        [Description("Rumia")]
        Rumia,
        [Description("Sikieiki Yamaxanadu")]
        SikieikiYamaxanadu,
        [Description("Suwako Moriya")]
        SuwakoMoriya,
        [Description("Youmu Konpaku")]
        YoumuKonpaku,
        [Description("Kokoro Hatano")]
        KokoroHatano,
        [Description("Kaguya")]
        Kaguya,
        [Description("Ibaraki Kasen")]
        IbarakiKasen,
        [Description("Nue Houjuu")]
        NueHoujuu,
        //[Description("Meme")]
        //Taikonator,
        [Description("Alice Muyart")]
        AliceMuyart,
        [Description("Arysa Muyart")]
        ArysaMuyart
    }
}
