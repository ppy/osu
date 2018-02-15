namespace osu.Game.Database
{
    public interface ICanAcceptFiles
    {
        void Import(params string[] paths);

        string[] HandledExtensions { get; }
    }
}
