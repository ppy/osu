using osu.Framework.Graphics;
using osu.Game.Rulesets.Vitaru.Objects.Drawables;
using OpenTK;
using osu.Game.Rulesets.Vitaru.Judgements;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Game.Rulesets.Vitaru.Objects.Characters;
using osu.Framework.Graphics.Cursor;
using osu.Game.Rulesets.Vitaru.UI.Cursor;
using osu.Framework.Configuration;
using System.Collections.Generic;
using Symcol.Rulesets.Core;
using osu.Game.Rulesets.Vitaru.Multi;
using osu.Framework.Logging;

namespace osu.Game.Rulesets.Vitaru.UI
{
    public class VitaruPlayfield : SymcolPlayfield
    {
        private readonly VitaruGamemode currentGameMode = VitaruSettings.VitaruConfigManager.GetBindable<VitaruGamemode>(VitaruSetting.GameMode);
        private readonly Characters currentCharacter = VitaruSettings.VitaruConfigManager.GetBindable<Characters>(VitaruSetting.Characters);
        private readonly bool multiplayer = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.ShittyMultiplayer);
        private bool friendlyPlayerOverride = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.FriendlyPlayerOverride);
        private readonly Bindable<int> friendlyPlayerCount = VitaruSettings.VitaruConfigManager.GetBindable<int>(VitaruSetting.FriendlyPlayerCount);
        private readonly Bindable<int> enemyPlayerCount = VitaruSettings.VitaruConfigManager.GetBindable<int>(VitaruSetting.EnemyPlayerCount);

        private readonly Characters playerOne = VitaruSettings.VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerOne);
        private readonly Characters playerTwo = VitaruSettings.VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerTwo);
        private readonly Characters playerThree = VitaruSettings.VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerThree);
        private readonly Characters playerFour = VitaruSettings.VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerFour);
        private readonly Characters playerFive = VitaruSettings.VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerFive);
        private readonly Characters playerSix = VitaruSettings.VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerSix);
        private readonly Characters playerSeven = VitaruSettings.VitaruConfigManager.GetBindable<Characters>(VitaruSetting.PlayerSeven);

        public static Container GamePlayfield;
        private readonly MirrorField mirrorPlayfield;
        private readonly Container judgementLayer;
        private readonly List<VitaruPlayer> playerList = new List<VitaruPlayer>();

        public static List<VitaruClientInfo> LoadPlayerList = new List<VitaruClientInfo>();

        public static VitaruPlayer VitaruPlayer;

        //public override bool ProvidingUserCursor => true;

        public virtual bool LoadPlayer => true;

        public static Vector2 BASE_SIZE = new Vector2(512, 820);

        private static Vector2 parentDrawSize = new Vector2(1280, 720);

        //TODO: Delete this and make it work
        public override Vector2 Size
        {
            get
            {
                var parentSize = parentDrawSize;

                if (Parent != null)
                {
                    parentDrawSize = Parent.DrawSize;
                    parentSize = Parent.DrawSize;
                }

                var aspectSize = parentSize.X * 0.75f < parentSize.Y ? new Vector2(parentSize.X, parentSize.X * 0.75f) : new Vector2(parentSize.Y * 5f / 8f, parentSize.Y);

                if (currentGameMode == VitaruGamemode.Dodge)
                {
                    aspectSize = parentSize.X * 0.75f < parentSize.Y ? new Vector2(parentSize.X, parentSize.X * 0.75f) : new Vector2(parentSize.Y * 4f / 3f, parentSize.Y);
                    BASE_SIZE = new Vector2(512, 384);
                }
                else
                    BASE_SIZE = new Vector2(512, 820);

                return new Vector2(aspectSize.X / parentSize.X, aspectSize.Y / parentSize.Y) * base.Size;
            }
        }

        public VitaruPlayfield() : base(BASE_SIZE)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            if (currentGameMode != VitaruGamemode.Dodge && multiplayer && enemyPlayerCount > 0)
            {
                Position = new Vector2(20, 0);
                Anchor = Anchor.Centre;
                Origin = Anchor.CentreLeft;
                Add(mirrorPlayfield = new MirrorField());
            }

            AddRange(new Drawable[]
            {
                GamePlayfield = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
                judgementLayer = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
            });

            if (LoadPlayer)
            {
                VitaruNetworkingClientHandler vitaruNetworkingClientHandler = RulesetNetworkingClientHandler as VitaruNetworkingClientHandler;

                if (vitaruNetworkingClientHandler != null)
                    playerList.Add(VitaruPlayer = new VitaruPlayer(GamePlayfield, currentCharacter) { VitaruNetworkingClientHandler = vitaruNetworkingClientHandler, PlayerID = vitaruNetworkingClientHandler.VitaruClientInfo.IP + vitaruNetworkingClientHandler.VitaruClientInfo.UserID });
                else
                    playerList.Add(VitaruPlayer = new VitaruPlayer(GamePlayfield, currentCharacter));

                foreach (VitaruClientInfo client in LoadPlayerList)
                    if (client.PlayerInformation.PlayerID != VitaruPlayer.PlayerID)
                    {
                        Logger.Log("Loading a player recieved from internet!", LoggingTarget.Network, LogLevel.Verbose);
                        playerList.Add(new VitaruPlayer(GamePlayfield, client.PlayerInformation.Character)
                        {
                            Puppet = true,
                            PlayerID = client.PlayerInformation.PlayerID,
                            VitaruNetworkingClientHandler = vitaruNetworkingClientHandler
                        });
                    }

                if (multiplayer && currentGameMode != VitaruGamemode.Dodge)
                {
                    switch (friendlyPlayerCount)
                    {
                        case 0:
                            break;
                        case 1:
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerOne) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512f / 2), Auto = true, Bot = true });
                            break;
                        case 2:
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerOne) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512 - 200, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerTwo) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(200, 700), Auto = true, Bot = true });
                            break;
                        case 3:
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerOne) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(0, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerTwo) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(200, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerThree) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512f / 2, 700), Auto = true, Bot = true });
                            break;
                        case 4:
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerOne) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(0, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerTwo) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(200, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerThree) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512 - 200, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerFour) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512, 700), Auto = true, Bot = true });
                            break;
                        case 5:
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerOne) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(0, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerTwo) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(200, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerThree) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512f / 2, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerFour) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512 - 200, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerFive) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512, 700), Auto = true, Bot = true });
                            break;
                        case 6:
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerOne) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(0, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerTwo) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(150, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerThree) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(250, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerFour) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512 - 250, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerFive) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512 - 150, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerSix) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512, 700), Auto = true, Bot = true });
                            break;
                        case 7:
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerOne) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(0, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerTwo) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(125, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerThree) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(200, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerFour) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512f / 2, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerFive) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512 - 200, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerSix) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512 - 125, 700), Auto = true, Bot = true });
                            playerList.Add(new VitaruPlayer(GamePlayfield, playerSeven) { Anchor = Anchor.Centre, Origin = Anchor.Centre, Position = new Vector2(512, 700), Auto = true, Bot = true });
                            break;
                    }
                }

                foreach (VitaruPlayer player in playerList)
                    GamePlayfield.Add(player);

                VitaruPlayer.Position = new Vector2(256, 700);
                if (currentGameMode == VitaruGamemode.Dodge)
                    VitaruPlayer.Position = BASE_SIZE / 2;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var cursor = CreateCursor();
            if (cursor != null)
                AddInternal(cursor);
        }

        public override void Add(DrawableHitObject h)
        {
            h.Depth = (float)h.HitObject.StartTime;

            h.OnJudgement += onJudgement;

            base.Add(h);
        }

        private void onJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            var vitaruJudgement = (VitaruJudgement)judgement;

            if (VitaruPlayer != null)
            {
                DrawableVitaruJudgement explosion = new DrawableVitaruJudgement(vitaruJudgement)
                {
                    Alpha = 0.5f,
                    Origin = Anchor.Centre,
                    Position = new Vector2(VitaruPlayer.Position.X, VitaruPlayer.Position.Y + 50)
                };

                judgementLayer.Add(explosion);
            }
        }

        protected virtual CursorContainer CreateCursor() => new GameplayCursor();
    }
}
