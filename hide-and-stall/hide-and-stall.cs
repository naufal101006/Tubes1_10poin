using Robocode.TankRoyale.BotApi;
using System.Drawing;
using Robocode.TankRoyale.BotApi.Events;
using System.Collections.Generic;
using System;

public class HideAndStall : Bot
{
    public Dictionary<int, double[]> RobotList { get; private set; }
    public double[][] Grid { get; private set; }

    // The main method starts our bot
    static void Main(string[] args)
    {
        new HideAndStall().Start();
    }

    // Constructor, which loads the bot config file
    HideAndStall() : base(BotInfo.FromFile("hide-and-stall.json")) { }

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
        RobotList = new Dictionary<int, double[]>();
        double[] LastTarget = {-1, -1};
        Random RNG = new();

        TurnRadarLeft(360);
        while (IsRunning)
        {
            SetTurnRadarLeft(360);
            bool DoJitter = true;

            CalculateGrid();
            int[] CurrentGrid = new int[2] {(int)(X*4/ArenaWidth), (int)(Y*4/ArenaHeight)};

            // Grid is too dense
            if (Grid[CurrentGrid[0]][CurrentGrid[1]] > 0.4) {
                double Minimum = 9999;
                double[] GridCoords = new double[2] {0, 0};

                for (int i = 0; i < Grid.Length; i++) {
                    for (int j = 0; j < Grid[0].Length; j++) {
                        if (Grid[i][j] < Minimum) {
                            GridCoords[0] = ((double)j + 1.0f/2.0f) * ArenaWidth/8;
                            GridCoords[1] = ((double)i + 1.0f/2.0f) * ArenaHeight/8;

                            Minimum = Grid[i][j];
                        }
                    }
                }

                double AngleToPoint = BearingTo(GridCoords[0], GridCoords[1]);
                double DistanceToPoint = DistanceTo(GridCoords[0], GridCoords[1]);
                if (LastTarget[0] != GridCoords[0] || LastTarget[1] != GridCoords[1] || DistanceToPoint > 50) {
                    DoJitter = false;

                    if (AngleToPoint > 0) {
                        SetTurnLeft(AngleToPoint);
                    } else if (AngleToPoint < 0) {
                        SetTurnRight(-AngleToPoint);
                    }

                    // Forwards only if facing correctly
                    if (AngleToPoint < 30 && AngleToPoint > -30) {
                        SetForward(DistanceToPoint);
                    }
                    Go();

                    LastTarget[0] = GridCoords[0];
                    LastTarget[1] = GridCoords[1];
                }
            }
            
            // If not moving to different grid, jitter
            if (DoJitter) {
                SetTurnLeft(RNG.NextDouble()*360);
                Forward((RNG.Next(0, 2)*2-1) * RNG.Next(1, 3)*18);
                Go();
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double[] coords = {e.X, e.Y};
        RobotList[e.ScannedBotId] =  coords;
    }

    // When hitting a bot, move 90deg to dodge
    public override void OnHitBot(HitBotEvent e)
    {
        TurnLeft(BearingTo(e.X, e.Y) + 90);
        Forward(64);
    }

    // Calculate density grid
    private void CalculateGrid() {
        Grid = new double[4][] {
            new double[4] {0, 0, 0, 0},
            new double[4] {0, 0, 0, 0},
            new double[4] {0, 0, 0, 0},
            new double[4] {0, 0, 0, 0},
        };

        foreach (var Coords in RobotList)
        {
            Grid[(int)(Coords.Value[1]*4 / ArenaHeight)][(int)(Coords.Value[0]*4 / ArenaWidth)] += 1;
        }
        
        // Diffuse grid
        for (int k = 0; k < 2; k++) {
            for (int i = 0; i < Grid.Length; i++)
            {
                for (int j = 0; j < Grid[0].Length; j++)
                {
                    Grid[i][j] += (
                        GetN(Grid, i, j, -1, -1) + GetN(Grid, i, j, -1, 0) + GetN(Grid, i, j, -1, 1)
                        + GetN(Grid, i, j, 0, -1) + 0 + GetN(Grid, i, j, 0, 1)
                        + GetN(Grid, i, j, 1, -1) + GetN(Grid, i, j, 1, 0) + GetN(Grid, i, j, 1, 1)
                    )/8.0f;
                }
            }
        }
    }

    // Get grid neighbour, reflecting border
    static double GetN(double[][] Grid, int i, int j, int ic, int jc) {
        if (i+ic < 0 || i+ic >= Grid.Length || j+jc < 0 || j+jc >= Grid[0].Length) {
            return Grid[i][j];
        } else {
            return Grid[i+ic][j+jc];
        };
    }
}

