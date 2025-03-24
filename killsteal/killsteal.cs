using Robocode.TankRoyale.BotApi;
using System.Drawing;
using Robocode.TankRoyale.BotApi.Events;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public class Killsteal : Bot
{
    #nullable enable
    public ScannedBotEvent? TargetBot { get; private set; }

    // The main method starts our bot
    static void Main(string[] args)
    {
        new Killsteal().Start();
    }

    // Constructor, which loads the bot config file
    Killsteal() : base(BotInfo.FromFile("killsteal.json")) { }

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

        // Free up turning for more freedom
        AdjustGunForBodyTurn = false;
        AdjustRadarForGunTurn = false;
        AdjustRadarForBodyTurn = false;

        // Move to center
        double AngleToMiddle = BearingTo(ArenaWidth/2, ArenaHeight/2);
        double DistanceToMiddle = DistanceTo(ArenaWidth/2, ArenaHeight/2);
        if (AngleToMiddle > 0) {
            TurnLeft(AngleToMiddle);
        } else if (AngleToMiddle < 0) {
            TurnRight(-AngleToMiddle);
        }
        Forward(DistanceToMiddle);
        
        // Repeat while the bot is running
        while (IsRunning) {
            if (TargetBot is not null && (TurnNumber - TargetBot.TurnNumber) < 10) {
                AdjustRadarForGunTurn = true;

                double GunAngle = GunBearingTo(TargetBot.X, TargetBot.Y);
                double RadarAngle = RadarBearingTo(TargetBot.X, TargetBot.Y);
                double BodyAngle = BearingTo(TargetBot.X, TargetBot.Y);

                SetTurnLeft(BodyAngle);
                SetTurnGunLeft(GunAngle);
                SetTurnRadarLeft(RadarAngle + Math.Sign(RadarAngle)*20);
                SetRescan();

                if (GunAngle < 0.5 && GunAngle > -0.5) {
                    SetFire(2);
                }

                if (BodyAngle < 30 && BodyAngle > -30) {
                    SetForward(DistanceTo(TargetBot.X, TargetBot.Y));
                }
            } else {
                SetTurnRadarLeft(360);
            }

            // Stale target, stop targetting
            if (TurnNumber - TargetBot?.TurnNumber >= 10) {
                TargetBot = null;
            }

            // When no target, wobble
            if (TurnNumber % 40 == 0 && TargetBot is null) {
                SetForward(64 * (TurnNumber % 80 / 20 - 1));
            }
            Go();
        }
    }

    // Find bot with <25 Energy or last bot
    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (TargetBot?.ScannedBotId == e.ScannedBotId || (e.Energy < 25 && (TargetBot is null || e.Energy < TargetBot.Energy)) || EnemyCount < 2) {
            TargetBot = e;
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        if (e.VictimId == TargetBot?.ScannedBotId) {
            TargetBot = null;
        }
    }
}

