// File: src/Chunks/ChunkMeshGenerator.cs
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HyatlasGame.Core.Blocks;         // für BlockTextureCache
using HyatlasGame.Core.Shared.Utils;
using WorldModel = HyatlasGame.Core.World.World;

namespace HyatlasGame.Core.Chunks
{
    /// <summary>
    /// Baut aus den Voxeln eines Chunks ein Greedy‑Mesh (zusammen‑gemergte Quads)
    /// und liefert Vertex‑/Index‑Arrays plus die Chunk‑Textur für einen GPU‑Buffer.
    /// </summary>
    public class ChunkMeshGenerator
    {
        private readonly WorldModel _world;

        public ChunkMeshGenerator(WorldModel world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        /* ---------- Ergebnis‑Typ ---------- */
        public readonly struct MeshData
        {
            public readonly VertexPositionTexture[] Vertices;
            public readonly int[]                   Indices;
            public readonly Texture2D               Texture;

            public MeshData(VertexPositionTexture[] v, int[] i, Texture2D tex)
            {
                Vertices = v;
                Indices  = i;
                Texture  = tex;
            }
        }

        /* ---------- Hilfstypen für Greedy‑Mesh ---------- */
        private struct FaceInfo { public int Dir; public byte BlockId; public FaceInfo(int d, byte b) { Dir = d; BlockId = b; } }
        private readonly struct DirCfg
        {
            public readonly int U;
            public readonly int V;
            public readonly int W;
            public readonly int Sign;
            public DirCfg(int u, int v, int w, int s) { U = u; V = v; W = w; Sign = s; }
        }
        private static readonly DirCfg[] _dir = {
            new(2,1,0,-1), new(2,1,0,+1),
            new(0,2,1,-1), new(0,2,1,+1),
            new(0,1,2,-1), new(0,1,2,+1)
        };

        /// <summary>Generiert ein MeshData-Objekt für den gegebenen Chunk.</summary>
        public MeshData GenerateMesh(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException(nameof(chunk));

            var vList = new List<VertexPositionTexture>();
            var iList = new List<int>();
            int idx = 0;

            int W = chunk.Width, H = chunk.Height, D = chunk.Depth;
            int baseX = chunk.ChunkX * W, baseZ = chunk.ChunkZ * D;

            // Hole die Standard-Textur (hier ID=1 = Erde) aus dem Cache
            Texture2D chunkTexture = BlockTextureCache.Get(1);

            byte AtGlobal(int x, int y, int z)
            {
                if (y < 0 || y >= H) return 0;
                if (x >= 0 && x < W && z >= 0 && z < D)
                    return chunk.GetBlock(new Vector3Int(x, y, z));
                return _world.GetBlockAt(new Vector3Int(baseX + x, y, baseZ + z));
            }

            for (int dir = 0; dir < 6; dir++)
            {
                var cfg = _dir[dir];
                int du = (cfg.U == 0 ? W : cfg.U == 1 ? H : D);
                int dv = (cfg.V == 0 ? W : cfg.V == 1 ? H : D);
                int dw = (cfg.W == 0 ? W : cfg.W == 1 ? H : D);

                var mask = new FaceInfo[du * dv];

                // 1) Maske füllen
                for (int w = 0; w < dw; w++)
                {
                    for (int v = 0; v < dv; v++)
                    for (int u = 0; u < du; u++)
                    {
                        int[] xyz = { 0, 0, 0 };
                        xyz[cfg.U] = u; xyz[cfg.V] = v; xyz[cfg.W] = w;
                        int ox = xyz[0] + (cfg.W == 0 ? cfg.Sign : 0);
                        int oy = xyz[1] + (cfg.W == 1 ? cfg.Sign : 0);
                        int oz = xyz[2] + (cfg.W == 2 ? cfg.Sign : 0);

                        byte id  = AtGlobal(xyz[0], xyz[1], xyz[2]);
                        byte idO = AtGlobal(ox, oy, oz);
                        mask[v * du + u] = (id != 0 && idO == 0) ? new FaceInfo(dir, id) : default;
                    }
                }

                // 2) Greedy‑Merge und Quads erzeugen
                for (int w = 0; w < dw; w++)
                {
                    for (int v = 0; v < dv; v++)
                    for (int u = 0; u < du; u++)
                    {
                        var face = mask[v * du + u];
                        if (face.BlockId == 0) continue;

                        int width = 1;
                        while (u + width < du && mask[v * du + u + width].BlockId == face.BlockId)
                            width++;
                        int height = 1; bool grow = true;
                        while (v + height < dv && grow)
                        {
                            for (int k = 0; k < width; k++)
                                if (mask[(v + height) * du + u + k].BlockId != face.BlockId)
                                { grow = false; break; }
                            if (grow) height++;
                        }

                        MakeQuad(
                            dir, face.BlockId,
                            u, v, w, width, height,
                            cfg, baseX, baseZ,
                            ref vList, ref iList, ref idx);

                        for (int dvv = 0; dvv < height; dvv++)
                        for (int duu = 0; duu < width; duu++)
                            mask[(v + dvv) * du + u + duu] = default;
                    }
                }
            }

            return new MeshData(vList.ToArray(), iList.ToArray(), chunkTexture);
        }

        /// <summary>Erzeugt ein Quad mit Voll‑UV 0→1 und fügt es ein.</summary>
        private static void MakeQuad(
            int dir, byte id,
            int u, int v, int w, int wLen, int hLen,
            DirCfg cfg,
            int baseX, int baseZ,
            ref List<VertexPositionTexture> vList,
            ref List<int> iList,
            ref int index)
        {
            int[] xyz = { 0, 0, 0 };
            xyz[cfg.U] = u; xyz[cfg.V] = v; xyz[cfg.W] = (cfg.Sign > 0 ? w + 1 : w);

            Vector3 du = AxisToVec(cfg.U) * wLen;
            Vector3 dv = AxisToVec(cfg.V) * hLen;

            Vector3 p0 = new(xyz[0] + baseX, xyz[1],           xyz[2] + baseZ);
            Vector3 p1 = p0 + du;
            Vector3 p2 = p0 + dv;
            Vector3 p3 = p0 + du + dv;
            if (cfg.Sign < 0) (p1, p2) = (p2, p1);

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 0);
            var uv2 = new Vector2(0, 1);
            var uv3 = new Vector2(1, 1);

            vList.Add(new VertexPositionTexture(p0, uv0));
            vList.Add(new VertexPositionTexture(p1, uv1));
            vList.Add(new VertexPositionTexture(p2, uv2));
            vList.Add(new VertexPositionTexture(p3, uv3));

            iList.AddRange(new[]
            {
                index, index + 1, index + 2,
                index + 2, index + 1, index + 3
            });
            index += 4;
        }

        private static Vector3 AxisToVec(int axis) => axis switch
        {
            0 => Vector3.UnitX,
            1 => Vector3.UnitY,
            _ => Vector3.UnitZ
        };
    }
}
