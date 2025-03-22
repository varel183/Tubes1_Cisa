using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Lawrie: Bot
{
    private readonly Dictionary<int, (double X, double Y)> enemyLocations = new Dictionary<int, (double, double)>();
    private const double minDistanceFromWall = 50;
    private const double dangerZoneMargin = 50; 
    private const double enemyDangerRadius = 200; 
    private readonly Random random = new Random();

    static void Main(string[] args)
    {
        new Lawrie().Start();
    }
    
    Lawrie() : base(BotInfo.FromFile("Lawrie.json")) { }

    public override void Run()
    {
        BodyColor = Color.Green;
        TurretColor = Color.Red;
        RadarColor = Color.White;
        while (IsRunning)
        {
            TurnRadarLeft(Double.PositiveInfinity);
        }
    }
    
    public override void OnScannedBot(ScannedBotEvent e)
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
        Fire(2);

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
                .Min(enemy => Math.Sqrt(Math.Pow(enemy.Item1 - spot.X, 2) + Math.Pow(enemy.Item2 - spot.Y, 2)))).First();
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
}
