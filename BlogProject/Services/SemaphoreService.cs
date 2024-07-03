using BlogProject.Repository.Interfaces;
using System.Collections.Concurrent;

namespace BlogProject.Services;

public class SemaphoreService : ISemaphoreService
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> _articleSemaphores = new ConcurrentDictionary<int, SemaphoreSlim>();

    public SemaphoreSlim findSemaphore(int id)
    {
        return _articleSemaphores.GetOrAdd(id, new SemaphoreSlim(1, 1));
    }
}
