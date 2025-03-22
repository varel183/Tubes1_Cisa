using System;
using System.Drawing;
using System.Collections.Generic;
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

public class Maniac : Bot
{
    private readonly Dictionary<string, EnemyData> enemyTracker = new Dictionary<string, EnemyData>();
    private static readonly Random rnd = new Random();

    private string currentTargetId = null;
    private bool inCorner = false;
    private const double CornerThreshold = 50;

    static void Main(string[] args)
    {
        new Maniac().Start();
    }

    Maniac() : base(BotInfo.FromFile("Maniac.json"))
    {
        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
    }

    public override void Run()
    {
        BodyColor = Color.Red;      
        GunColor = Color.Black;   
        RadarColor = Color.Lime;    
        BulletColor = Color.Red; 
        ScanColor = Color.Purple;   

        if (!inCorner)
        {
            MoveToNearestCorner();
            inCorner = true;
        }

        while (IsRunning)
        {
            TurnRadarLeft(Double.PositiveInfinity);
        }

        Rescan();
    }

    private void MoveToNearestCorner()
    {
        double arenaWidth = ArenaWidth;
        double arenaHeight = ArenaHeight;

        var corners = new (double X, double Y)[]
        {
            (CornerThreshold, CornerThreshold),                            
            (arenaWidth - CornerThreshold, CornerThreshold),                 
            (CornerThreshold, arenaHeight - CornerThreshold),                
            (arenaWidth - CornerThreshold, arenaHeight - CornerThreshold)     
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

    private bool IsEnemyInCorner(EnemyData enemy)
    {
        double arenaWidth = ArenaWidth;
        double arenaHeight = ArenaHeight;
        return (enemy.X <= CornerThreshold || enemy.X >= arenaWidth - CornerThreshold) &&
               (enemy.Y <= CornerThreshold || enemy.Y >= arenaHeight - CornerThreshold);
    }

    public override void OnScannedBot(ScannedBotEvent e)
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
        Console.WriteLine($"[DEBUG] Radar turn: {radarTurn}, RadarDirection: {RadarDirection}, AngleToEnemy: {angleToEnemy}");
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
}
