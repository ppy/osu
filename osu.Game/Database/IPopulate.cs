namespace osu.Game.Database
{
    public interface IPopulate
    {
        void Populate(OsuDbContext connection);
    }
}
