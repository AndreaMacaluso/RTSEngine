using RTSEngine.Core.Helpers;

namespace RTSEngine.Core.Entities;

public class Projectile
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public FixedPoint X { get; set; }
    public FixedPoint Y { get; set; }
    public FixedPoint TargetX { get; set; }
    public FixedPoint TargetY { get; set; }
    public int? TargetEntityId { get; set; }
    public int Damage { get; set; }
    public FixedPoint Speed { get; set; }
    public bool IsSingleTarget { get; set; } = true;
    public FixedPoint SplashRadius { get; set; }
    public bool IsActive { get; set; } = true;

    private const int ReachThresholdSq = 250; // 0.5² × 1000

    public int DistanceToTargetSquared =>
        FixedPoint.DistanceSquared(X, Y, TargetX, TargetY);

    public bool HasReachedTarget => DistanceToTargetSquared < ReachThresholdSq;

    public void MoveTowardsTarget()
    {
        if (HasReachedTarget) return;

        int dx = TargetX.Raw - X.Raw;
        int dy = TargetY.Raw - Y.Raw;
        int distSq = dx * dx + dy * dy;
        int speedRaw = Speed.Raw;

        if (distSq <= speedRaw * speedRaw)
        {
            X = TargetX;
            Y = TargetY;
        }
        else
        {
            int dist = FixedPoint.IntegerSqrt(distSq);
            if (dist > 0)
            {
                X = new FixedPoint(X.Raw + dx * speedRaw / dist);
                Y = new FixedPoint(Y.Raw + dy * speedRaw / dist);
            }
        }
    }
}
