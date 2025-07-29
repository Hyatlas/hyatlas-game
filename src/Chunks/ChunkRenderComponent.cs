// File: src/Chunks/ChunkRenderComponent.cs
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HyatlasGame.Core.Chunks;    // für ChunkMeshGenerator
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.Chunks
{
    /// <summary>
    /// Hält Vertex‑ und Index‑Buffer für einen Chunk und erlaubt das Zeichnen
    /// in genau einem GPU‑Aufruf, wahlweise mit BasicEffect für Texturen.
    /// </summary>
    public sealed class ChunkRenderComponent : IDisposable
    {
        public VertexBuffer VertexBuffer { get; }
        public IndexBuffer  IndexBuffer  { get; }
        public Texture2D    Texture      { get; }

        /// <summary>Anzahl der Dreiecke (Index‑Tripel).</summary>
        public int PrimitiveCount { get; }

        /// <summary>True, wenn der Chunk keine sichtbare Geometrie enthält.</summary>
        public bool IsEmpty => PrimitiveCount == 0;

        public ChunkRenderComponent(GraphicsDevice device, ChunkMeshGenerator.MeshData mesh)
        {
            if (device        == null) throw new ArgumentNullException(nameof(device));
            if (mesh.Vertices == null) throw new ArgumentNullException(nameof(mesh.Vertices));
            if (mesh.Indices  == null) throw new ArgumentNullException(nameof(mesh.Indices));
            if (mesh.Texture  == null) throw new ArgumentNullException(nameof(mesh.Texture));

            Texture = mesh.Texture;

            // Leerer Chunk → Dummy-Puffer
            if (mesh.Vertices.Length == 0 || mesh.Indices.Length == 0)
            {
                VertexBuffer   = new VertexBuffer(device, typeof(VertexPositionTexture), 1, BufferUsage.None);
                IndexBuffer    = new IndexBuffer(device, IndexElementSize.SixteenBits, 1, BufferUsage.None);
                PrimitiveCount = 0;
                return;
            }

            // Vertex-Buffer (VertexPositionTexture)
            VertexBuffer = new VertexBuffer(
                device,
                typeof(VertexPositionTexture),
                mesh.Vertices.Length,
                BufferUsage.WriteOnly);
            VertexBuffer.SetData(mesh.Vertices);

            // Index-Buffer (16‑/32‑Bit)
            bool require32 =
                   mesh.Vertices.Length > ushort.MaxValue
                || MaxIndex(mesh.Indices)      > ushort.MaxValue;

            if (require32)
            {
                IndexBuffer = new IndexBuffer(
                    device,
                    IndexElementSize.ThirtyTwoBits,
                    mesh.Indices.Length,
                    BufferUsage.WriteOnly);
                IndexBuffer.SetData(mesh.Indices);
            }
            else
            {
                var idx16 = new ushort[mesh.Indices.Length];
                for (int i = 0; i < idx16.Length; i++)
                    idx16[i] = (ushort)mesh.Indices[i];

                IndexBuffer = new IndexBuffer(
                    device,
                    IndexElementSize.SixteenBits,
                    idx16.Length,
                    BufferUsage.WriteOnly);
                IndexBuffer.SetData(idx16);
            }

            PrimitiveCount = mesh.Indices.Length / 3;
        }

        /// <summary>
        /// Zeichnet den Chunk mit einfachem Device-Aufruf (Drahtgitter oder Farb-Effekt).
        /// </summary>
        public void Draw(GraphicsDevice device)
        {
            if (IsEmpty) return;

            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;
            device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                baseVertex:     0,
                startIndex:     0,
                primitiveCount: PrimitiveCount);
        }

        /// <summary>
        /// Zeichnet den Chunk mit BasicEffect und bindet die Textur.
        /// </summary>
        public void Draw(GraphicsDevice device, BasicEffect effect)
        {
            if (IsEmpty)         return;
            if (effect == null)  throw new ArgumentNullException(nameof(effect));

            effect.Texture        = Texture;
            effect.TextureEnabled = true;

            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;
            device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                baseVertex:     0,
                startIndex:     0,
                primitiveCount: PrimitiveCount);
        }

        private static int MaxIndex(int[] indices)
        {
            int max = 0;
            foreach (int i in indices)
                if (i > max) max = i;
            return max;
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
