using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using VRageMath;

namespace IngameScript
{
    public class Grid
    {
        bool customSeed;

        int arraySizeX, arraySizeY, iterator = Constants.xmin;
        Dictionary<Vector2, bool>
            buffer = new Dictionary<Vector2, bool>(),
            bufferOld = new Dictionary<Vector2, bool>();
        Random rand = new Random();

        public Grid(MyGridProgram program, bool custom = false, Vector2? size = null) 
        {
            customSeed = custom;
            arraySizeX = (int)(size.HasValue ? size.Value.X : Constants.xrange);
            arraySizeY = (int)(size.HasValue ? size.Value.Y : Constants.yrange);
            for (int i = Constants.xmin; i < Constants.xmax; ++i)
                for (int j = Constants.ymin; j < Constants.ymax; ++j)
            {
                var c = new Vector2(i, j);    
                if (!customSeed)
                    if (rand.NextDouble() < 0.29)
                        buffer.Add(c, true);
                    else
                        buffer.Add(c, false);
                if (buffer[c])
                    program.Echo($"Cell born at {c}");
            }
        }

        public Dictionary<Vector2, bool> Buffer { get { return buffer; } }
        Vector2 WrapCoords(ref Vector2 v)
        {
            v.X = v.X > arraySizeX ? 0 : v.X;
            v.X = v.X < 0 ? arraySizeX : v.X;
            v.Y = v.Y > arraySizeY ? 0 : v.Y;
            v.Y = v.Y < 0 ? arraySizeY : v.Y;
            return v;
        }

        public bool Get(Vector2 v)
        {
            v = WrapCoords(ref v);
            return buffer[v];
        }

        public void Set(Vector2 v, bool b = true)
        {
            v = WrapCoords(ref v);
            buffer[v] = b;
        }

        public void Copy()
        {
            bufferOld = buffer;
        }

        public void Update(int cycles = 0)
        {
            if (cycles == 0) // this is too much for it to do in a single run. gonna leave it in tho
                updateLoop(buffer.Keys); 
            else
            {
                var array = new List<Vector2>();
                for(int i = iterator; i < iterator + cycles; ++i)
                    for (int j = Constants.ymin; j < Constants.ymax; ++j)
                        array.Add(new Vector2(i, j));
                    updateLoop(array);

                iterator++;
            }
            if (iterator >= Constants.xmax) iterator = Constants.xmin;
        }

        void updateLoop(IEnumerable<Vector2> group)
        {
            foreach (var position in group)
            {
                // theres proabbly a WAYY better way of doiing this
                var x = position.X;
                var y = position.Y;
                var adj = new Vector2[8];
                var ret = new List<bool>();
                adj[0] = new Vector2(x - 1, y - 1);
                adj[1] = new Vector2(x - 1, y + 1);
                adj[2] = new Vector2(x - 1, y);
                adj[3] = new Vector2(x, y - 1);
                adj[4] = new Vector2(x, y + 1);
                adj[5] = new Vector2(x + 1, y - 1);
                adj[6] = new Vector2(x + 1, y);
                adj[7] = new Vector2(x + 1, y + 1);
                for (int i = 0; i < 8; ++i)
                    WrapCoords(ref adj[i]);
                var count = 0;

                foreach (var v in adj)
                    if (bufferOld.ContainsKey(v))
                        if(bufferOld[v])
                            ++count;

                    if (count == 3)
                        // if cell have 3 neighbor it alive
                        buffer[position] = true;
                    else if (bufferOld[position] && count == 2)
                        // if cell have 2 neighbor and it alr alive it stay alive
                        buffer[position] = true;
                    else
                        // otherwise it dood
                        buffer[position] = false;               

            }          
        }


    }
}