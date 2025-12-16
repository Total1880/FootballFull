namespace FootballFull.Repositories.Interfaces
{
    public interface INameRepository
    {
        string GetRandomFirstName(Guid CountryId);
        string GetRandomLastName(Guid CountryId);
    }
}
