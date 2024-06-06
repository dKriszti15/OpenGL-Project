using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Projekt
{
    internal static class Program
    {
        private static CameraDescriptor cameraDescriptor = new();

        private static CubeArrangementModel cubeArrangementModel = new();

        private static IWindow window;

        private static IInputContext inputContext;

        private static GL Gl;

        private static ImGuiController controller;

        private static uint program;

        private static GlObject Character;

        private static GlObject flower;

        private static GlObject Axe;

        private static GlCube table;

        private static GlCube skyBox;

        private static Matrix4X4<float> rotateConformMovement = Matrix4X4.CreateRotationY((float)Math.PI);

        private static int[][] map;

        private static int[] wasRow, wasCol;

        private static int[] positionRow, positionCol;

        private static List<Tuple<int,int>> FlowersToHide;

        private static bool firstPerson = false;

        private static float Shininess = 150;

        private static int numberOfRemainingFlowers = 15;

        private static float maxY;

        private static float offsetAxe = 0f, offsetAxe2 = 0f;

        private static bool moveForwardAxe = true;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string TextureUniformVariableName = "uTexture";

        private const string LightColorVariableName = "lightColor";
        private const string LightPositionVariableName = "lightPos";
        private const string ViewPosVariableName = "viewPos";
        private const string ShininessVariableName = "shininess";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Projekt";
            windowOptions.Size = new Vector2D<int>(1000, 1000);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();
        }

        private static void Window_Load()
        {
            //Console.WriteLine("Load");
            Random random = new();
            map = new int[16][];
            wasRow = new int[16];
            wasCol = new int[16];
            positionCol = new int[16];
            positionRow = new int[16];
            FlowersToHide = new List<Tuple<int, int>>();
            for (int i = 0; i < 16; i++)
            {
                wasRow[i] = 0;
                wasCol[i] = 0;
                map[i] = new int[16];
                for (int j = 0; j < 16; j++)
                {
                    map[i][j] = 0;
                }
            }

            int row, col;
            
            for (int i = 0; i < 15; i++)
            {
                row = random.Next(16);
                col = random.Next(16);
                while (wasRow[row] == 1 && wasCol[col] == 1)
                {
                    row = random.Next(16);
                    col = random.Next(16);
                }
                wasCol[col] = 1;
                wasRow[row] = 1;
                map[row][col] = 1;

                positionRow[i] = row;
                positionCol[i] = col;
            }

            // set up input handling
            inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            Gl = window.CreateOpenGL();

            controller = new ImGuiController(Gl, window, inputContext);

            window.FramebufferResize += s =>
            {
                Gl.Viewport(s);
            };


            Gl.ClearColor(System.Drawing.Color.Black);

            SetUpObjects();

            LinkProgram();

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);

            //Console.WriteLine(maxY);
        }

        private static void LinkProgram()
        {
            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, ReadShader("VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, ReadShader("FragmentShader.frag"));
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
        }

        private static string ReadShader(string shaderFileName)
        {
            using (Stream shaderStream = typeof(Program).Assembly.GetManifestResourceStream("Projekt.Shaders." + shaderFileName))
            using (StreamReader shaderReader = new StreamReader(shaderStream))
                return shaderReader.ReadToEnd();
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            float stepSize = 12.5f;
            switch (key)
            {
                case Key.Left:
                    if (Character.PositionX >= -81.25f)
                    {
                        Character.MoveLeft(stepSize);
                        rotateConformMovement = Matrix4X4.CreateRotationY(-(float)Math.PI / 2);
                        if (firstPerson)
                        {
                            cameraDescriptor.SetFPPPosition(Character.PositionX - 5f, maxY, Character.PositionZ);
                            cameraDescriptor.SetFPPTarget(-100f, 0, Character.PositionZ);
                        }
                    }
                    break;
                case Key.Right:
                    if (Character.PositionX <= +81.25f)
                    {
                        Character.MoveRight(stepSize);
                        rotateConformMovement = Matrix4X4.CreateRotationY((float)Math.PI / 2);
                        if (firstPerson)
                        {
                            cameraDescriptor.SetFPPPosition(Character.PositionX + 5f, maxY, Character.PositionZ);
                            cameraDescriptor.SetFPPTarget(+100f, 0, Character.PositionZ);
                        }
                    }
                    break;
                case Key.Down:
                    if (Character.PositionZ <= +81.25f)
                    {
                        Character.MoveBackward(stepSize);
                        rotateConformMovement = Matrix4X4.CreateRotationY(0f);
                        if (firstPerson)
                        {
                            cameraDescriptor.SetFPPPosition(Character.PositionX, maxY, Character.PositionZ + 5f);
                            cameraDescriptor.SetFPPTarget(Character.PositionX, 0, 100f);
                        }
                    }
                    break;
                case Key.Up:
                    if (Character.PositionZ >= -81.25f)
                    {
                        Character.MoveForward(stepSize);
                        rotateConformMovement = Matrix4X4.CreateRotationY((float)Math.PI);
                        if (firstPerson)
                        {
                            cameraDescriptor.SetFPPPosition(Character.PositionX, maxY, Character.PositionZ - 5f);
                            cameraDescriptor.SetFPPTarget(Character.PositionX, 0, -100f);
                        }
                    }
                    break;
                case Key.Space:
                    cubeArrangementModel.AnimationEnabeld = !cubeArrangementModel.AnimationEnabeld;
                    break;
            }
            if (map[(int)((Character.PositionX + 93.75f) / 12.5f)][(int)((Character.PositionZ + 93.75f) / 12.5f)] == 1)
            {
                FlowersToHide.Add(Tuple.Create((int)((Character.PositionX + 93.75f) / 12.5f), (int)((Character.PositionZ + 93.75f) / 12.5f)));
                numberOfRemainingFlowers--;

                if (numberOfRemainingFlowers == 0)
                {
                    Console.WriteLine("You collected all the flowers. YOU WON!");
                    Environment.Exit(0);
                }

                Console.WriteLine("Remaining flowers: " + numberOfRemainingFlowers);
                map[(int)((Character.PositionX + 93.75f) / 12.5f)][(int)((Character.PositionZ + 93.75f) / 12.5f)] = 0;
            }
            
        }

        private static void Window_Update(double deltaTime)
        {
            //Console.WriteLine($"Update after {deltaTime} [s].");
            // multithreaded
            // make sure it is threadsafe
            // NO GL calls
            cubeArrangementModel.AdvanceTime(deltaTime);

            controller.Update((float)deltaTime);
        }

        private static unsafe void Window_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            SetViewMatrix();
            SetProjectionMatrix();

            SetLightColor();
            SetLightPosition();
            SetViewerPosition();
            SetShininess();

            DrawCharacter(rotateConformMovement);

            DrawPulsingFlowers();

            DrawHorizontalMovingAxe();

            DrawHorizontalMovingAxe2();

            DrawSkyBox();

            ImGuiNET.ImGui.Begin("POV" , ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove);

            if (ImGui.Button("FPP"))
            {
                firstPerson = true;
                cameraDescriptor.SetFPPPosition(Character.PositionX, maxY + 5f, Character.PositionZ);
                Console.WriteLine("POV switched to FPP");
                
            }

            if (ImGui.Button("TPP"))
            {
                firstPerson = false;
                cameraDescriptor.SetPosition(0f, 200f,200f);
                Console.WriteLine("POV switched to TPP");
            }

            ImGuiNET.ImGui.End();

            controller.Render();
        }

        private static unsafe void DrawSkyBox()
        {
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(400f);
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(skyBox.Vao);

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, skyBox.Texture.Value);

            Gl.DrawElements(GLEnum.Triangles, skyBox.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void SetLightColor()
        {
            int location = Gl.GetUniformLocation(program, LightColorVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightColorVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 1f, 1f, 1f);
            CheckError();
        }

        private static unsafe void SetLightPosition()
        {
            int location = Gl.GetUniformLocation(program, LightPositionVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightPositionVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 0f, 10f, 0f);
            CheckError();
        }

        private static unsafe void SetViewerPosition()
        {
            int location = Gl.GetUniformLocation(program, ViewPosVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewPosVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, cameraDescriptor.Position.X, cameraDescriptor.Position.Y, cameraDescriptor.Position.Z);
            CheckError();
        }

        private static unsafe void SetShininess()
        {
            int location = Gl.GetUniformLocation(program, ShininessVariableName);

            if (location == -1)
            {
                throw new Exception($"{ShininessVariableName} uniform not found on shader.");
            }

            Gl.Uniform1(location, Shininess);
            CheckError();
        }

        private static unsafe void DrawCharacter(Matrix4X4<float> rotateConfMovement)
        {
            // set material uniform to rubber
            var move = Matrix4X4.CreateTranslation(new Vector3D<float>(Character.PositionX, Character.PositionY, Character.PositionZ));
            //var modelMatrixForCenterCube = Matrix4X4.CreateScale((float)cubeArrangementModel.CenterCubeScale); // pulzal
            //var rotionMatrix90 = Matrix4X4.CreateRotationY((float)(Math.PI));
            move = rotateConfMovement * move;
            
            SetModelMatrix(move);
            Gl.BindVertexArray(Character.Vao);
            Gl.DrawElements(GLEnum.Triangles, Character.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            var modelMatrixForTable = Matrix4X4.CreateScale(1f, 1f, 1f);
            SetModelMatrix(modelMatrixForTable);
            Gl.BindVertexArray(table.Vao);

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, table.Texture.Value);

            Gl.DrawElements(GLEnum.Triangles, table.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();

        }

        private static unsafe void DrawHorizontalMovingAxe()
        {
            var move = Matrix4X4.CreateTranslation(new Vector3D<float>(offsetAxe * 12.5f - 93.75f, 15f, 6.25f));

            //Console.WriteLine(Math.Floor(offsetAxe * 12.5f - 93.75f) + " >> " + Character.PositionX + " " + Character.PositionY + " " + Character.PositionZ);

            if(offsetAxe > 15f)
            {
                moveForwardAxe = false;
                
            }
            
            if(offsetAxe < 0)
            {
                moveForwardAxe = true;
            }

            
            if (moveForwardAxe)
            {
                offsetAxe += 0.05f;
                var pleaseStandUp = Matrix4X4.CreateRotationX((float)Math.PI / 2);

                var minimizeAxe = Matrix4X4.CreateScale(1f, 1f, 1f);

                move = pleaseStandUp * minimizeAxe * move;
            }
            else
            {
                offsetAxe -= 0.05f;
                var rotateConfDirection = Matrix4X4.CreateRotationY((float)Math.PI);

                var pleaseStandUp = Matrix4X4.CreateRotationX((float)Math.PI / 2);

                var minimizeAxe = Matrix4X4.CreateScale(1f, 1f, 1f);

                move = pleaseStandUp * rotateConfDirection  * minimizeAxe * move;
            }

            if(Math.Floor(offsetAxe * 12.5f - 93.75f) == Math.Floor(Character.PositionX) && Character.PositionZ == 6.25f)
            {
                Console.WriteLine("You got hit by Axe[1]. YOU LOST!");
                Environment.Exit(0);
            }

            SetModelMatrix(move);

            Gl.BindVertexArray(Axe.Vao);
            Gl.DrawElements(GLEnum.Triangles, Character.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            var modelMatrixForTable = Matrix4X4.CreateScale(1f, 1f, 1f);
            SetModelMatrix(modelMatrixForTable);
            Gl.BindVertexArray(table.Vao);

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, table.Texture.Value);

            Gl.DrawElements(GLEnum.Triangles, table.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
            

        }

        private static unsafe void DrawHorizontalMovingAxe2()
        {
            var move = Matrix4X4.CreateTranslation(new Vector3D<float>(offsetAxe2 * 12.5f - 93.75f, 15f, 56.25f));

            //Console.WriteLine(Math.Floor(offsetAxe2 * 12.5f - 93.75f) + " >> " + Character.PositionX + " " + Character.PositionY + " " + Character.PositionZ);

            if (offsetAxe2 > 15f)
            {
                moveForwardAxe = false;

            }

            if (offsetAxe2 < 0)
            {
                moveForwardAxe = true;
            }


            if (moveForwardAxe)
            {
                offsetAxe2 += 0.1f;
                var pleaseStandUp = Matrix4X4.CreateRotationX((float)Math.PI / 2);

                var minimizeAxe = Matrix4X4.CreateScale(1f, 1f, 1f);

                move = pleaseStandUp * minimizeAxe * move;
            }
            else
            {
                offsetAxe2 -= 0.1f;
                var rotateConfDirection = Matrix4X4.CreateRotationY((float)Math.PI);

                var pleaseStandUp = Matrix4X4.CreateRotationX((float)Math.PI / 2);

                var minimizeAxe = Matrix4X4.CreateScale(1f, 1f, 1f);

                move = pleaseStandUp * rotateConfDirection * minimizeAxe * move;
            }

            SetModelMatrix(move);

            if (Math.Floor(offsetAxe2 * 12.5f - 93.75f) == Math.Floor(Character.PositionX) && Character.PositionZ == 56.25f)
            {
                Console.WriteLine("You got hit by Axe[2]. YOU LOST!");
                Environment.Exit(0);
            }


            Gl.BindVertexArray(Axe.Vao);
            Gl.DrawElements(GLEnum.Triangles, Character.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            var modelMatrixForTable = Matrix4X4.CreateScale(1f, 1f, 1f);
            SetModelMatrix(modelMatrixForTable);
            Gl.BindVertexArray(table.Vao);

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, table.Texture.Value);

            Gl.DrawElements(GLEnum.Triangles, table.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();


        }

        private static unsafe void DrawPulsingFlowers()
        {
            
            for(int i = 0; i < 15; i++) {
                var moveFlower = Matrix4X4.CreateTranslation(new Vector3D<float>(positionRow[i]*12.5f - 93.75f, 0f, positionCol[i] * 12.5f - 93.75f));
                var modelMatrixForCenterCube = Matrix4X4.CreateScale((float)cubeArrangementModel.CenterCubeScale); // pulzal

                Gl.BindVertexArray(flower.Vao);

                Gl.BindVertexArray(0);

                var minimizeFlower = Matrix4X4.CreateScale(0.1f, 0.1f, 0.1f);
                Gl.BindVertexArray(flower.Vao);

                if (FlowersToHide.Contains(Tuple.Create(positionRow[i], positionCol[i])))
                {
                    minimizeFlower = Matrix4X4.CreateScale(0f, 0f, 0f);
                }

                int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
                if (textureLocation == -1)
                {
                    throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
                }
                Gl.Uniform1(textureLocation, 0);

                Gl.ActiveTexture(TextureUnit.Texture0);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
                Gl.BindTexture(TextureTarget.Texture2D, flower.Texture);

                SetModelMatrix(modelMatrixForCenterCube * minimizeFlower * moveFlower);
                Gl.DrawElements(GLEnum.Triangles, flower.IndexArrayLength, GLEnum.UnsignedInt, null);
                Gl.BindVertexArray(0);

                CheckError();
                Gl.BindTexture(TextureTarget.Texture2D, 0);
                CheckError();
                
            }
            

        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            int location = Gl.GetUniformLocation(program, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError();

            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
            location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetUpObjects()
        {

            float[] face1Color = [1f, 0f, 0f, 1.0f];
            float[] face2Color = [0.0f, 1.0f, 0.0f, 1.0f];
            float[] face3Color = [0.0f, 0.0f, 1.0f, 1.0f];
            float[] face4Color = [1.0f, 0.0f, 1.0f, 1.0f];
            float[] face5Color = [0.0f, 1.0f, 1.0f, 1.0f];
            float[] face6Color = [1.0f, 1.0f, 0.0f, 1.0f];

            Character = ObjResourceReader.CreateCharacterWithColor(Gl, face1Color);

            maxY = Character.maxY;

            Axe = ObjResourceReader.CreateAxeWithColor(Gl, face2Color);

            flower = ObjResourceReader.CreateFlowerWithColor(Gl, face1Color);

            float[] tableColor = [System.Drawing.Color.Azure.R/256f,
                                  System.Drawing.Color.Azure.G/256f,
                                  System.Drawing.Color.Azure.B/256f,
                                  1f];
            table = GlCube.CreateSquare(Gl, tableColor);

            skyBox = GlCube.CreateInteriorCube(Gl, "");
        }

        private static void Window_Closing()
        {
            Character.ReleaseGlObject();
            flower.ReleaseGlObject();
        }

        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 1000);
            int location = Gl.GetUniformLocation(program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError();
        }

        private static unsafe void SetViewMatrix()
        {
            var viewMatrix = Matrix4X4.CreateLookAt(cameraDescriptor.Position, cameraDescriptor.Target, cameraDescriptor.UpVector);
            int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }

    
}