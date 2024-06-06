using Projekt;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Globalization;

namespace Projekt
{
    internal class ObjResourceReader
    {
        public static unsafe GlObject CreateCharacterWithColor(GL Gl, float[] faceColor)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<string[]> objFaces;
            List<float[]> objNormals;

            float maxY = ReadObjDataForCharacter(out objVertices, out objFaces, out objNormals);
            
            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(faceColor, objVertices, objFaces, objNormals, glVertices, glColors, glIndices);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices, maxY);
        }

        public static unsafe GlObject CreateFlowerWithColor(GL Gl, float[] faceColor)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<string[]> objFaces;
            List<float[]> objNormals;
            List<float[]> objTextures;
            ReadObjDataForFlower(out objVertices, out objFaces, out objNormals, out objTextures);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArraysFlower(faceColor, objVertices, objFaces, objNormals, objTextures, glVertices, glColors, glIndices);

            return CreateOpenGlObjectFlower(Gl, vao, glVertices, glColors, glIndices);
        }

        public static unsafe GlObject CreateAxeWithColor(GL Gl, float[] faceColor)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<string[]> objFaces;
            List<float[]> objNormals;
            List<float[]> objTextures;
            ReadObjDataForAxe(out objVertices, out objFaces, out objNormals, out objTextures);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArraysAxe(faceColor, objVertices, objFaces, objNormals, objTextures, glVertices, glColors, glIndices);

            return CreateOpenGlObjectAxe(Gl, vao, glVertices, glColors, glIndices);
        }

        private static unsafe GlObject CreateOpenGlObject(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices, float maxY = 0)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetNormal + (3 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glColors.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.Count;

            return new GlObject(vao, vertices, colors, indices, indexArrayLength, Gl, 0, maxY);
        }

        private static unsafe GlObject CreateOpenGlObjectFlower(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint offsetTexture = offsetNormal + (3 * sizeof(float));
            uint vertexSize = offsetTexture + (2 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            //Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            //Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            //Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            //Gl.EnableVertexAttribArray(1);

            // set texture
            // create texture
            uint texture = Gl.GenTexture();
            // activate texture 0
            Gl.ActiveTexture(TextureUnit.Texture0);
            // bind texture
            Gl.BindTexture(TextureTarget.Texture2D, texture);

            var skyboxImageResult = GlObject.ReadTextureImage("roseTexture.jpg");
            var textureBytes = (ReadOnlySpan<byte>)skyboxImageResult.Data.AsSpan();
            // Here we use "result.Width" and "result.Height" to tell OpenGL about how big our texture is.
            Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)skyboxImageResult.Width,
                (uint)skyboxImageResult.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureBytes);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            // unbinde texture
            Gl.BindTexture(TextureTarget.Texture2D, 0);

            Gl.EnableVertexAttribArray(3);
            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetTexture);


            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.ToArray().Length;

            return new GlObject(vao, vertices, colors, indices, indexArrayLength, Gl, texture);
        }

        private static unsafe GlObject CreateOpenGlObjectAxe(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint offsetTexture = offsetNormal + (3 * sizeof(float));
            uint vertexSize = offsetTexture + (2 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glColors.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.Count;

            return new GlObject(vao, vertices, colors, indices, indexArrayLength, Gl, 0);
        }

        private static unsafe void CreateGlArraysFromObjArrays(float[] faceColor, List<float[]> objVertices, List<string[]> objFaces, List<float[]> objNormals, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();
            foreach (var objFace in objFaces)
            {

                // process 3 vertices
                for (int i = 0; i < objFace.Length; ++i)
                {
                    if (i != 3)
                    {
                        var parts = objFace[i].Split("//");
                        var objVertex = objVertices[int.Parse(parts[0]) - 1];
                        var normal = objNormals[int.Parse(parts[1]) - 1];

                        // create gl description of vertex
                        List<float> glVertex = new List<float>();
                        glVertex.AddRange(objVertex);
                        glVertex.AddRange(normal);
                        // add textrure, color

                        // check if vertex exists
                        var glVertexStringKey = string.Join(" ", glVertex);
                        if (!glVertexIndices.ContainsKey(glVertexStringKey))
                        {
                            glVertices.AddRange(glVertex);
                            glColors.AddRange(faceColor);
                            glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                        }

                        // add vertex to triangle indices
                        glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                    }
                }

                for (int i = 0; i < objFace.Length; ++i)
                {
                    if (i != 1)
                    {
                        var parts = objFace[i].Split("//");
                        var objVertex = objVertices[int.Parse(parts[0]) - 1];
                        var normal = objNormals[int.Parse(parts[1]) - 1];

                        // create gl description of vertex
                        List<float> glVertex = new List<float>();
                        glVertex.AddRange(objVertex);
                        glVertex.AddRange(normal);
                        // add textrure, color

                        // check if vertex exists
                        var glVertexStringKey = string.Join(" ", glVertex);
                        if (!glVertexIndices.ContainsKey(glVertexStringKey))
                        {
                            glVertices.AddRange(glVertex);
                            glColors.AddRange(faceColor);
                            glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                        }

                        // add vertex to triangle indices
                        glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                    }
                }


            }
        }

        private static unsafe void CreateGlArraysFromObjArraysFlower(float[] faceColor, List<float[]> objVertices, List<string[]> objFaces, List<float[]> objNormals, List<float[]> objTextures, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            foreach (var objFace in objFaces)
            {

                // process 3 vertices
                for (int i = 0; i < objFace.Length; ++i)
                {
                    if (i != 3)
                    {
                        var parts = objFace[i].Split("/");
                        var objVertex = objVertices[int.Parse(parts[0]) - 1];
                        var texture = objTextures[int.Parse(parts[1]) - 1];
                        var normal = objNormals[int.Parse(parts[2]) - 1];

                        // create gl description of vertex
                        List<float> glVertex = new List<float>();
                        glVertex.AddRange(objVertex);
                        glVertex.AddRange(texture);
                        glVertex.AddRange(normal);
                        // add textrure, color

                        // check if vertex exists
                        var glVertexStringKey = string.Join(" ", glVertex);
                        if (!glVertexIndices.ContainsKey(glVertexStringKey))
                        {
                            glVertices.AddRange(glVertex);
                            glColors.AddRange(faceColor);
                            glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                        }

                        // add vertex to triangle indices
                        glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                    }
                }

                for (int i = 0; i < objFace.Length; ++i)
                {
                    if (i != 1)
                    {
                        var parts = objFace[i].Split("/");
                        var objVertex = objVertices[int.Parse(parts[0]) - 1];
                        var texture = objTextures[int.Parse(parts[1]) - 1];
                        var normal = objNormals[int.Parse(parts[2]) - 1];

                        // create gl description of vertex
                        List<float> glVertex = new List<float>();
                        glVertex.AddRange(objVertex);
                        glVertex.AddRange(texture);
                        glVertex.AddRange(normal);
                        // add textrure, color

                        // check if vertex exists
                        var glVertexStringKey = string.Join(" ", glVertex);
                        if (!glVertexIndices.ContainsKey(glVertexStringKey))
                        {
                            glVertices.AddRange(glVertex);
                            glColors.AddRange(faceColor);
                            glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                        }

                        // add vertex to triangle indices
                        glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                    }
                }


            }
        }

        private static unsafe void CreateGlArraysFromObjArraysAxe(float[] faceColor, List<float[]> objVertices, List<string[]> objFaces, List<float[]> objNormals, List<float[]> objTextures, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            foreach (var objFace in objFaces)
            {

                // process 3 vertices
                for (int i = 0; i < objFace.Length; ++i)
                {
                    if (i != 3)
                    {
                        var parts = objFace[i].Split("/");
                        var objVertex = objVertices[int.Parse(parts[0]) - 1];
                        var texture = objTextures[int.Parse(parts[1]) - 1];
                        var normal = objNormals[int.Parse(parts[2]) - 1];

                        // create gl description of vertex
                        List<float> glVertex = new List<float>();
                        glVertex.AddRange(objVertex);
                        glVertex.AddRange(texture);
                        glVertex.AddRange(normal);
                        // add textrure, color

                        // check if vertex exists
                        var glVertexStringKey = string.Join(" ", glVertex);
                        if (!glVertexIndices.ContainsKey(glVertexStringKey))
                        {
                            glVertices.AddRange(glVertex);
                            glColors.AddRange(faceColor);
                            glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                        }

                        // add vertex to triangle indices
                        glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                    }
                }

                for (int i = 0; i < objFace.Length; ++i)
                {
                    if (i != 1)
                    {
                        var parts = objFace[i].Split("/");
                        var objVertex = objVertices[int.Parse(parts[0]) - 1];
                        var texture = objTextures[int.Parse(parts[1]) - 1];
                        var normal = objNormals[int.Parse(parts[2]) - 1];

                        // create gl description of vertex
                        List<float> glVertex = new List<float>();
                        glVertex.AddRange(objVertex);
                        glVertex.AddRange(texture);
                        glVertex.AddRange(normal);
                        // add textrure, color

                        // check if vertex exists
                        var glVertexStringKey = string.Join(" ", glVertex);
                        if (!glVertexIndices.ContainsKey(glVertexStringKey))
                        {
                            glVertices.AddRange(glVertex);
                            glColors.AddRange(faceColor);
                            glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                        }

                        // add vertex to triangle indices
                        glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                    }
                }


            }
        }

        private static unsafe float ReadObjDataForCharacter(out List<float[]> objVertices, out List<string[]> objFaces, out List<float[]> objNormals)
        {
            objVertices = new List<float[]>();
            objFaces = new List<string[]>();
            objNormals = new List<float[]>();
            float maxY = -999f;
            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("Projekt.Resources.FinalBaseMesh.obj"))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;

                    var lineClassifier = line.Substring(0, line.IndexOf(' '));
                    var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                    switch (lineClassifier)
                    {
                        case "v":
                            float[] vertex = new float[3];
                            
                            for (int i = 0; i < vertex.Length; ++i)
                                vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objVertices.Add(vertex);

                            if (vertex[1] > maxY)
                            {
                                maxY = vertex[1];
                            }

                            break;
                        case "f":
                            string[] face = new string[4];
                            for (int i = 0; i < face.Length; ++i)
                            {
                                face[i] = lineData[i];
                            }
                            objFaces.Add(face);
                            break;
                        case "vn":
                            float[] normal = new float[3];
                            for (int i = 0; i < normal.Length; ++i)
                                normal[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objNormals.Add(normal);
                            break;
                    }
                }
            }
            return maxY;
        }

        private static unsafe void ReadObjDataForFlower(out List<float[]> objVertices, out List<string[]> objFaces, out List<float[]> objNormals, out List<float[]> objTextures)
        {
            objVertices = new List<float[]>();
            objFaces = new List<string[]>();
            objNormals = new List<float[]>();
            objTextures = new List<float[]>();

            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("Projekt.Resources.rose.obj"))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;

                    var lineClassifier = line.Substring(0, line.IndexOf(' '));
                    var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                    switch (lineClassifier)
                    {
                        case "v":
                            float[] vertex = new float[3];
                            for (int i = 0; i < vertex.Length; ++i)
                                vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objVertices.Add(vertex);
                            break;
                        case "f":
                            string[] face = new string[4];
                            for (int i = 0; i < face.Length; ++i)
                            {
                                face[i] = lineData[i];
                            }
                            objFaces.Add(face);
                            break;
                        case "vn":
                            float[] normal = new float[3];
                            for (int i = 0; i < normal.Length; ++i)
                                normal[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objNormals.Add(normal);
                            break;
                        case "vt":
                            float[] texture = new float[2];
                            for (int i = 0; i < texture.Length; ++i)
                                texture[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objTextures.Add(texture);
                            break;
                    }
                }
            }
        }

        private static unsafe void ReadObjDataForAxe(out List<float[]> objVertices, out List<string[]> objFaces, out List<float[]> objNormals, out List<float[]> objTextures)
        {
            objVertices = new List<float[]>();
            objFaces = new List<string[]>();
            objNormals = new List<float[]>();
            objTextures = new List<float[]>();

            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("Projekt.Resources.untitled.obj"))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;

                    var lineClassifier = line.Substring(0, line.IndexOf(' '));
                    var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                    switch (lineClassifier)
                    {
                        case "v":
                            float[] vertex = new float[3];
                            for (int i = 0; i < vertex.Length; ++i)
                                vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objVertices.Add(vertex);
                            break;
                        case "f":
                            string[] face = new string[4];
                            for (int i = 0; i < face.Length; ++i)
                            {
                                face[i] = lineData[i];
                            }
                            objFaces.Add(face);
                            break;
                        case "vn":
                            float[] normal = new float[3];
                            for (int i = 0; i < normal.Length; ++i)
                                normal[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objNormals.Add(normal);
                            break;
                        case "vt":
                            float[] texture = new float[2];
                            for (int i = 0; i < texture.Length; ++i)
                                texture[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objTextures.Add(texture);
                            break;
                    }
                }
            }
        }
    }
}
