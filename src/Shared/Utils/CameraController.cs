using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HyatlasGame.Core.Shared.Utils
{
    /// <summary>
    /// First‑Person‑Kamera mit Maus‑Look und (optional) WASD‑Translation.
    /// ESC toggelt den Mauslock ein/aus.
    /// </summary>
    public class CameraController
    {
        /* ----------------- öffentliche Properties ----------------- */
        public Vector3 Position { get; set; }
        public Vector3 Front    { get; private set; }
        public Vector3 Up       => Vector3.Up;
        public Vector3 Right    => Vector3.Normalize(Vector3.Cross(Front, Up));

        /// <summary>Wenn <c>false</c>, wird keine Tastatur‑Translation durchgeführt.</summary>
        public bool AllowTranslation { get; set; } = true;
        public bool Enabled          => _enabled;

        /* ---------------------- Felder & Settings ------------------ */
        private bool  _enabled      = true;
        private bool  _prevEscDown  = false;
        private float _yaw;
        private float _pitch;

        public float MoveSpeed        { get; set; } = 10f;  // Blöcke / s
        public float MouseSensitivity { get; set; } = 0.15f; // Grad / Pixel

        /* ------------------------- ctor ---------------------------- */
        public CameraController(Vector3 startPosition,
                                float   startYaw   = -90f,
                                float   startPitch =   0f)
        {
            Position = startPosition;
            _yaw     = startYaw;
            _pitch   = startPitch;
            Front    = ComputeFront();
        }

        /* -------------------- Initialisierung ---------------------- */
        public void InitializeMouse(Game game)
        {
            var c = game.Window.ClientBounds.Center;
            Mouse.SetPosition(c.X, c.Y);
        }

        /* ----------------------- pro Frame ------------------------- */
        public void Update(GameTime gameTime, Game game)
        {
            /* ---------- 0) ESC → Lock/Unlock Cursor ---------- */
            var kb      = Keyboard.GetState();
            bool escNow = kb.IsKeyDown(Keys.Escape);

            if (escNow && !_prevEscDown)
            {
                _enabled            = !_enabled;
                game.IsMouseVisible = !_enabled;
                if (_enabled)
                {
                    var cen = game.Window.ClientBounds.Center;
                    Mouse.SetPosition(cen.X, cen.Y);
                }
            }
            _prevEscDown = escNow;

            if (!_enabled) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            /* ---------- 1) Optionale Tastatur‑Movement ---------- */
            if (AllowTranslation)
            {
                Vector3 dir = Vector3.Zero;
                if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up))    dir += Front;
                if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down))  dir -= Front;
                if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left))  dir -= Right;
                if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right)) dir += Right;

                if (dir != Vector3.Zero)
                    Position += Vector3.Normalize(dir) * MoveSpeed * dt;
            }

            /* ---------- 2) Maus‑Look ---------- */
            var bounds = game.Window.ClientBounds;
            int cx = bounds.Width / 2, cy = bounds.Height / 2;
            var ms = Mouse.GetState();
            float dx = ms.X - cx, dy = ms.Y - cy;

            _yaw   += dx * MouseSensitivity;
            _pitch -= dy * MouseSensitivity;
            _pitch  = MathHelper.Clamp(_pitch, -89f, 89f);
            Front   = ComputeFront();

            /* ---------- 3) Cursor zurücksetzen ---------- */
            Mouse.SetPosition(cx, cy);
        }

        /* ---------------------- Hilfsfunktionen -------------------- */
        private Vector3 ComputeFront()
        {
            float yawRad   = MathHelper.ToRadians(_yaw);
            float pitchRad = MathHelper.ToRadians(_pitch);
            return Vector3.Normalize(new Vector3(
                (float)Math.Cos(yawRad) * (float)Math.Cos(pitchRad),
                (float)Math.Sin(pitchRad),
                (float)Math.Sin(yawRad) * (float)Math.Cos(pitchRad)
            ));
        }

        public Matrix GetViewMatrix()
            => Matrix.CreateLookAt(Position, Position + Front, Up);

        public Matrix GetProjectionMatrix(float aspectRatio)
            => Matrix.CreatePerspectiveFieldOfView(
                   MathHelper.PiOver4,
                   aspectRatio,
                   0.1f, 1000f);
    }
}
