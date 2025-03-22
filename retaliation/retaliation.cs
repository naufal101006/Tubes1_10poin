using Robocode.TankRoyale.BotApi;
using System.Drawing;
using Robocode.TankRoyale.BotApi.Events;
using System;

public class TargetBot {
    public int VictimId { get; }

    public double Energy { get; }

    public double X { get; set; }

    public double Y { get; set; }

    public bool IsRammed { get; }

    public TargetBot(int victimId, double energy, double x, double y, bool isRammed) {
        bool flag = isRammed;
        VictimId = victimId;
        Energy = energy;
        X = x;
        Y = y;
        IsRammed = flag;
    }
}


public class Retaliation : Bot
{
    #nullable enable
    public TargetBot? TargetBot { get; private set; }

    // The main method starts our bot
    static void Main(string[] args)
    {
        new Retaliation().Start();
    }

    // Constructor, which loads the bot config file
    Retaliation() : base(BotInfo.FromFile("retaliation.json")) { }

    // Called when a new round is started -> initialize and do some movement
    public override void Run()
    {

        BodyColor = Color.FromArgb(0x1c, 0xa3, 0x3e);
        TurretColor = Color.FromArgb(0x08, 0x30, 0x12);
        RadarColor = Color.FromArgb(0x00, 0xff, 0x40);
        BulletColor = Color.FromArgb(0xa8, 0xff, 0xbe);
        ScanColor = Color.FromArgb(0x00, 0xff, 0x40);
        TracksColor = Color.FromArgb(0x55, 0x55, 0x55);
        GunColor = Color.FromArgb(0x0c, 0x70, 0x4c);
        
        // Repeat while the bot is running
        while (IsRunning) {
            SetTurnRadarLeft(360);

            if (TurnNumber % 80 == 0 && TargetBot is null) {
                SetForward(256 * (TurnNumber % 160 / 40 - 1));
            }

            if (TargetBot is not null) {
                double Distance = DistanceTo(TargetBot.X, TargetBot.Y);

                if (Distance < 40) {
                    double GunBearing = GunBearingTo(TargetBot.X, TargetBot.Y);
                    SetTurnGunLeft(GunBearing);

                    if (GunBearing < 0.5 && GunBearing > -0.5) {
                        SetFire(Math.Max(0.1, Math.Min(TargetBot.Energy/4, 3)));
                    }
                } else {
                    TargetBot = null;
                }
            }

            Go();
        }
    }
    
    public override void OnHitBot(HitBotEvent e) {
        if (TargetBot is null) {
            TargetBot = new TargetBot(e.VictimId, e.Energy, e.X, e.Y, e.IsRammed);
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        if (TargetBot?.VictimId == e.VictimId) {
            TargetBot = null;
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (e.ScannedBotId == TargetBot?.VictimId) {
            TargetBot.X = e.X;
            TargetBot.Y = e.Y;
        }
    }
}

