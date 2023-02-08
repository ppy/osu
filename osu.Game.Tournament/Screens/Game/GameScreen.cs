using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Online.API;
using osu.Game.Tests.Visual;

namespace osu.Game.Tournament.Screens.Game
{
    public partial class GameScreen : TournamentScreen
    {
        [Resolved]
        private GameHost host { get; set; } = null!;

        private OsuGameTestScene.TestOsuGame? nestedGame;

        public override void Show()
        {
            this.FadeIn();

            nestedGame = new OsuGameTestScene.TestOsuGame(host.Storage, new DummyAPIAccess())
            {
                Masking = true
            };

            nestedGame.SetHost(host);

            AddInternal(nestedGame);
        }

        public override void Hide()
        {
            if (nestedGame != null) RemoveInternal(nestedGame, true);

            base.Hide();
        }
    }
}
