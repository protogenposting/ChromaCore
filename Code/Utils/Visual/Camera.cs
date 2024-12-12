using ChromaCore.Code.Objects;
using ChromaCore.Code.Scenes;

namespace ChromaCore.Code.Utils.Visual
{
    public class Camera
    {
        public Matrix viewMatrix;

        public Vector2 target;
        public Vector2 position;
        private Vector2 overridePosition;
        public void ResetOverridePosition() => overridePosition = Vector2.Zero;

        public int CamWidth => (int)(1920 / zoom);
        public int CamHeight => (int)(1080 / zoom);

        public float zoom = 1f;
        private float overrideZoom = 0;
        public float normalZoom;

        private float shakeIntensity;
        private int shakeDuration;

        public Camera()
        {
            target = Vector2.Zero;
        }

        public void Update(bool instant = false)
        {
            Game game = Game.Instance;
            if (game.Scene is InGame scene)
            {
                List<GameObject> targets = new List<GameObject>(scene.players);

                float avgDist = 0;
                if (targets.Count == 0)
                {
                    target = Vector2.Zero;
                }
                else if (targets.Count == 1)
                {
                    target = targets[0].position;
                }
                else
                {
                    target = Vector2.Zero;
                    foreach (GameObject o in targets)
                    {
                        if (o != null) target += o.position;
                    }
                    target /= targets.Count;
                    foreach (GameObject o in targets)
                    {
                        if (o != null) avgDist += Math.Abs(target.X - o.position.X);
                    }
                    avgDist /= targets.Count;
                }

                zoom = (avgDist + 200) / 1200;
                zoom = 1 / zoom;

                zoom = Math.Clamp(zoom, 2.8f, 3f);

                normalZoom = zoom;

                if (overridePosition != Vector2.Zero)
                {
                    target = overridePosition;
                    overridePosition = Vector2.Zero;
                }
                if (overrideZoom != 0)
                {
                    zoom = overrideZoom;
                    overrideZoom = 0;
                }
                target.Y -= CamHeight / 12;

                try
                {
                    target.X = (int)Math.Clamp(target.X, scene.room.bounds.Left + (CamWidth / 2) + 4, scene.room.bounds.Right - (CamWidth / 2) - 4);
                }
                catch
                {
                    target.X = scene.room.bounds.Center.X;
                }
                target.Y = Math.Max(target.Y, scene.room.bounds.Top + CamHeight);
                target.Y = Math.Min(target.Y, scene.room.bounds.Bottom - (CamHeight / 2));

                if (instant) position = target - (new Vector2(CamWidth, CamHeight) / 2);
                else position = Vector2.Lerp(position, target - (new Vector2(CamWidth, CamHeight) / 2), 0.6f);

                if (shakeDuration > 0)
                {
                    position.Y += (float)Math.Sin(shakeDuration * Math.PI / 2) * shakeIntensity;
                    shakeDuration--;
                    shakeIntensity -= 0.25f;
                }
                else shakeIntensity = 0;
            }
            else
            {
                position = Vector2.Zero;
                zoom = 1;
            }

            viewMatrix = Matrix.CreateTranslation(new Vector3(-position, 0)) * Matrix.CreateScale(zoom, zoom, 1);
        }

        public void OverridePosition(Vector2 position)
        {
            overridePosition = position;
        }

        public void OverrideZoom(float zoom)
        {
            overrideZoom = zoom;
        }

        public void ShakeCamera(float intensity, int duration)
        {
            shakeIntensity = Math.Max(shakeIntensity, intensity);
            shakeDuration = duration;
        }
    }
}
