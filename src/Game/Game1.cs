// File: src/Game/Game1.cs
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HyatlasGame.Core.Blocks;
using HyatlasGame.Core.Chunks;
using HyatlasGame.Core.Entities.Player;
using HyatlasGame.Core.Rendering;
using HyatlasGame.Core.Shared.Constants;
using HyatlasGame.Core.Shared.Enums;
using HyatlasGame.Core.Shared.Utils;
using HyatlasGame.Core.World;
using HyatlasGame.Core.Zones;

namespace HyatlasGame.Core
{
    /// <summary>
    /// Einstieg in den Desktop-Renderer-Test.
    /// Lädt/streamt Chunks, rendert sie über den ChunkRenderCache
    /// und kümmert sich um sämtliche Spieler-Interaktion.
    /// </summary>
    public class Game1 : Game
    {
        /* -----------------------------------------------------------
         * Felder
         * -------------------------------------------------------- */
        private GraphicsDeviceManager _graphics = default!;
        private BasicEffect           _solidFx  = default!;
        private BasicEffect           _lineFx   = default!;      // separater Effekt für Linien

        private CameraController    _camera      = default!;
        private WorldManager        _worldMgr    = default!;
        private ChunkMeshGenerator  _meshGen     = default!;
        private MeshService         _meshService = default!;
        private ChunkRenderCache    _renderCache = default!;
        private PlayerController    _player      = default!;
        private WorldZone _currentZone = WorldZone.Surface;

        /* – Overlay – */
        private SpriteBatch _spriteBatch = default!;
        private Texture2D   _crossTex    = default!;
        private Vector2     _crossPos;

        /* – Selektion – */
        private Vector3Int?            _selectedBlock;
        private readonly VertexPositionColor[] _cubeWire = new VertexPositionColor[24];

