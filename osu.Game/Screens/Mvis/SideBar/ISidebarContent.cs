namespace osu.Game.Screens.Mvis.SideBar
{
    public interface ISidebarContent
    {
        float ResizeWidth { get; }
        string Title { get; }
        float ResizeHeight => 1f;
    }
}
