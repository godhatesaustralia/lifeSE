using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.Achievements;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public MyIni ini;
        public Random rand;
        Grid grid;
        public IMyTextSurface drawSurface;
        public MyGridProgram program;
        public bool customSeed;
        int runs = 0;
        double runtimeMS, worst;
        string cdtag = "[CUSTOM]";

        // Custom data just
        // [CUSTOM]
        // (X1, Y1)
        // (X2, Y2)
        // ...
        // (XN, YN)
        // where these are spots youre setting to true

        public Program()
        {            
            program = this;
            rand = new Random();
            ini = new MyIni();
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            var blcok = GridTerminalSystem.GetBlockWithName("EVT LCD Console CGL");
            drawSurface = ((IMyTextSurfaceProvider)blcok).GetSurface(0);
            customSeed = blcok.CustomData.Contains(cdtag);
            Vector2 screen;
            if (drawSurface != null )
            {
                drawSurface.ContentType = ContentType.SCRIPT;
                drawSurface.ScriptBackgroundColor = new Color(7, 7, 7, 255);
                screen = drawSurface.SurfaceSize;
            }
            if (customSeed)
            {
                grid = new Grid(this, true);
                string cd = blcok.CustomData;
                var array = cd.Split('\n');
                foreach (var line in array)
                {
                    if (line.Contains(cdtag)) 
                        continue;
                    line.Trim(new char[] { '(', ')' });
                    var coords = line.Split(',');
                    if (coords.Length != 2) 
                        continue;
                    grid.Set(new Vector2(float.Parse(coords[0]), float.Parse(coords[1])));
                }
            }
            else
            {
                grid = new Grid(this);
                // Me.CustomData = "";
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            runtimeMS = Runtime.TimeSinceLastRun.TotalMilliseconds;
            worst = runtimeMS > worst ? runtimeMS : worst;
            var echoString = $"{runs} cycles\n";
            var tickModulus = runs % Constants.xrange;
            if (runs <= Constants.xrange)
            {
                ++runs;
                if (tickModulus == 0)
                    grid.Copy();
                return;
            }

            else if (tickModulus != 0)
                grid.Update(1);


            else if (tickModulus == 0)
            {
                grid.Copy();
                Me.CustomData = "";

                echoString += $"Tick {tickModulus}";

                var frame = drawSurface.DrawFrame();
                foreach (var c in grid.Buffer)
                    if (c.Value)
                        frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", c.Key * Constants.sizeMPLR, new Vector2(Constants.sizeMPLR, Constants.sizeMPLR), Constants.display, null, TextAlignment.CENTER, 0f));

                frame.Add(new MySprite(SpriteType.TEXT, $"UPDATE {runs/Constants.xrange}           GAME OF LIFE", Constants.textPos, null, Constants.display, "VCRBold", TextAlignment.CENTER, 1f));
                frame.Dispose();
            }
            Echo(echoString + $"\nRuntime: {Runtime.TimeSinceLastRun.TotalMilliseconds} ms");
            ++runs;

        }
    }
    public static class Constants
    {
        // for entertainment hub block
        // was going to do logic automatically determining how to set this up
        // but it really relies on what subjectively looks "good" so thatd be a waste
        public static int
            xmin = 1,
            xmax = 127,
            ymin = 27,
            ymax = 119,
            xrange = xmax - xmin,
            yrange = ymax - ymin;
        public static float sizeMPLR = 8;
        public static Vector2 
            size = new Vector2(xrange, yrange),
            textPos = new Vector2(508, 752);
        public static Color display = new Color(250, 250, 250, 255);


    }
}
