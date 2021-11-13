namespace osu.Game.Database
{
    public partial interface IModelDownloader<T>
    {
        bool AccelDownload(T item, bool minimiseDownloadSize);
    }
}
