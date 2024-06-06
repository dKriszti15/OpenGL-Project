using Silk.NET.OpenGL;
using StbImageSharp;

namespace Projekt
{
    internal class GlObject
    {
        public uint Vao { get; }
        public uint Vertices { get; }
        public uint Colors { get; }
        public uint Indices { get; }
        public uint IndexArrayLength { get; }

        public float maxY;
        public uint Texture {  get; }

        public float PositionX { get; private set; } = 6.25f;
        public float PositionY { get; private set; } = 0f;
        public float PositionZ { get; private set; } = 6.25f;

        private GL Gl;

        public GlObject(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, GL gl, uint texture = 0, float maxY = 0)
        {
            this.Vao = vao;
            this.Vertices = vertices;
            this.Colors = colors;
            this.Indices = indeces;
            this.IndexArrayLength = indexArrayLength;
            this.Gl = gl;
            this.Texture = texture;
            this.maxY = maxY;
        }

        internal void ReleaseGlObject()
        {
            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(Vertices);
            Gl.DeleteBuffer(Colors);
            Gl.DeleteBuffer(Indices);
            Gl.DeleteVertexArray(Vao);
        }

        public void MoveForward(float stepSize)
        {
            PositionZ -= stepSize;
        }

        public void MoveBackward(float stepSize)
        {
            PositionZ += stepSize;
        }

        public void MoveLeft(float stepSize)
        {
            PositionX -= stepSize;
        }

        public void MoveRight(float stepSize)
        {
            PositionX += stepSize;
        }

        public static unsafe ImageResult ReadTextureImage(string textureResource)
        {
            ImageResult result;
            using (Stream skyeboxStream
                = typeof(GlCube).Assembly.GetManifestResourceStream("Projekt.Resources." + textureResource))
                result = ImageResult.FromStream(skyeboxStream, ColorComponents.RedGreenBlueAlpha);

            return result;
        }
    }
}
