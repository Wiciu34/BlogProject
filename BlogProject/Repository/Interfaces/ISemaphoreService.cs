namespace BlogProject.Repository.Interfaces
{
    public interface ISemaphoreService
    {
        SemaphoreSlim findSemaphore(int id);
    }
}
