using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using HyatlasGame.Core.Shared.Utils;
using HyatlasGame.Core.World;

namespace HyatlasGame.Core.Entities.Player
{
    /// <summary>
    /// First‑Person‑Spieler: Eingabe + Schwerkraft + Zylinder‑Kollision.
    /// Aktualisiert die Kamera (Augenhöhe).
    /// Doppeltipp Leertaste → Flugmodus.
    /// </summary>
    public class PlayerController
    {
        /* --- öffentliche Status‑Properties --- */
        public Vector3 Position   { get; private set; }
        public Vector3 Velocity   { get; private set; }
        public bool    IsGrounded { get; private set; }

        /* --- Konstanten --- */
        private const float Gravity   = 30f;
        private const float MoveSpeed = 6f;
        private const float JumpSpeed = 9f;

        private const float Height = 1.8f;
        private const float Radius = 0.35f;
        private const float Skin   = 0.001f;

        // Double‑Tap‑Threshold in Sekunden
        private const double DoubleTapThreshold = 0.3;

        /* --- Flugmodus‑Status --- */
        private bool _flightMode       = false;
        private double _lastSpaceTap   = -1.0;
        private bool   _prevSpaceDown  = false;

        /* --- Abhängigkeiten --- */
        private readonly WorldManager     _world;
        private readonly CameraController _cam;

        public PlayerController(WorldManager worldMgr, CameraController cam, Vector3 spawnPos)
        {
            _world = worldMgr ?? throw new ArgumentNullException(nameof(worldMgr));
            _cam   = cam      ?? throw new ArgumentNullException(nameof(cam));

            Position = spawnPos + Vector3.Up * (Height * 0.5f); // Mittelpunkt
        }

        public void Update(GameTime gt, Game game)
        {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            var kb      = Keyboard.GetState();
            bool spaceDown = kb.IsKeyDown(Keys.Space);

            // --- Double‑Tap Leertaste für Flugmodus ---
            if (spaceDown && !_prevSpaceDown)
            {
                double now = gt.TotalGameTime.TotalSeconds;
                if (_lastSpaceTap > 0 && now - _lastSpaceTap <= DoubleTapThreshold)
                {
                    _flightMode = !_flightMode;
                }
                _lastSpaceTap = now;
            }
            _prevSpaceDown = spaceDown;

            /* 1) Eingabe → Wunschgeschwindigkeit (horizontal) */
            Vector3 wish = Vector3.Zero;
            Vector3 fwd   = Vector3.Normalize(new Vector3(_cam.Front.X, 0, _cam.Front.Z));
            Vector3 right = Vector3.Normalize(Vector3.Cross(fwd, Vector3.Up));

            if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up))    wish += fwd;
            if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down))  wish -= fwd;
            if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left))  wish -= right;
            if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right)) wish += right;
            if (wish != Vector3.Zero) wish = Vector3.Normalize(wish) * MoveSpeed;

            // --- Flugmodus: einfache Bewegung in alle Richtungen ---
            if (_flightMode)
            {
                // vertikale Eingabe im Flug
                float yWish = 0f;
                if (kb.IsKeyDown(Keys.Space))           yWish = MoveSpeed;
                if (kb.IsKeyDown(Keys.LeftControl))     yWish = -MoveSpeed;

                Vector3 vel = new Vector3(wish.X, yWish, wish.Z);
                Vector3 proposed = Position + vel * dt;

                // nur horizontale Kollision
                ResolveAxis(ref proposed, ref vel.X, Radius, Axis.X);
                ResolveAxis(ref proposed, ref vel.Z, Radius, Axis.Z);

                // Status übernehmen
                Position = proposed;
                Velocity = vel;
                _cam.Position = Position + Vector3.Up * (Height * 0.5f);
                return;
            }

            /* 2) vertikale Geschwindigkeit (Jump / Gravity) */
            if (IsGrounded && spaceDown)
                Velocity = new Vector3(Velocity.X, JumpSpeed, Velocity.Z);

            Vector3 combinedVel = new Vector3(
                wish.X,
                Velocity.Y - Gravity * dt,
                wish.Z);

            /* 3) Kollisions‑Sweep */
            Vector3 proposedPos = Position + combinedVel * dt;

            ResolveAxis(ref proposedPos, ref combinedVel.X, Radius, Axis.X);
            ResolveAxis(ref proposedPos, ref combinedVel.Z, Radius, Axis.Z);

            IsGrounded = ResolveVertical(ref proposedPos, ref combinedVel.Y);

            /* 4) Status übernehmen & Kamera setzen */
            Position = proposedPos;
            Velocity = combinedVel;
            _cam.Position = Position + Vector3.Up * (Height * 0.5f);
        }

        /* ---------- Kollision‑Implementation ---------- */

        private enum Axis { X, Z }

        private void ResolveAxis(ref Vector3 pos, ref float vComp, float radius, Axis axis)
        {
            if (Math.Abs(vComp) < 1e-5f) return;

            float dir = MathF.Sign(vComp);
            float travel = vComp * (1f / 60f);
            float targetCoord = (axis == Axis.X ? pos.X : pos.Z) + travel;

            int cur = (int)MathF.Floor((axis == Axis.X ? pos.X : pos.Z) + dir * radius);
            int end = (int)MathF.Floor(targetCoord + dir * radius);

            while (cur != end)
            {
                int bx = axis == Axis.X ? end : (int)MathF.Floor(pos.X);
                int bz = axis == Axis.Z ? end : (int)MathF.Floor(pos.Z);

                float[] ySamples = { pos.Y - Height * 0.5f + Skin, pos.Y + Height * 0.5f - Skin };
                foreach (float y in ySamples)
                {
                    int by = (int)MathF.Floor(y);
                    if (_world.IsSolidBlockAt(new Vector3Int(bx, by, bz)))
                    {
                        if (axis == Axis.X)
                            pos.X = bx + (dir < 0 ? 1 + radius : -radius) + -dir * Skin;
                        else
                            pos.Z = bz + (dir < 0 ? 1 + radius : -radius) + -dir * Skin;

                        vComp = 0;
                        return;
                    }
                }
                cur -= (int)dir;
            }

            if (axis == Axis.X) pos.X = targetCoord;
            else                pos.Z = targetCoord;
        }

        private bool ResolveVertical(ref Vector3 pos, ref float vy)
        {
            if (vy >= 0)
            {
                pos.Y += vy * (1f / 60f);
                return false;
            }

            float proposedY = pos.Y + vy * (1f / 60f);
            float foot = proposedY - Height * 0.5f;
            int by = (int)MathF.Floor(foot);

            (float dx, float dz)[] corners = {
                (-Radius, -Radius), (-Radius,  Radius),
                ( Radius, -Radius), ( Radius,  Radius)
            };

            foreach (var c in corners)
            {
                int bx = (int)MathF.Floor(pos.X + c.dx);
                int bz = (int)MathF.Floor(pos.Z + c.dz);

                if (_world.IsSolidBlockAt(new Vector3Int(bx, by, bz)))
                {
                    pos.Y = by + 1f + Height * 0.5f + Skin;
                    vy = 0;
                    return true;
                }
            }

            pos.Y = proposedY;
            return false;
        }
    }
}