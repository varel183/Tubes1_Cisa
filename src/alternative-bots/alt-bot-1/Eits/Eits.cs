using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Eits : Bot
{
    static void Main(string[] args)
    {
        new Eits().Start();
    }

    Eits() : base(BotInfo.FromFile("Eits.json")) { }

    private Dictionary<int, double> enemyEnergy = new Dictionary<int, double>();
    public override void Run()
    {
        BodyColor = Color.White;
        GunColor = Color.White;
        TurretColor = Color.White;
        RadarColor = Color.White;
        ScanColor = Color.White;
        BulletColor = Color.White;

        while (IsRunning)
        {
            TurnRadarLeft(Double.PositiveInfinity);
        }

    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double bearing = BearingTo(e.X, e.Y);
        double angleToEnemy = Direction + bearing;
        double distance = DistanceTo(e.X, e.Y);

        // Radar Tracking
        double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);
        double extraTurn = Math.Min(Math.Atan(36.0 / distance), MaxTurnRate);
        radarTurn += (radarTurn < 0 ? -extraTurn : extraTurn);
        SetTurnRadarLeft(radarTurn);

        // Gun Tracking and Firing
        double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);
        SetTurnGunLeft(gunTurn);

        double firepower = (distance < 200) ? 3 : 2;
        SetFire(firepower);

        double desiredDistance = 250;
        double distanceError = distance - desiredDistance;

        // Jaga jarak dari musuh
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

        // Detect Penembakan Bullet
        if (enemyEnergy.ContainsKey(e.ScannedBotId))
        {
            double energyDrop = enemyEnergy[e.ScannedBotId] - e.Energy;

            if (energyDrop >= 0.1 && energyDrop <= 3.0)
            {
                PerformDodge();

            }
        }

        // Update energi musuh
        enemyEnergy[e.ScannedBotId] = e.Energy;
    }


    // Simple Dodging Bullet
    private void PerformDodge()
    {
        double moveAngle = Direction + (new Random().Next(2) == 0 ? 90 : -90);
        SetTurnLeft(NormalizeRelativeAngle(moveAngle - Direction));
        SetForward(100);
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