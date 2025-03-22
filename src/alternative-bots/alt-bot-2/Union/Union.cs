using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class EnemyData
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Energy { get; set; }
    public int LastTurnScanned { get; set; }
    public double PrevX { get; set; }
    public double PrevY { get; set; }
}

public class Union : Bot
{
    private const double HighEnergyThreshold = 60;
    private const double MiddleEnergyThreshold = 30;

    private readonly Dictionary<int, (double X, double Y)> enemyLocations = new Dictionary<int, (double, double)>();
    private const double minDistanceFromWall = 50;
    private const double dangerZoneMargin = 50; 
    private const double enemyDangerRadius = 200;
    private readonly Random random = new Random();

    private readonly Dictionary<int, double> enemyEnergy = new Dictionary<int, double>();
    private readonly Dictionary<string, EnemyData> enemyTracker = new Dictionary<string, EnemyData>();
    private string currentTargetId = null;
    private bool inCorner = false;
    private const double CornerThreshold = 50;

    static void Main(string[] args)
    {
        new Union().Start();
    }

    Union() : base(BotInfo.FromFile("Union.json")) { }

    public override void Run()
    {
        BodyColor = Color.Green;
        TurretColor = Color.Black;
        RadarColor = Color.White;
        BulletColor = Color.Red;

        if (Energy >= HighEnergyThreshold && !inCorner)
        {
            MoveToNearestCorner();
            inCorner = true;
        }

        while (IsRunning)
        {
            TurnRadarLeft(Double.PositiveInfinity);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (Energy >= HighEnergyThreshold)
        {
            ManiacBehavior(e);
        }
        else if (Energy >= MiddleEnergyThreshold)
        {
            EitsBehavior(e);
        }
        else
        {
            LawrieBehaviour(e);
        }
    }
    private void LawrieBehaviour(ScannedBotEvent e)
    {
        enemyLocations[e.ScannedBotId] = (e.X, e.Y);

        double angleToEnemy = NormalizeAbsoluteAngle(Direction + BearingTo(e.X, e.Y));
        double distance = DistanceTo(e.X, e.Y);

        double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);
        double extraTurn = Math.Min(Math.Atan(36.0 / distance), MaxTurnRate);
        radarTurn += (radarTurn < 0 ? -extraTurn : extraTurn);
        SetTurnRadarLeft(radarTurn);

        double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);
        SetTurnGunLeft(gunTurn);
        Fire(1);

