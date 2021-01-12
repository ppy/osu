using NUnit.Framework;
using osu.Game.Screens.Purcashe;

namespace osu.Game.Tests.Visual.Mvis
{
    public class TestScenePurcasheScreen : ScreenTestScene
    {
        [Test]
        public void CreateMvisScreen()
        {
            AddStep("Create screen", () =>
            {
                if (Stack.CurrentScreen != null)
                    Stack?.Exit();

                LoadScreen(new PurcasheScreen());
            });
        }
    }
}
