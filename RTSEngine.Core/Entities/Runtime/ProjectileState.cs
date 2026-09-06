using RTSEngine.Core.Entities;

namespace RTSEngine.Core.Entities.Runtime;

public class ProjectileState
{
    private readonly List<Projectile> _projectiles = [];
    private int _nextProjectileId = 1;

    public IReadOnlyList<Projectile> Projectiles => _projectiles;

    public int NextId() => _nextProjectileId++;

    public void Add(Projectile projectile) => _projectiles.Add(projectile);

    public void RemoveAt(int index) => _projectiles.RemoveAt(index);

    public void Clear()
    {
        _projectiles.Clear();
        _nextProjectileId = 1;
    }
}
