namespace MedPal.API.Repositories
{
    public interface ITenantAwareRepository<T>
    {
        IQueryable<T> ApplyTenantFilter(IQueryable<T> query);
    }
}