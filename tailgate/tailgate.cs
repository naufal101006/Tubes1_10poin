using Robocode.TankRoyale.BotApi;
using System.Drawing;
using Robocode.TankRoyale.BotApi.Events;
using System.Collections.Generic;
using System;

public class Tailgate : Bot
{
    #nullable enable
    public ScannedBotEvent? TailgateTarget { get; private set; }
    public ScannedBotEvent? GunTarget { get; private set; }

    // The main method starts our bot
    static void Main(string[] args)
    {
        new Tailgate().Start();
    }

    // Constructor, which loads the bot config file
    Tailgate() : base(BotInfo.FromFile("tailgate.json")) { }

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
            
            // Follow TailgateTarget
            if (TailgateTarget is not null) {
                double TargetX = TailgateTarget.X - 36*Math.Cos(TailgateTarget.Direction/180 * Math.PI);
                double TargetY = TailgateTarget.Y - 36*Math.Sin(TailgateTarget.Direction/180 * Math.PI);
                double Distance = DistanceTo(TargetX, TargetY);
                double Direction = BearingTo(TargetX, TargetY);

                SetTurnLeft(Direction);
                if (Direction < 30 && Direction > -30) {
                    SetForward(Distance);
                }

            }

            // Shoot GunTarget
            if (GunTarget is not null) {
                double Direction = GunBearingTo(GunTarget.X, GunTarget.Y);

                SetTurnGunLeft(Direction);

                if (Direction < 0.5 && Direction > -0.5) {
                    SetFire(1);
                }

                if (TurnNumber - GunTarget.TurnNumber >= 10) {
                    GunTarget = null;
                }
            }

            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (TailgateTarget is null || TailgateTarget?.ScannedBotId == e.ScannedBotId) {
            TailgateTarget = e;
        }

        if (GunTarget?.ScannedBotId == e.ScannedBotId || (GunTarget is null && TailgateTarget?.ScannedBotId != e.ScannedBotId)) {
            GunTarget = e;
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        if (TailgateTarget?.ScannedBotId == e.VictimId) {
            TailgateTarget = null;
        }

        if (GunTarget?.ScannedBotId == e.VictimId) {
            GunTarget = null;
        }
    }
}