        /* -----------------------------------------------------------
         * Konstanten
         * -------------------------------------------------------- */
        private const int   SURFACE_RADIUS     = 30;
        private const int   UNDERGROUND_RADIUS = 10;
        private const float PICK_DISTANCE      = 6f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth       = 1280,
                PreferredBackBufferHeight      = 720,
                SynchronizeWithVerticalRetrace = true
            };
            Content.RootDirectory = "Content";
            IsMouseVisible        = false;
        }

        protected override void Initialize()
        {
            base.Initialize();

            GraphicsDeviceHelper.Initialize(GraphicsDevice);

            // 1) Block-Definitions laden
            string defsPath = Path.Combine(Content.RootDirectory, "blocks.json");
            BlockDefinitionRegistry.LoadDefinitions(defsPath);

            /* --- Welt ------------------------------------------------ */
            var generator = new FlatWorldGenerator(0);
            var storage   = new FileChunkStorage("chunks", generator);
            _worldMgr     = new WorldManager(generator, storage);

            /* --- Kamera + Spieler ----------------------------------- */
            var spawnPos = new Vector3(
                0 + 0.5f,
                generator.GroundLevel + 1f,
                0 + 0.5f);
            _camera = new CameraController(spawnPos);
            _camera.InitializeMouse(this);
            _camera.AllowTranslation = false;

            _player = new PlayerController(_worldMgr, _camera, spawnPos);

            /* --- Meshing / Rendering -------------------------------- */
            _meshGen     = new ChunkMeshGenerator(_worldMgr.World);
            _meshService = new MeshService(_meshGen);
            _renderCache = new ChunkRenderCache(GraphicsDevice, _meshService);

            /* --- Overlay-Crosshair ---------------------------------- */
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _crossTex    = new Texture2D(GraphicsDevice, 1, 1);
            _crossTex.SetData(new[] { Color.White });
            _crossPos = new Vector2(
                GraphicsDevice.Viewport.Width  * 0.5f,
                GraphicsDevice.Viewport.Height * 0.5f);
        }

        protected override void LoadContent()
        {
            // BasicEffect für farbige Voxel
            _solidFx = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled     = true,   // Texturen nutzen
                VertexColorEnabled = false,
                LightingEnabled    = false
            };
            // BasicEffect für Drahtgitter
            _lineFx = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled     = false,
                VertexColorEnabled = true,
                LightingEnabled    = false
            };
        }

        protected override void Update(GameTime gameTime)
        {
            _camera.Update(gameTime, this);
            _player.Update(gameTime, this);

            _worldMgr.UpdateLoadedChunks(
                _player.Position,
                surfaceBlockRadius:     SURFACE_RADIUS,
                undergroundBlockRadius: UNDERGROUND_RADIUS);

            var zone = ZoneConstants.GetZoneForY((int)MathF.Floor(_player.Position.Y));
            if (zone != _currentZone)
            {
                _currentZone = zone;
                Console.WriteLine($"Entered zone: {_currentZone}");
            }

            _selectedBlock = TryPickBlock(_camera.Position, _camera.Front, PICK_DISTANCE);

            if (Mouse.GetState().LeftButton == ButtonState.Pressed &&
                _selectedBlock is Vector3Int hit)
            {
                _worldMgr.World.SetBlockAt(hit, 0);
            }

            _solidFx.View       = _camera.GetViewMatrix();
            _solidFx.Projection = _camera.GetProjectionMatrix(GraphicsDevice.Viewport.AspectRatio);
            _lineFx.View        = _solidFx.View;
            _lineFx.Projection  = _solidFx.Projection;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var renderers = _renderCache.SyncAndGetRenderers(
                _worldMgr.LoadedChunks,
                _worldMgr.World);

            // 1) Voxel-Geometrie mit Texturen
            foreach (var pass in _solidFx.CurrentTechnique.Passes)
            {
                _solidFx.World = Matrix.Identity;
                pass.Apply();
                foreach (var rc in renderers)
                    rc.Draw(GraphicsDevice, _solidFx);
            }

            // 2) Draht-Würfel um selektierten Block
            if (_selectedBlock is Vector3Int sel)
                DrawWireCube(sel, Color.Yellow);

            // 3) Crosshair
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            const int len = 6;
            _spriteBatch.Draw(_crossTex,
                new Rectangle((int)_crossPos.X - len, (int)_crossPos.Y, len*2+1, 1),
                Color.White);
            _spriteBatch.Draw(_crossTex,
                new Rectangle((int)_crossPos.X, (int)_crossPos.Y - len, 1, len*2+1),
                Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private Vector3Int? TryPickBlock(Vector3 origin, Vector3 dir, float maxDist)
        {
            const float step = 0.1f;
            for (float d = 0; d < maxDist; d += step)
            {
                Vector3 p = origin + dir * d;
                var ip = new Vector3Int(
                    (int)MathF.Floor(p.X),
                    (int)MathF.Floor(p.Y),
                    (int)MathF.Floor(p.Z));
                if (_worldMgr.World.GetBlockAt(ip) != 0)
                    return ip;
            }
            return null;
        }

        private void DrawWireCube(in Vector3Int pos, Color col)
        {
            Vector3 p = pos.ToVector3();
            Vector3 q = p + Vector3.One;

            _cubeWire[ 0] = new VertexPositionColor(p, col);
            _cubeWire[ 1] = new VertexPositionColor(new Vector3(q.X, p.Y, p.Z), col);
            _cubeWire[ 2] = new VertexPositionColor(p, col);
            _cubeWire[ 3] = new VertexPositionColor(new Vector3(p.X, q.Y, p.Z), col);
            _cubeWire[ 4] = new VertexPositionColor(p, col);
            _cubeWire[ 5] = new VertexPositionColor(new Vector3(p.X, p.Y, q.Z), col);
            _cubeWire[ 6] = new VertexPositionColor(q, col);
            _cubeWire[ 7] = new VertexPositionColor(new Vector3(p.X, q.Y, q.Z), col);
            _cubeWire[ 8] = new VertexPositionColor(q, col);
            _cubeWire[ 9] = new VertexPositionColor(new Vector3(q.X, p.Y, q.Z), col);
            _cubeWire[10] = new VertexPositionColor(q, col);
            _cubeWire[11] = new VertexPositionColor(new Vector3(q.X, q.Y, p.Z), col);
            _cubeWire[12] = new VertexPositionColor(new Vector3(p.X, q.Y, p.Z), col);
            _cubeWire[13] = new VertexPositionColor(new Vector3(p.X, q.Y, q.Z), col);
            _cubeWire[14] = new VertexPositionColor(new Vector3(p.X, q.Y, p.Z), col);
            _cubeWire[15] = new VertexPositionColor(new Vector3(q.X, q.Y, p.Z), col);
            _cubeWire[16] = new VertexPositionColor(new Vector3(q.X, p.Y, p.Z), col);
            _cubeWire[17] = new VertexPositionColor(new Vector3(q.X, q.Y, p.Z), col);
            _cubeWire[18] = new VertexPositionColor(new Vector3(q.X, p.Y, p.Z), col);
            _cubeWire[19] = new VertexPositionColor(new Vector3(q.X, p.Y, q.Z), col);
            _cubeWire[20] = new VertexPositionColor(new Vector3(p.X, p.Y, q.Z), col);
            _cubeWire[21] = new VertexPositionColor(new Vector3(q.X, p.Y, q.Z), col);
            _cubeWire[22] = new VertexPositionColor(new Vector3(p.X, p.Y, q.Z), col);
            _cubeWire[23] = new VertexPositionColor(new Vector3(p.X, q.Y, q.Z), col);

            _lineFx.World = Matrix.Identity;
            foreach (var pass in _lineFx.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.LineList,
                    _cubeWire,
                    0,
                    primitiveCount: 12);
            }
        }
    }
}