        MoveToSafeLocation();
    }

    private void MoveToSafeLocation()
    {
        double targetX, targetY;

        var potentialSpots = new List<(double X, double Y)>
        {
            (ArenaWidth - minDistanceFromWall, ArenaHeight - minDistanceFromWall),
            (minDistanceFromWall, ArenaHeight - minDistanceFromWall),             
            (ArenaWidth - minDistanceFromWall, minDistanceFromWall),               
            (minDistanceFromWall, minDistanceFromWall)                            
        };

        var validSpots = potentialSpots
            .Where(spot => !IsInCenterDangerZone(spot.X, spot.Y)
                           && !IsNearEnemy(spot.X, spot.Y))
            .ToList();

        if (validSpots.Any())
        {
            var safestSpot = validSpots.OrderByDescending(spot =>
                enemyLocations.Values.DefaultIfEmpty((0.0, 0.0))
                .Min(enemy => Math.Sqrt(Math.Pow(enemy.Item1 - spot.X, 2) + Math.Pow(enemy.Item2 - spot.Y, 2)))
            ).First();
            targetX = safestSpot.X;
            targetY = safestSpot.Y;
        }
        else
        {
            var alternativeSpot = potentialSpots.OrderByDescending(spot =>
                Math.Sqrt(Math.Pow((ArenaWidth / 2) - spot.X, 2) + Math.Pow((ArenaHeight / 2) - spot.Y, 2))
            ).First();
            targetX = alternativeSpot.X;
            targetY = alternativeSpot.Y;
        }

        targetX = Math.Max(minDistanceFromWall, Math.Min(ArenaWidth - minDistanceFromWall, targetX));
        targetY = Math.Max(minDistanceFromWall, Math.Min(ArenaHeight - minDistanceFromWall, targetY));

        GoToPosition(targetX, targetY);
    }

    private bool IsInCenterDangerZone(double x, double y)
    {
        double centerX = ArenaWidth / 2;
        double centerY = ArenaHeight / 2;
        return (Math.Abs(x - centerX) < dangerZoneMargin) && (Math.Abs(y - centerY) < dangerZoneMargin);
    }

    private bool IsNearEnemy(double x, double y)
    {
        foreach (var enemy in enemyLocations.Values)
        {
            double distance = Math.Sqrt(Math.Pow(enemy.X - x, 2) + Math.Pow(enemy.Y - y, 2));
            if (distance < enemyDangerRadius)
                return true;
        }
        return false;
    }

    private void GoToPosition(double targetX, double targetY)
    {
        double bearing = BearingTo(targetX, targetY);
        double angleToTarget = NormalizeRelativeAngle(bearing + Direction);
        double distance = DistanceTo(targetX, targetY);

        if (angleToTarget >= 0)
            SetTurnLeft(angleToTarget);
        else
            SetTurnRight(-angleToTarget);

        SetForward(distance);
    }
    private void ManiacBehavior(ScannedBotEvent e)
    {
        int currentTurn = TurnNumber;
        var keysToRemove = enemyTracker
            .Where(kvp => (currentTurn - kvp.Value.LastTurnScanned) > 1)
            .Select(kvp => kvp.Key)
            .ToList();
        foreach (string key in keysToRemove)
        {
            enemyTracker.Remove(key);
            if (currentTargetId == key)
                currentTargetId = null;
        }

        string enemyIdStr = e.ScannedBotId.ToString();
        bool lockedOn = false;
        double previousBearing = 0;

        if (enemyTracker.ContainsKey(enemyIdStr))
        {
            EnemyData data = enemyTracker[enemyIdStr];
            previousBearing = Math.Atan2(data.X - Y, data.Y - X) * 180 / Math.PI;
            data.PrevX = data.X;
            data.PrevY = data.Y;
        }
        else
        {
            enemyTracker[enemyIdStr] = new EnemyData();
        }

        EnemyData enemy = enemyTracker[enemyIdStr];
        double newBearing = Math.Atan2(e.Y - Y, e.X - X) * 180 / Math.PI;
        enemy.X = e.X;
        enemy.Y = e.Y;
        enemy.Energy = e.Energy;
        enemy.LastTurnScanned = currentTurn;

        if (enemy.PrevX != 0 || enemy.PrevY != 0)
        {
            double bearingDifference = Math.Abs(NormalizeRelativeAngle(newBearing - previousBearing));
            if (bearingDifference < 5)
            {
                lockedOn = true;
            }
        }

        if (currentTargetId == null || !enemyTracker.ContainsKey(currentTargetId))
        {
            var cornerEnemies = enemyTracker.Where(kvp => IsEnemyInCorner(kvp.Value));
            if (cornerEnemies.Any())
            {
                var targetEntry = cornerEnemies.OrderBy(kvp => DistanceTo(kvp.Value.X, kvp.Value.Y)).First();
                currentTargetId = targetEntry.Key;
            }
            else
            {
                var targetEntry = enemyTracker.OrderBy(kvp => DistanceTo(kvp.Value.X, kvp.Value.Y)).FirstOrDefault();
                if (targetEntry.Value != null)
                    currentTargetId = targetEntry.Key;
            }
        }

        if (currentTargetId == null)
            return;

        EnemyData target = enemyTracker[currentTargetId];
        double angleToEnemy = NormalizeAbsoluteAngle(Direction + BearingTo(target.X, target.Y));
        double distance = DistanceTo(target.X, target.Y);

        double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);
        double extraTurn = Math.Min(Math.Atan(36.0 / distance), MaxTurnRate);
        radarTurn += (radarTurn < 0 ? -extraTurn : extraTurn);
        SetTurnRadarLeft(radarTurn);

        double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);
        SetTurnGunLeft(gunTurn);
        if (Math.Abs(gunTurn) < 10)
        {
            double firePower = lockedOn ? 3 : 2;
            Fire(firePower);
        }

        double moveAngle = NormalizeRelativeAngle(angleToEnemy - Direction);
        if (moveAngle >= 0)
            SetTurnLeft(moveAngle);
        else
            SetTurnRight(-moveAngle);

        if (distance > 100)
            SetForward(100);
        else
            SetForward(50);
    }

    private bool IsEnemyInCorner(EnemyData enemy)
    {
        return (enemy.X <= CornerThreshold || enemy.X >= ArenaWidth - CornerThreshold) &&
               (enemy.Y <= CornerThreshold || enemy.Y >= ArenaHeight - CornerThreshold);
    }

    private void MoveToNearestCorner()
    {
        var corners = new (double X, double Y)[]
        {
            (CornerThreshold, CornerThreshold),                              
            (ArenaWidth - CornerThreshold, CornerThreshold),                
            (CornerThreshold, ArenaHeight - CornerThreshold),               
            (ArenaWidth - CornerThreshold, ArenaHeight - CornerThreshold)     
        };

        (double targetX, double targetY) = corners.OrderBy(corner =>
            Math.Sqrt(Math.Pow(corner.X - X, 2) + Math.Pow(corner.Y - Y, 2))
        ).First();

        double angleToCorner = Math.Atan2(targetY - Y, targetX - X) * 180 / Math.PI;
        double turnAngle = NormalizeRelativeAngle(angleToCorner - Direction);
        if (turnAngle >= 0)
            SetTurnRight(turnAngle);
        else
            SetTurnLeft(-turnAngle);

        double distance = Math.Sqrt(Math.Pow(targetX - X, 2) + Math.Pow(targetY - Y, 2));
        SetForward(distance);

        Go();
    }
    private void EitsBehavior(ScannedBotEvent e)
    {
        double bearing = BearingTo(e.X, e.Y);
        double angleToEnemy = Direction + bearing;
        double distance = DistanceTo(e.X, e.Y);

        double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);
        double extraTurn = Math.Min(Math.Atan(36.0 / distance), MaxTurnRate);
        radarTurn += (radarTurn < 0 ? -extraTurn : extraTurn);
        SetTurnRadarLeft(radarTurn);

        double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);
        SetTurnGunLeft(gunTurn);
        double firepower = (distance < 200) ? 3 : 2;
        SetFire(firepower);

        double desiredDistance = 250;
        double distanceError = distance - desiredDistance;
        if (distanceError > 50)
        {
            SetTurnLeft(NormalizeRelativeAngle(angleToEnemy - Direction));
            SetForward(100);
        }
        else if (distanceError < -50)
        {
            SetTurnRight(NormalizeRelativeAngle(angleToEnemy + Direction));
            SetBack(100);
        }

        if (enemyEnergy.ContainsKey(e.ScannedBotId))
        {
            double energyDrop = enemyEnergy[e.ScannedBotId] - e.Energy;
            if (energyDrop >= 0.1 && energyDrop <= 3.0)
            {
                PerformDodge();
            }
        }
        enemyEnergy[e.ScannedBotId] = e.Energy;
    }

    private void PerformDodge()
    {
        double moveAngle = Direction + (random.Next(2) == 0 ? 90 : -90);
        SetTurnLeft(NormalizeRelativeAngle(moveAngle - Direction));
        SetForward(100);
    }
    public override void OnHitBot(HitBotEvent e)
    {
        TurnToFaceTarget(e.X, e.Y);
        if (e.Energy > 10)
            Fire(3);
        else if (e.Energy > 4)
            Fire(1);
        else if (e.Energy > 2)
            Fire(0.5);
        else if (e.Energy > 0.4)
            Fire(0.1);
        Forward(40);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        double bearing = BearingTo(X, Y);
        if (bearing >= 0)
            SetTurnLeft(bearing);
        else
            SetTurnRight(bearing);
        SetForward(50);
    }

    private void TurnToFaceTarget(double targetX, double targetY)
    {
        double desiredAngle = Math.Atan2(targetY - Y, targetX - X) * 180 / Math.PI;
        double turnAngle = NormalizeRelativeAngle(desiredAngle - Direction);
        if (turnAngle >= 0)
            SetTurnRight(turnAngle);
        else
            SetTurnLeft(-turnAngle);
        Go();
    }
}
