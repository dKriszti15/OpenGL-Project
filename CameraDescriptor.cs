using Silk.NET.Maths;

namespace Projekt
{
    internal class CameraDescriptor
    {
        private static float DistanceToOrigin = 50;

        private static float AngleToZYPlane = 200f;

        private static float AngleToZXPlane = 0.5f;

        private static Vector3D<float> FPPPosition;

        private static Vector3D<float> FPPTarget;
        /// <summary>
        /// Gets the position of the camera.
        /// </summary>
        public Vector3D<float> Position
        {
            get;
            set;
        } = GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane);

        public float getDistanceToOrigin()
        {
            return DistanceToOrigin;
        }

        public void SetFPPPosition(float X, float Y, float Z)
        {
            FPPPosition = new Vector3D<float>(X, Y, Z);

            Target = new Vector3D<float>(0f,5f,100f);
            
            Position = FPPPosition;
        }

        public void SetPosition()
        {
            Position = new Vector3D<float>(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane);
        }

        public void SetPosition(float x, float y, float z)
        {
            Position = new Vector3D<float>(x, y, z);
        }

        public void SetPositionTPP(float X, float Y, float Z)
        {
            Position = new Vector3D<float>(X, Y, Z);
        }

        public void SetFPPTarget(float X, float Y, float Z)
        {
            
            FPPTarget = new Vector3D<float>(X,Y,Z);
            Target = FPPTarget;

        }



        /// <summary>
        /// Gets the up vector of the camera.
        /// </summary>
        public Vector3D<float> UpVector
        {
            get
            {
                return new Vector3D<float>(0f,1f,0f);
            }
        }

        /// <summary>
        /// Gets the target point of the camera view.
        /// </summary>
        public Vector3D<float> Target
        {
            get;
            set;
        } = Vector3D<float>.Zero;


        private static Vector3D<float> GetPointFromAngles(float distanceToOrigin, float angleToMinZYPlane, float angleToMinZXPlane)
        {
            var x = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Sin(angleToMinZYPlane);
            var z = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Cos(angleToMinZYPlane);
            var y = distanceToOrigin * Math.Sin(angleToMinZXPlane);

            return new Vector3D<float>((float)x, (float)y, (float)z);
        }
    }
}
