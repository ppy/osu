namespace osu.Game.Screens.Mvis.SideBar
{
    public interface ISidebarContent
    {
        public float ResizeWidth { get; }
        public string Title { get; }
        public float ResizeHeight => 1f;
    }
}
