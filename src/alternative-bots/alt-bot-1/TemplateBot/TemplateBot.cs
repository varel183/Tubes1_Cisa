using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class TemplateBot : Bot
{
    private bool enemyDetected = false;
    private bool firstTime = false;
    private double lastEnemyX = 0;
    private double lastEnemyY = 0;
    private double lastEnemyEnergy = 100;
    private int missedScans = 0;

    private static readonly Random rnd = new Random();

    static void Main(string[] args)
    {
        new TemplateBot().Start();
    }

    TemplateBot() : base(BotInfo.FromFile("TemplateBot.json")) { }

    public override void Run()
    {
        BodyColor = Color.FromArgb(0x33, 0x33, 0x33);    
        TurretColor = Color.FromArgb(0xCC, 0x33, 0x00);  
        RadarColor = Color.FromArgb(0xFF, 0x99, 0x00);   
        BulletColor = Color.FromArgb(0x00, 0xCC, 0xFF);  
        ScanColor = Color.FromArgb(0xFF, 0xFF, 0x00);   

        enemyDetected = false;
        missedScans = 0;

        firstTime = true;

        if (firstTime) {
            MaxRadarTurnRate = 10;
            SetTurnGunRight(360);
        }

        firstTime = false;

        while (IsRunning)
        {
            WallSmoothing();

            if (!enemyDetected)
            {
                SetTurnGunRight(360);
            }
            else
            {
                missedScans++;
                if (missedScans > 10)
                {
                    enemyDetected = false;
                }
                
                double gunBearing = NormalizeRelativeAngle(DirectionTo(lastEnemyX, lastEnemyY) - GunDirection);
                SetTurnGunLeft(gunBearing);
            }
            
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        enemyDetected = true;
        missedScans = 0;
        lastEnemyX = e.X;
        lastEnemyY = e.Y;
        lastEnemyEnergy = e.Energy;

        double targetAngle = DirectionTo(e.X, e.Y);
            
        TurnGunTo(targetAngle);

        double absoluteBearing = DirectionTo(e.X, e.Y);
        
        double turnAngle = NormalizeRelativeAngle(absoluteBearing - Direction);
        SetTurnRight(turnAngle);
        
        if (DistanceTo(e.X, e.Y) < 200) {
            SetForward(50); 
        } else {
            SetForward(20);
        }
        
        double gunAdjust = NormalizeRelativeAngle(absoluteBearing - GunDirection);
        SetTurnGunRight(gunAdjust);

        
        
    double currentDistance = DistanceTo(e.X, e.Y);
    if (currentDistance < 20 && Energy > 30)
        Fire(3);
    else if (currentDistance < 100)
        Fire(2);
    else
        Fire(1);
        
        Rescan();
    }

private void WallSmoothing()
{
    double distanceToWall = 50; 
    double battlefieldWidth = 800;
    double battlefieldHeight = 600;

    bool nearLeftWall = X < distanceToWall;
    bool nearRightWall = X > battlefieldWidth - distanceToWall;
    bool nearBottomWall = Y < distanceToWall;
    bool nearTopWall = Y > battlefieldHeight - distanceToWall;

    if (nearLeftWall || nearRightWall)
    {
        SetTurnRight(45); 
        SetBack(30); 
    }
    if (nearBottomWall || nearTopWall)
    {
        SetTurnRight(45);
        SetBack(30);
    }
}


    public override void OnHitByBullet(HitByBulletEvent e)
    {
        SetTurnLeft(-1 * rnd.Next(45, 180)); 
        MaxSpeed = rnd.Next(3, 8);
        Rescan();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        lastEnemyX = e.X;
        lastEnemyY = e.Y;
        
        if (!enemyDetected)
        {
            enemyDetected = true;
            missedScans = 0;
        }
        
        TurnGunTo(DirectionTo(e.X, e.Y));

        if (e.Energy > 16)
            Fire(3);
        else if (e.Energy > 10)
            Fire(2);
        else if (e.Energy > 4)
            Fire(1);
        else if (e.Energy > 2)
            Fire(0.5);
        else if (e.Energy > 0.4)
            Fire(0.1);
            
        if (e.IsRammed)
        {
            SetBack(100);
            TurnLeft(rnd.Next(30, 90));
        }
    }

    public override void OnHitWall(HitWallEvent e)
    {
        SetTurnRight(120);
        SetForward(100);
    }

    private void TurnGunTo(double targetAngle)
    {
        double gunTurn = NormalizeRelativeAngle(targetAngle - GunDirection);
        if (gunTurn >= 0)
            SetTurnGunRight(gunTurn);
        else
            SetTurnGunLeft(-gunTurn);
    }
}