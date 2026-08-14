using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;
public class Program
{
    private static uint _program;
    private static uint _vbo;
    private static uint _vao;
    private static uint _ebo;
    private static GL _gl;
    private static IWindow _window;
    public static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default with
        {
          Size = new Vector2D<int>(800,600),
          Title = "My first Silk.NET application!"  
        };

        _window = Window.Create(options);
        _window.Load += OnLoad;
        
        _window.Update += OnUpdate;
        _window.Render += OnRender;

        _window.Run();
        _window.Dispose();
    }

    public static unsafe void OnLoad()
    {
        _gl = _window.CreateOpenGL();
        _gl.ClearColor(Color.Wheat);
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);
        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        float[] vertices =
        {
            0.5f,  0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            -0.5f,  0.5f, 0.0f
        };
        
        uint[] indices =
        {
            0u, 1u, 3u,
            1u, 2u, 3u
        };
        
        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        fixed (float* buf = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
        Console.WriteLine("Load!");

        fixed (uint* buf = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

        // CREATE THE VERTEX CODE
        const string vertexCode = @"
        #version 330 core

        layout (location = 0) in vec3 aPosition;

        void main()
        {
            gl_Position = vec4(aPosition, 1.0);
        }";
        

        // CREATE THE FRAGMENT CODE
        const string fragmentCode = @"
        #version 330 core

        out vec4 out_color;

        void main()
        {
            out_color = vec4(1.0, 0.5, 0.2, 1.0);
        }";


        // CREATING A VERTEX SHADER
        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexCode);
        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True) //CASO O VERTEX SHADER FALHE MANDA LOG
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));


        
        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentCode);
        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if(fStatus != (int) GLEnum.Type) 
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));


        /* CREATE THE PROGRAM
        LOGICS SEQUENCE:
            - CREATE PROGRAM
            - ATTACH SHADERS
            - LINK THEM
            - GET INFO FROM PROGRAM TO DEBUG
            - DETACH SHADERS (_program, {SHADER})
            - DELETE SHADERS
        */
        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vertexShader); //ATTACH SHADER TO THE PROGRAM
        _gl.AttachShader(_program, fragmentShader); //ATTACH SHADER TO THE PROGRAM

        _gl.LinkProgram(_program); //LINKS VERTEX AND FRAGMENT SHADERS INTO A SINGLE EXECUTABLE PROGRAM READY FOR GPU RENDERING

        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));

        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        const uint positionLoc = 0;
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*) 0);

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        IInputContext input = _window.CreateInput();
        for(int i = 0; i < input.Keyboards.Count; i++)
            input.Keyboards[i].KeyDown += KeyDown;
        
    }


    private static void OnUpdate(double deltaTime)
    {

    }

    private static unsafe void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_program);
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);

    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        Console.WriteLine(key);
        if (key == Key.Escape)
            _window.Close();
    }
}