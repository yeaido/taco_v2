using OpenTK;
using OpenTK.Graphics.OpenGL;
using QuickFont;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Taco.Classes;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Taco
{
    public partial class MainForm
    {
        #region Variables
        private bool _glLoaded, _shadersLoaded, _fontLoaded;
        private int _startX, _startY, _dY, _dX;
        private int _texSystem, _texGreenCh, _texRedCh, _texYellowCh, _texRedGreenCh, _texRedYellowCh, _texYellowGreenCh;
        private int _w, _h;
        private float _pSize = 1;
        private Shader _shader, _shaderConn, _shaderCrosshair;
        private QFontDrawing _drawing;
        private QFont _mainText;
        private QFontRenderOptions _mainTextOptions;
        private Matrix4 _matrixProjection, _matrixModelview, _dragModelView, _dragProjection;
        private Vector3 _vecY = new Vector3(0f, 1f, 0f);
        private VboInfo _vbOs = new VboInfo();
        #endregion Variables

        #region VBO init and refresh
        private bool InitVboContent()
        {
            GL.GenBuffers(1, out _vbOs.SystemVbo);
            GL.GenBuffers(1, out _vbOs.SystemVao);
            GL.GenBuffers(1, out _vbOs.ColorVao);
            GL.GenBuffers(1, out _vbOs.ConnectionVbo);
            GL.GenBuffers(1, out _vbOs.ConnectionVao);
            GL.GenBuffers(1, out _vbOs.ConnectionColor);

            GL.GenBuffers(1, out _vbOs.TextQuadVbo);
            GL.GenBuffers(1, out _vbOs.TextQuadVao);
            GL.GenBuffers(1, out _vbOs.TextQuadColor);

            GL.GenBuffers(1, out _vbOs.TextLineVbo);
            GL.GenBuffers(1, out _vbOs.TextLineVao);
            GL.GenBuffers(1, out _vbOs.TextLineColor);

            _solarSystems.IsSystemVboDirty = !UploadSystemVbo(false);
            _solarSystems.IsSystemVaoDirty = !UploadSystemVao(false);
            _solarSystems.IsSystemColorVaoDirty = !UploadSystemColorVao(false);
            _solarSystems.IsConnectionVboDirty = !UploadConnectionVbo(false);
            _solarSystems.IsConnectionVaoDirty = !UploadConnectionVao(false);
            _solarSystems.IsConnectionColorVaoDirty = !UploadConnectionColorVao(false);

            return _vbOs.AllVbOsGenerated;
        }
        private bool RefreshVboContent()
        {
            if (_solarSystems.IsSystemVboDirty)
                _solarSystems.IsSystemVboDirty = !UploadSystemVbo();

            if (_solarSystems.IsSystemVaoDirty)
                _solarSystems.IsSystemVaoDirty = !UploadSystemVao();

            if (_solarSystems.IsSystemColorVaoDirty)
                _solarSystems.IsSystemColorVaoDirty = !UploadSystemColorVao();

            if (_solarSystems.IsConnectionVboDirty)
                _solarSystems.IsConnectionVboDirty = !UploadConnectionVbo();

            if (_solarSystems.IsConnectionVaoDirty)
                _solarSystems.IsConnectionVaoDirty = !UploadConnectionVao();

            if (_solarSystems.IsConnectionColorVaoDirty)
                _solarSystems.IsConnectionColorVaoDirty = !UploadConnectionColorVao();

            return _solarSystems.AllVbOsClean;
        }
        #endregion VBO init and refresh
        #region VBO Uploaders
        #region Systems
        private bool UploadSystemVbo(bool refresh = true)
        {
            if (refresh)
            {
                GL.DeleteBuffer(_vbOs.SystemVbo);
                GL.GenBuffers(1, out _vbOs.SystemVbo);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.SystemVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * _solarSystems.SystemCount), _solarSystems.SystemVboContent, BufferUsageHint.DynamicDraw);

            int bufferSize;

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return _solarSystems.SystemCount * Vector3.SizeInBytes == bufferSize;
        }
        private bool UploadSystemVao(bool refresh = true)
        {
            if (refresh)
            {
                GL.DeleteBuffer(_vbOs.SystemVao);
                GL.GenBuffers(1, out _vbOs.SystemVao);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vbOs.SystemVao);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(int) * _solarSystems.SystemCount), _solarSystems.SystemVaoContent, BufferUsageHint.DynamicDraw);

            int bufferSize;

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return (sizeof(int) * _solarSystems.SystemCount) == bufferSize;
        }
        private bool UploadSystemColorVao(bool refresh = true)
        {
            if (refresh)
            {
                GL.DeleteBuffer(_vbOs.ColorVao);
                GL.GenBuffers(1, out _vbOs.ColorVao);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.ColorVao);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(int) * _solarSystems.SystemCount), _solarSystems.SystemColorVaoContent, BufferUsageHint.DynamicDraw);

            int bufferSize;

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return (sizeof(int) * _solarSystems.SystemCount) == bufferSize;

        }
        #endregion Systems
        #region Connections
        private bool UploadConnectionVbo(bool refresh = true)
        {
            if (refresh)
            {
                GL.DeleteBuffer(_vbOs.ConnectionVbo);
                GL.GenBuffers(1, out _vbOs.ConnectionVbo);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.ConnectionVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * _solarSystems.ConnectionVertexCount), _solarSystems.ConnectionVboContent, BufferUsageHint.DynamicDraw);

            int bufferSize;

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return _solarSystems.ConnectionVertexCount * Vector3.SizeInBytes == bufferSize;
        }
        private bool UploadConnectionVao(bool refresh = true)
        {
            if (refresh)
            {
                GL.DeleteBuffer(_vbOs.ConnectionVao);
                GL.GenBuffers(1, out _vbOs.ConnectionVao);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vbOs.ConnectionVao);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(int) * _solarSystems.ConnectionVertexCount), _solarSystems.ConnectionVaoContent, BufferUsageHint.DynamicDraw);

            int bufferSize;

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return (sizeof(int) * _solarSystems.ConnectionVertexCount) == bufferSize;
        }
        private bool UploadConnectionColorVao(bool refresh = true)
        {
            if (refresh)
            {
                GL.DeleteBuffer(_vbOs.ConnectionColor);
                GL.GenBuffers(1, out _vbOs.ConnectionColor);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.ConnectionColor);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector4.SizeInBytes * _solarSystems.ConnectionVertexCount), _solarSystems.ConnectionColorVaoContent, BufferUsageHint.DynamicDraw);

            int bufferSize;

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return (Vector4.SizeInBytes * _solarSystems.ConnectionVertexCount) == bufferSize;
        }
        #endregion Connections
        #endregion VBO Uploaders
        #region Shaders
        private void CreateTexture(out int texture, Bitmap bitmap)
        {
            // load texture 
            GL.GenTextures(1, out texture);

            //Still required else TexImage2D will be applyed on the last bound texture
            GL.BindTexture(TextureTarget.Texture2D, texture);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
            OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }
        public bool InitShaders()
        {
            string vs = "";
            string fs = "";
            Bitmap bmp = null;

            string vsc = "";
            string fsc = "";

            string vsch = "";
            string fsch = "";

            var assembly = Assembly.GetExecutingAssembly();

            var result = true;

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.Shaders.shader.vert"))
            {
                if (file != null)
                    using (var reader = new StreamReader(file))
                    {
                        try
                        {
                            vs = reader.ReadToEnd();
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }
            }

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.Shaders.shader.frag"))
            {
                if (file != null)
                    using (var reader = new StreamReader(file))
                    {
                        try
                        {
                            fs = reader.ReadToEnd();
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }
            }


            using (var file = assembly.GetManifestResourceStream("Taco.Resources.Shaders.connection.vert"))
            {
                if (file != null)
                    using (var reader = new StreamReader(file))
                    {
                        try
                        {
                            vsc = reader.ReadToEnd();
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }
            }

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.Shaders.connection.frag"))
            {
                if (file != null)
                    using (var reader = new StreamReader(file))
                    {
                        try
                        {
                            fsc = reader.ReadToEnd();
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }
            }

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.Shaders.crosshair.vert"))
            {
                if (file != null)
                    using (var reader = new StreamReader(file))
                    {
                        try
                        {
                            vsch = reader.ReadToEnd();
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }
            }

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.Shaders.crosshair.frag"))
            {
                if (file != null)
                    using (var reader = new StreamReader(file))
                    {
                        try
                        {
                            fsch = reader.ReadToEnd();
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }
            }

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.TexturesOther.system.png"))
            {
                try
                {
                    if (file != null) bmp = (Bitmap)Image.FromStream(file);
                    CreateTexture(out _texSystem, bmp);
                }
                catch (Exception)
                {
                    result = false;
                }
            }

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.TexturesCrosshairs.green-crosshair.png"))
            {
                try
                {
                    if (file != null) bmp = (Bitmap)Image.FromStream(file);
                    CreateTexture(out _texGreenCh, bmp);
                }
                catch (Exception)
                {
                    result = false;
                }
            }

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.TexturesCrosshairs.red-crosshair.png"))
            {
                try
                {
                    if (file != null) bmp = (Bitmap)Image.FromStream(file);
                    CreateTexture(out _texRedCh, bmp);
                }
                catch (Exception)
                {
                    result = false;
                }
            }

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.TexturesCrosshairs.yellow-crosshair.png"))
            {
                try
                {
                    if (file != null) bmp = (Bitmap)Image.FromStream(file);
                    CreateTexture(out _texYellowCh, bmp);
                }
                catch (Exception)
                {
                    result = false;
                }
            }

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.TexturesCrosshairs.redgreen-crosshair.png"))
            {
                try
                {
                    if (file != null) bmp = (Bitmap)Image.FromStream(file);
                    CreateTexture(out _texRedGreenCh, bmp);
                }
                catch (Exception)
                {
                    result = false;
                }
            }

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.TexturesCrosshairs.redyellow-crosshair.png"))
            {
                try
                {
                    if (file != null) bmp = (Bitmap)Image.FromStream(file);
                    CreateTexture(out _texRedYellowCh, bmp);
                }
                catch (Exception)
                {
                    result = false;
                }
            }

            using (var file = assembly.GetManifestResourceStream("Taco.Resources.TexturesCrosshairs.yellowgreen-crosshair.png"))
            {
                try
                {
                    if (file != null) bmp = (Bitmap)Image.FromStream(file);
                    CreateTexture(out _texYellowGreenCh, bmp);
                }
                catch (Exception)
                {
                    result = false;
                }
            }

            _shader = new Shader(vs, fs);

            _shaderConn = new Shader(vsc, fsc);

            _shaderCrosshair = new Shader(vsch, fsch);

            _shadersLoaded = true;

            return result;
        }
        #endregion Shaders
        #region glOut Events and Helpers
        private void glOut_Load(object sender, EventArgs e)
        {
            if (!CheckOpenGL())
            {
                MessageBox.Show("Looks like you're hardware or drivers don't support OpenGL 3.3+. Exiting.",
                    "OpenGL Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
            }

            _solarSystems.InitVboData();
            SetupVariables();
            SetupViewport();
            InitVboContent();
            InitShaders();
            SetupQFont();

            EventArgs args = new EventArgs();
            MapRangeFrom_SelectedIndexChanged(0, args);
            CentreOnCharacter_SelectedIndexChanged(0, args);

            _glLoaded = true;
        }
        private void glOut_Paint(object sender, PaintEventArgs e)
        {
            if (!_glLoaded)
                return;

            BeginRender();
            RenderConnections();
            RenderSystems();
            RenderHud();
            EndRender();

            _hasRendered = true;
        }
        void glOut_MouseWheel(object sender, MouseEventArgs e)
        {
            const int cameraDivisor = 1000;
            if (e.Delta > 0)
            {
                _cameraDistance += (-50 * (_cameraDistance / cameraDivisor));
            }
            else
            {
                _cameraDistance += (50 * (_cameraDistance / cameraDivisor));
            }

            if (_cameraDistance > 12100)
                _cameraDistance = 12100f;

            if (_cameraDistance < 50.0f)
                _cameraDistance = 50.0f;

            _pSize = CalcPointSize();

            glOut.Invalidate();
        }
        private void glOut_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        _dragging = true;

                        if (_zooming)
                            StopZoom();

                        _startX = e.X;
                        _startY = e.Y;

                        _dragProjection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)_w / _h, 1f, _cameraDistance);
                        _dragModelView = _matrixModelview;

                        var foundId = -1;
                        foreach (KeyValuePair<int, SolarSystem> tempSolarSystem in from tempSolarSystem in _solarSystems.SolarSystems let tempProjection = Project(tempSolarSystem.Value.Xyz, _matrixModelview, _matrixProjection, _w, _h) where IntersectsMouseCursor(tempProjection, e.X, e.Y, 5) select tempSolarSystem)
                        {
                            foundId = tempSolarSystem.Key;
                            break;
                        }

                        if (foundId < 0) return;
                        _solarSystems.AddHighlight(foundId);
                        _isHighlighting = true;
                        _currentHighlight = foundId;
                        if (!Ticker.Enabled)
                            Ticker.Enabled = true;

                        if (e.Clicks == 2 && _isHighlighting)
                        {
                            ZoomTo(_currentHighlight);
                        }

                        if (e.Clicks == 1 && _isHighlighting)
                        {
                            if (_stickyHighlightSystems.Contains(_currentHighlight))
                                _stickyHighlightSystems.Remove(_currentHighlight);
                            else
                                _stickyHighlightSystems.Add(_currentHighlight);

                            SaveStickySystems();
                        }
                    }
                    break;
                case MouseButtons.Right:
                    {
                        var foundId = -1;
                        foreach (KeyValuePair<int, SolarSystem> tempSolarSystem in from tempSolarSystem in _solarSystems.SolarSystems let tempProjection = Project(tempSolarSystem.Value.Xyz, _matrixModelview, _matrixProjection, _w, _h) where IntersectsMouseCursor(tempProjection, e.X, e.Y, 5) select tempSolarSystem)
                        {
                            foundId = tempSolarSystem.Key;
                            break;
                        }

                        if (foundId < 0)
                        {
                            MenuStrip.Show(glOut, new Point(e.X, e.Y));
                        }
                        else
                        {
                            if (_solarSystems.HomeSystemId != foundId)
                                _solarSystems.SetCurrentHomeSystem(foundId);
                            else if (_solarSystems.HomeSystemId == foundId)
                                _solarSystems.ClearHomeSystem();
                        }

                        if (!Ticker.Enabled)
                            Ticker.Enabled = true;
                    }
                    break;
            }
        }
        private void glOut_MouseUp(object sender, MouseEventArgs e)
        {
            //if ((e.Button != MouseButtons.Right) || !_dragging) return;

            _dragging = false;

            _dX = 0;
            _dY = 0;
        }
        private void glOut_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_hasRendered)
                return;

            if (e.Button == MouseButtons.Left)
            {
                if (_dragging)
                {
                    _dX = e.X - _startX;
                    _dY = e.Y - _startY;

                    var ray = new MouseRay((_w / 2) - _dX, (_h / 2) - _dY, _dragModelView, _dragProjection);

                    _lookAt.X = ray.End.X;
                    _lookAt.Y = ray.End.Y;

                    glOut.Invalidate();
                }
            }
            else
            {
                var foundId = -1;

                foreach (var tempSolarSystem in from tempSolarSystem in _solarSystems.SolarSystems let tempProjection = Project(tempSolarSystem.Value.Xyz, _matrixModelview, _matrixProjection, _w, _h) where IntersectsMouseCursor(tempProjection, e.X, e.Y, 5) select tempSolarSystem)
                {
                    foundId = tempSolarSystem.Key;
                    break;
                }

                if ((!_isHighlighting) && (_currentHighlight == foundId) && (foundId != -1))
                {
                    _isHighlighting = true;
                }
                else if ((_isHighlighting) && (_currentHighlight != foundId))
                {
                    _isHighlighting = false;
                    _highlightTick = 0;
                    _solarSystems.RemoveHighlight(_currentHighlight);
                }
                else if ((!_isHighlighting) && (foundId != -1))
                {
                    _currentHighlight = foundId;
                }
            }
        }
        private void glOut_Resize(object sender, EventArgs e)
        {
            if (!_glLoaded)
                return;

            SetupViewport();
        }
        private bool IntersectsMouseCursor(Vector2 point, int mouseX, int mouseY, int radius)
        {
            //(x - center_x)^2 + (y - center_y)^2 < radius^2

            return Math.Pow(mouseX - point.X, 2) + Math.Pow(mouseY - point.Y, 2) <= Math.Pow(radius, 2);
        }
        public float CalcPointSize()
        {
            const float pMax = 10;
            const float pMin = 7;
            const float divisor = 40;

            var temp = ((pMin + pMax) - (_cameraDistance / divisor));

            if (temp > pMax)
                temp = pMax;

            if (temp < pMin)
                temp = pMin;

            return temp;
        }
        private bool CheckOpenGL()
        {
            if (File.Exists(Application.StartupPath + @"\taco-gldiag.txt"))
            {
                using (var diagWrite = new StreamWriter(Application.StartupPath + @"\taco-gldiag.txt"))
                {
                    var Renderer = GL.GetString(StringName.Renderer);
                    var GLSLang = GL.GetString(StringName.ShadingLanguageVersion);
                    var Vendor = GL.GetString(StringName.Vendor);
                    var Version = GL.GetString(StringName.Version);

                    var ExtensionsRaw = GL.GetString(StringName.Extensions);
                    var splitter = new string[] { " " };
                    var Extensions = ExtensionsRaw.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

                    diagWrite.WriteLine("===================================[ Main Info ]");
                    diagWrite.WriteLine("TACO Version: v0.9.0b");
                    diagWrite.WriteLine("Vendor: " + Vendor);
                    diagWrite.WriteLine("Renderer: " + Renderer);
                    diagWrite.WriteLine("GL Version: " + Version);
                    diagWrite.WriteLine("GLSL Version: " + GLSLang);
                    diagWrite.WriteLine(Environment.NewLine);

                    diagWrite.WriteLine("===================================[ Extensions ]");
                    foreach (var extension in Extensions)
                        diagWrite.WriteLine(extension);
                    diagWrite.WriteLine(Environment.NewLine);

                    diagWrite.WriteLine("===================================[ Framebuffer ]");
                    diagWrite.WriteLine(Analyze(GetPName.Doublebuffer, eType.Boolean));
                    diagWrite.WriteLine(Analyze(GetPName.MaxColorAttachments, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxDrawBuffers, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.AuxBuffers, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.DrawBuffer, eType.IntEnum));
                    diagWrite.WriteLine(Analyze(GetPName.MaxSamples, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxViewportDims, eType.IntArray2));
                    diagWrite.WriteLine(Analyze(GetPName.Viewport, eType.IntArray4));
                    diagWrite.WriteLine(Environment.NewLine);

                    diagWrite.WriteLine("===================================[ Framebuffer channels ]");
                    diagWrite.WriteLine(Analyze(GetPName.RedBits, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.GreenBits, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.BlueBits, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.AlphaBits, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.DepthBits, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.StencilBits, eType.Int));

                    diagWrite.WriteLine(Analyze(GetPName.AccumRedBits, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.AccumGreenBits, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.AccumBlueBits, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.AccumAlphaBits, eType.Int));
                    diagWrite.WriteLine(Environment.NewLine);

                    diagWrite.WriteLine("===================================[ Textures ]");
                    diagWrite.WriteLine(Analyze(GetPName.MaxCombinedTextureImageUnits, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxVertexTextureImageUnits, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxTextureImageUnits, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxTextureUnits, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxTextureSize, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.Max3DTextureSize, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxCubeMapTextureSize, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxRenderbufferSize, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxTextureLodBias, eType.Int));
                    diagWrite.WriteLine(Environment.NewLine);

                    diagWrite.WriteLine("===================================[ Point&Line volumes ]");
                    diagWrite.WriteLine(Analyze(GetPName.AliasedPointSizeRange, eType.FloatArray2));
                    diagWrite.WriteLine(Analyze(GetPName.PointSizeMin, eType.Float));
                    diagWrite.WriteLine(Analyze(GetPName.PointSizeMax, eType.Float));
                    diagWrite.WriteLine(Analyze(GetPName.PointSizeGranularity, eType.Float));
                    diagWrite.WriteLine(Analyze(GetPName.PointSizeRange, eType.FloatArray2));

                    diagWrite.WriteLine(Analyze(GetPName.AliasedLineWidthRange, eType.FloatArray2));
                    diagWrite.WriteLine(Analyze(GetPName.LineWidthGranularity, eType.Float));
                    diagWrite.WriteLine(Analyze(GetPName.LineWidthRange, eType.FloatArray2));
                    diagWrite.WriteLine(Environment.NewLine);

                    diagWrite.WriteLine("===================================[ VBO ]");
                    diagWrite.WriteLine(Analyze(GetPName.MaxElementsIndices, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxElementsVertices, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxVertexAttribs, eType.Int));
                    diagWrite.WriteLine(Environment.NewLine);

                    diagWrite.WriteLine("===================================[ GLSL ]");
                    diagWrite.WriteLine(Analyze(GetPName.MaxCombinedFragmentUniformComponents, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxCombinedGeometryUniformComponents, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxCombinedVertexUniformComponents, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxFragmentUniformComponents, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxVertexUniformComponents, eType.Int));

                    diagWrite.WriteLine(Analyze(GetPName.MaxCombinedUniformBlocks, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxFragmentUniformBlocks, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxGeometryUniformBlocks, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxVertexUniformBlocks, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxUniformBlockSize, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxUniformBufferBindings, eType.Int));

                    diagWrite.WriteLine(Analyze(GetPName.MaxVaryingFloats, eType.Int));
                    diagWrite.WriteLine(Environment.NewLine);

                    diagWrite.WriteLine("===================================[ Transform Feedback ]");
                    diagWrite.WriteLine(Analyze(GetPName.MaxTransformFeedbackInterleavedComponents, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxTransformFeedbackSeparateAttribs, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxTransformFeedbackSeparateComponents, eType.Int));
                    diagWrite.WriteLine(Environment.NewLine);

                    diagWrite.WriteLine("===================================[ Fixed-Func Stacks ]");
                    diagWrite.WriteLine(Analyze(GetPName.MaxClientAttribStackDepth, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxAttribStackDepth, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxProjectionStackDepth, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxModelviewStackDepth, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxTextureStackDepth, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxNameStackDepth, eType.Int));
                    diagWrite.WriteLine(Environment.NewLine);

                    diagWrite.WriteLine("===================================[ Fixed-Func misc. stuff ]");
                    diagWrite.WriteLine(Analyze(GetPName.MaxEvalOrder, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxClipPlanes, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxArrayTextureLayers, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxListNesting, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxLights, eType.Int));
                    diagWrite.WriteLine(Analyze(GetPName.MaxTextureCoords, eType.Int));
                }
            }

            var majorVersion = GL.GetInteger(GetPName.MajorVersion);
            var minorVersion = GL.GetInteger(GetPName.MinorVersion);

            if (majorVersion > 3)
            {
                return true;
            }

            return majorVersion == 3 && minorVersion >= 3;
        }
        public string Analyze(GetPName pname, eType type)
        {
            bool result1b;
            int result1i;
            int[] result2i = new int[2];
            int[] result4i = new int[4];
            float result1f;
            Vector2 result2f;
            Vector4 result4f;
            string output;

            switch (type)
            {
                case eType.Boolean:
                    GL.GetBoolean(pname, out result1b);
                    output = pname + ": " + result1b;
                    break;
                case eType.Int:
                    GL.GetInteger(pname, out result1i);
                    output = pname + ": " + result1i;
                    break;
                case eType.IntEnum:
                    GL.GetInteger(pname, out result1i);
                    output = pname + ": " + (All)result1i;
                    break;
                case eType.IntArray2:
                    GL.GetInteger(pname, result2i);
                    output = pname + ": ( " + result2i[0] + ", " + result2i[1] + " )";
                    break;
                case eType.IntArray4:
                    GL.GetInteger(pname, result4i);
                    output = pname + ": ( " + result4i[0] + ", " + result4i[1] + " ) ( " + result4i[2] + ", " + result4i[3] + " )";
                    break;
                case eType.Float:
                    GL.GetFloat(pname, out result1f);
                    output = pname + ": " + result1f;
                    break;
                case eType.FloatArray2:
                    GL.GetFloat(pname, out result2f);
                    output = pname + ": ( " + result2f.X + ", " + result2f.Y + " )";
                    break;
                case eType.FloatArray4:
                    GL.GetFloat(pname, out result4f);
                    output = pname + ": ( " + result4f.X + ", " + result4f.Y + ", " + result4f.Z + ", " + result4f.W + " )";
                    break;
                default: throw new NotImplementedException();
            }

            ErrorCode err = GL.GetError();
            if (err != ErrorCode.NoError)
                return "Unsupported Token: " + pname;

            return output;
        }
        #endregion glOut Events and Helpers
        #region GL Setup
        private void SetupVariables()
        {
            _w = glOut.Width;
            _h = glOut.Height;

            _cameraDistance = _conf.CameraDistance;

            if (_cameraDistance < 50)
                _cameraDistance = 50.0f;

            if (_conf.PreserveLookAt)
            {
                var lookAtX = _conf.LookAtX;
                var lookAtY = _conf.LookAtY;

                _lookAt = new Vector3(lookAtX, lookAtY, 0f);
                _eye = new Vector3(_lookAt.X, _lookAt.Y, _cameraDistance);
            }
            else
            {
                _lookAt = new Vector3(0f, 0f, 0f);
                _eye = new Vector3(0f, 0f, _cameraDistance);
            }

            _pSize = CalcPointSize();

        }
        private void SetupViewport()
        {
            if (!_glLoaded)
                return;

            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.PointSmooth);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.Blend);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


            GL.LoadIdentity();

            _w = glOut.Width;
            _h = glOut.Height;
            GL.Viewport(0, 0, _w, _h);
        }
        #endregion GL Setup
        #region Rendering
        private void RenderSystems()
        {
            if (_shadersLoaded)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.PointSprite);
                GL.Enable(EnableCap.ProgramPointSize);

                _shader.BindTexture(_texSystem, TextureUnit.Texture0, "tex");
                _shader.SetVariable("projection", _matrixProjection);
                _shader.SetVariable("modelView", _matrixModelview);
                _shader.SetVariable("hlpoints", _solarSystems.UniformSystems);
                _shader.SetVariable("hlsizes", _solarSystems.UniformSizes);
                _shader.SetVariable("hlcolors", _solarSystems.UniformColors);
                _shader.SetVariable("pointsize", _pSize);
                _shader.SetVariable("ncolor", SolarSystem.DefaultDrawColor);

                Shader.Bind(_shader);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.ColorVao);
            GL.ColorPointer(4, ColorPointerType.UnsignedByte, sizeof(int), IntPtr.Zero);
            GL.EnableClientState(ArrayCap.ColorArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.SystemVbo);
            GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vbOs.SystemVao);

            GL.EnableClientState(ArrayCap.VertexArray);

            GL.DrawElements(PrimitiveType.Points, _solarSystems.SystemCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.ColorArray);

            if (_shadersLoaded)
            {
                Shader.Bind(null);

                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Disable(EnableCap.ProgramPointSize);
                GL.Disable(EnableCap.PointSprite);
                GL.Disable(EnableCap.Texture2D);
            }
        }
        private void RenderConnections()
        {
            if (_shadersLoaded)
            {
                _shaderConn.SetVariable("projection", _matrixProjection);
                _shaderConn.SetVariable("modelView", _matrixModelview);
                GL.Enable(EnableCap.ColorMaterial);
                Shader.Bind(_shaderConn);
            }

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.ConnectionVbo);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.ConnectionColor);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vbOs.ConnectionVao);

            GL.EnableClientState(ArrayCap.VertexArray);

            GL.DrawElements(PrimitiveType.Lines, _solarSystems.ConnectionVertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.DisableClientState(ArrayCap.VertexArray);

            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(0);

            if (_shadersLoaded)
            {
                Shader.Bind(null);
                GL.Disable(EnableCap.ColorMaterial);
            }
        }
        private void RenderHud()
        {
            ResetVbOs();
            _solarSystems.RemoveExpiredAlerts();
            DrawCrossHairLabels();
            DrawHoverLabels();
            DrawStickyLabels();

            RenderCrossHairs();
            RenderCrossHairLabels();
            RenderFont();
        }
        private void DrawHoverLabels()
        {
            Vector2 screenTemp;

            if (!_fontLoaded)
                return;

            if (!_isHighlighting)
                return;

            var hovering = _solarSystems.SolarSystems[_currentHighlight];


            if ((!_solarSystems.GreenCrossHairIDs.Contains(_currentHighlight)) && (!_solarSystems.RedCrossHairIDs.Contains(_currentHighlight)))
            {
                screenTemp = Project(_solarSystems.SolarSystems[_currentHighlight].Xyz, _matrixModelview, _matrixProjection, _w, _h);
                var pos = new Vector3((float)Math.Round(screenTemp.X + 10), _h - (float)Math.Round(screenTemp.Y - 10), 0.01f);

                var displayName = _solarSystems.SolarSystems[_currentHighlight].Name;

                var dp = new QFontDrawingPimitive(_mainText, _mainTextOptions ?? new QFontRenderOptions());
                dp.Print(displayName, pos, QFontAlignment.Left, Color.White);
                _drawing.DrawingPimitiveses.Add(dp);
            }

            var numSystems = hovering.ConnectedTo.Count;

            if (numSystems <= 0) return;
            foreach (SolarSystemConnection tempConnection in _solarSystems.SolarSystems[_currentHighlight].ConnectedTo.Where(tempConnection => (!_solarSystems.GreenCrossHairIDs.Contains(tempConnection.ToSystemNativeId)) && (!_solarSystems.RedCrossHairIDs.Contains(tempConnection.ToSystemNativeId))))
            {
                screenTemp = Project(_solarSystems.SolarSystems[tempConnection.ToSystemNativeId].Xyz, _matrixModelview, _matrixProjection, _w, _h);
                Vector3 tempPos = new Vector3((float)Math.Round(screenTemp.X + 10), _h - (float)Math.Round(screenTemp.Y - 10), 0.01f);

                string displayName = _solarSystems.SolarSystems[tempConnection.ToSystemNativeId].Name;

                var dp = new QFontDrawingPimitive(_mainText, _mainTextOptions ?? new QFontRenderOptions());
                dp.Print(displayName, tempPos, QFontAlignment.Left, Color.FromArgb(255, 160, 160, 160));
                _drawing.DrawingPimitiveses.Add(dp);
            }
        }
        private HashSet<int> ConnectedIds(int systemId)
        {
            var tempIds = new HashSet<int>();

            foreach (var tempConnection in _solarSystems.SolarSystems[systemId].ConnectedTo)
            {
                tempIds.Add(tempConnection.ToSystemNativeId);
            }

            return tempIds;
        }
        private void DrawStickyLabels()
        {

            if (!_fontLoaded)
                return;

            foreach (var tempId in _stickyHighlightSystems)
            {
                if ((!_solarSystems.GreenCrossHairIDs.Contains(tempId)) && (!_solarSystems.RedCrossHairIDs.Contains(tempId)))
                {
                    if (_isHighlighting && ((_currentHighlight == tempId) || ConnectedIds(_currentHighlight).Contains(tempId)))
                        continue;

                    var screenTemp = Project(_solarSystems.SolarSystems[tempId].Xyz, _matrixModelview, _matrixProjection, _w, _h);
                    var pos = new Vector3((float)Math.Round(screenTemp.X + 8), _h - (float)Math.Round(screenTemp.Y - 15), 0.01f);
                    var displayName = _solarSystems.SolarSystems[tempId].Name;

                    var dp = new QFontDrawingPimitive(_mainText, _mainTextOptions ?? new QFontRenderOptions());

                    dp.Print(displayName, pos, QFontAlignment.Left, Color.FromArgb(255, 110, 110, 110));
                    _drawing.DrawingPimitiveses.Add(dp);
                }
            }
        }
        private void RenderFont()
        {
            if (!_fontLoaded)
                return;

            _drawing.RefreshBuffers();
            _drawing.Draw();
        }
        private void ResetVbOs()
        {
            if (!_fontLoaded)
                return;

            _drawing.ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, _w, 0, _h, -1, 0);
            _drawing.DrawingPimitiveses.Clear();
        }
        private void DrawCrossHairLabels()
        {
            if (!_fontLoaded)
                return;

            var queueLength = _solarSystems.RedCrossHairIDs.Count;
            queueLength += _solarSystems.GreenCrossHairIDs.Count;

            if (_conf.DisplayCharacterNames && _charLocations.Count > 0)
                queueLength += _charLocations.Values.Distinct().Count();

            if (queueLength <= 0) return;

            var sizes = new SizeF[queueLength];
            var pos = new Vector3[queueLength];

            Vector2 screenTemp;

            var i = 0;

            if (_conf.DisplayCharacterNames && _charLocations.Count > 0)
            {
                var tempLocations = BuildCharacterLocationIndex();

                foreach (var tempLocation in tempLocations)
                {
                    if (_fontLoaded)
                    {
                        var displayName = string.Empty;

                        foreach (var charName in tempLocation.Value)
                        {
                            displayName += charName;

                            if (charName != tempLocation.Value.Last())
                            {
                                displayName += Environment.NewLine;
                            }
                        }

                        screenTemp = Project(_solarSystems.SolarSystems[tempLocation.Key].Xyz, _matrixModelview, _matrixProjection, _w, _h);

                        var dp = new QFontDrawingPimitive(_mainText, _mainTextOptions ?? new QFontRenderOptions());

                        SizeF tempSize = dp.Measure(displayName, QFontAlignment.Right);

                        var tempAdjust = (int)Math.Round(tempSize.Height / 2);

                        Vector3 tempPos = new Vector3((float)Math.Round(screenTemp.X - 20),
                            _h - (float)Math.Round(screenTemp.Y - tempAdjust), 0.01f);

                        sizes[i] = dp.Print(displayName, tempPos, QFontAlignment.Right, Color.White);
                        pos[i] = new Vector3(tempPos.X - sizes[i].Width, tempPos.Y, tempPos.Z);
                        _drawing.DrawingPimitiveses.Add(dp);
                    }
                    i++;
                }
            }

            foreach (var systemId in _solarSystems.RedCrossHairIDs)
            {
                if (_fontLoaded)
                {
                    var displayName = _solarSystems.SolarSystems[systemId].Name;
                    var tempPathId = -1;

                    // Home system
                    if (_conf.MapRangeFrom == kHomeIndexMapRange)
                    {
                        if (_solarSystems.HomeSystemId != -1)
                            tempPathId = _solarSystems.GenerateUniquePathId(_solarSystems.HomeSystemId, systemId);
                    }
                    else // Character
                    {
                        if (_charLocations.ContainsKey(_conf.CharacterList[_conf.MapRangeFrom - 1]))
                        {
                            tempPathId =
                                _solarSystems.GenerateUniquePathId(
                                    _charLocations[_conf.CharacterList[_conf.MapRangeFrom - 1]], systemId);
                        }
                    }

                    if ((tempPathId != -1) && (_solarSystems.PathFindingCache.ContainsKey(tempPathId)))
                        if (_solarSystems.PathFindingCache[tempPathId].TotalJumps - 1 > 0)
                            displayName += " (" + (_solarSystems.PathFindingCache[tempPathId].TotalJumps - 1) + ")";

                    var tempStats = _solarSystems.GetSystemStats(systemId);

                    if (tempStats != null)
                    {
                        TimeSpan intelAge = DateTime.Now - tempStats.LastReport;
                        string mins = intelAge.Minutes.ToString();
                        string secs = intelAge.Seconds.ToString().PadLeft(2, '0');

                        if (_conf.ShowAlertAge && _conf.ShowAlertAgeSecs && _conf.ShowReportCount)
                            displayName += Environment.NewLine + mins + "m " + secs + "s | " + tempStats.ReportCount;
                        else if (_conf.ShowAlertAge && _conf.ShowAlertAgeSecs)
                            displayName = mins + ":" + secs + " | " + displayName; //displayName += Environment.NewLine + mins + "m " + secs + "s";
                        else if (_conf.ShowAlertAge && _conf.ShowReportCount)
                            displayName += Environment.NewLine + mins + "m | " + tempStats.ReportCount;
                        else if (_conf.ShowAlertAge)
                            displayName = mins + "m | " + displayName;
                        else if (_conf.ShowReportCount)
                            displayName = tempStats.ReportCount + " | " + displayName;
                    }

                    var dp = new QFontDrawingPimitive(_mainText, _mainTextOptions ?? new QFontRenderOptions());

                    screenTemp = Project(_solarSystems.SolarSystems[systemId].Xyz, _matrixModelview, _matrixProjection, _w, _h);

                    SizeF tempSize = dp.Measure(displayName);

                    var tempAdjust = (int)Math.Round(tempSize.Height / 2);

                    pos[i] = new Vector3((float)Math.Round(screenTemp.X + 23),
                        _h - (float)Math.Round(screenTemp.Y - tempAdjust), 0.01f);

                    sizes[i] = dp.Print(displayName, pos[i], QFontAlignment.Left, Color.White);
                    _drawing.DrawingPimitiveses.Add(dp);
                }
                i++;
            }

            foreach (var systemId in _solarSystems.GreenCrossHairIDs.Where(systemId => !_solarSystems.RedCrossHairIDs.Contains(systemId)))
            {
                if (_fontLoaded)
                {
                    if (!_solarSystems.SolarSystems.ContainsKey(systemId))
                    {
                        break;
                    }
                    var displayName = _solarSystems.SolarSystems[systemId].Name;

                    if (_conf.MapRangeFrom > kHomeIndexMapRange)
                    {
                        var tempPathId = -1;

                        if (_solarSystems.HomeSystemId != -1)
                        {
                            if (_charLocations.ContainsKey(_conf.CharacterList[_conf.MapRangeFrom - 1]))
                            {
                                tempPathId =
                                    _solarSystems.GenerateUniquePathId(
                                        _charLocations[_conf.CharacterList[_conf.MapRangeFrom - 1]], _solarSystems.HomeSystemId);
                            }
                        }

                        if ((tempPathId != -1) && (_solarSystems.PathFindingCache.ContainsKey(tempPathId)))
                            if (_solarSystems.PathFindingCache[tempPathId].TotalJumps - 1 > 0)
                                displayName += " (" + (_solarSystems.PathFindingCache[tempPathId].TotalJumps - 1) + ")";
                    }

                    var dp = new QFontDrawingPimitive(_mainText, _mainTextOptions ?? new QFontRenderOptions());

                    screenTemp = Project(_solarSystems.SolarSystems[systemId].Xyz, _matrixModelview, _matrixProjection, _w, _h);

                    SizeF tempSize = dp.Measure(displayName);

                    var tempAdjust = (int)Math.Round(tempSize.Height / 2);

                    pos[i] = new Vector3((float)Math.Round(screenTemp.X + 23),
                        _h - (float)Math.Round(screenTemp.Y - tempAdjust), 0.01f);

                    sizes[i] = dp.Print(displayName, pos[i], QFontAlignment.Left, Color.White);
                    _drawing.DrawingPimitiveses.Add(dp);
                }
                i++;
            }

            // Draw plates

            var quads = new Vector4[queueLength * 4];
            var lines = new Vector4[queueLength * 4];

            for (var j = 0; j < queueLength; j++)
            {
                quads[(j * 4) + 0] = new Vector4(pos[j].X - 2f, pos[j].Y + 1f, 0, 1);
                quads[(j * 4) + 1] = new Vector4(pos[j].X + sizes[j].Width + 1f, pos[j].Y + 1f, 0, 1);
                quads[(j * 4) + 2] = new Vector4(pos[j].X + sizes[j].Width + 1f, pos[j].Y - sizes[j].Height - 1f, 0, 1);
                quads[(j * 4) + 3] = new Vector4(pos[j].X - 2f, pos[j].Y - sizes[j].Height - 1f, 0, 1);

                lines[(j * 4) + 0] = new Vector4(pos[j].X - 2.5f, pos[j].Y + 1.5f, 0, 1);
                lines[(j * 4) + 1] = new Vector4(pos[j].X + sizes[j].Width + 1.5f, pos[j].Y + 1.5f, 0, 1);
                lines[(j * 4) + 2] = new Vector4(pos[j].X + sizes[j].Width + 1.5f, pos[j].Y - sizes[j].Height - 1.5f, 0, 1);
                lines[(j * 4) + 3] = new Vector4(pos[j].X - 2.5f, pos[j].Y - sizes[j].Height - 1.5f, 0, 1);
            }

            var quadVao = new int[queueLength * 4];
            var lineVao = new int[queueLength * 4];
            var quadColor = new Vector4[queueLength * 4];
            var lineColor = new Vector4[queueLength * 4];

            for (var k = 0; k < queueLength * 4; k++)
            {
                //Element Arrays
                quadVao[k] = k;
                lineVao[k] = k;

                //Color Arrays
                quadColor[k] = new Vector4(0.5f, 0.5f, 0.5f, 0.25f);
                lineColor[k] = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
            }

            // Quads
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.TextQuadVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector4.SizeInBytes * queueLength * 4), quads,
                BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vbOs.TextQuadVao);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(int) * queueLength * 4), quadVao,
                BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.TextQuadColor);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector4.SizeInBytes * queueLength * 4), quadColor,
                BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Lines
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.TextLineVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector4.SizeInBytes * queueLength * 4), lines,
                BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vbOs.TextLineVao);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(int) * queueLength * 4), lineVao,
                BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.TextLineColor);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector4.SizeInBytes * queueLength * 4), lineColor,
                BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        private Dictionary<int, List<string>> BuildCharacterLocationIndex()
        {
            Dictionary<int, List<string>> tempLocations = new Dictionary<int, List<string>>();

            foreach (var charLocation in _charLocations)
            {
                if (!tempLocations.ContainsKey(charLocation.Value))
                {
                    tempLocations.Add(charLocation.Value, new List<string>());
                }

                tempLocations[charLocation.Value].Add(charLocation.Key);
            }
            return tempLocations;
        }
        private void RenderCrossHairLabels()
        {
            var queueLength = _solarSystems.RedCrossHairIDs.Count;
            queueLength += _solarSystems.GreenCrossHairIDs.Count;

            if (_conf.DisplayCharacterNames && _charLocations.Count > 0)
                queueLength += _charLocations.Values.Distinct().Count();

            if (_shadersLoaded)
            {
                _shaderConn.SetVariable("projection", Matrix4.CreateOrthographicOffCenter(0, _w, 0, _h, -1, 0));
                _shaderConn.SetVariable("modelView", Matrix4.Identity);
                GL.Enable(EnableCap.ColorMaterial);
                Shader.Bind(_shaderConn);
            }

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.TextLineVbo);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.TextLineColor);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vbOs.TextLineVao);

            GL.EnableClientState(ArrayCap.VertexArray);

            for (var i = 0; i < queueLength; i++)
            {
                var startElement = i * 4 * sizeof(uint);

                GL.DrawElements(PrimitiveType.LineLoop, 4, DrawElementsType.UnsignedInt, startElement);
            }

            GL.DisableClientState(ArrayCap.VertexArray);

            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(0);

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.TextQuadVbo);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbOs.TextQuadColor);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vbOs.TextQuadVao);

            GL.EnableClientState(ArrayCap.VertexArray);

            // TODO: GL.MultiDrawElements(PrimitiveType.Quads, queueLength, DrawElementsType.UnsignedInt, );

            for (var i = 0; i < queueLength; i++)
            {
                var startElement = i * 4 * sizeof(uint);

                GL.DrawElements(PrimitiveType.Quads, 4, DrawElementsType.UnsignedInt, startElement);
            }

            GL.DisableClientState(ArrayCap.VertexArray);

            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(0);

            if (_shadersLoaded)
            {
                Shader.Bind(null);
                GL.Disable(EnableCap.ColorMaterial);
            }
        }
        private void RenderCrossHairs()
        {
            if (!_shadersLoaded) return;

            var red = new List<Vector3>();
            var yellow = new List<Vector3>();

            var redGreen = new List<Vector3>();
            var redYellow = new List<Vector3>();
            var yellowGreen = new List<Vector3>();


            foreach (var systemId in _solarSystems.RedCrossHairIDs)
            {
                if (_charLocations.ContainsValue(systemId))
                {
                    // Char in system + alert
                    redYellow.Add(_solarSystems.SolarSystems[systemId].Xyz);
                }
                else if (_solarSystems.GreenCrossHairIDs.Contains(systemId))
                {
                    // Home system + alert
                    redGreen.Add(_solarSystems.SolarSystems[systemId].Xyz);
                }
                else
                {
                    // Plain red
                    red.Add(_solarSystems.SolarSystems[systemId].Xyz);
                }
            }

            foreach (var systemId in _charLocations.Values)
            {
                if (_solarSystems.GreenCrossHairIDs.Contains(systemId) && !_solarSystems.RedCrossHairIDs.Contains(systemId))
                {
                    // Char in home system + not alerting
                    yellowGreen.Add(_solarSystems.SolarSystems[systemId].Xyz);
                }
                else
                {
                    yellow.Add(_solarSystems.SolarSystems[systemId].Xyz);
                }
            }        

            var green = (from systemId in _solarSystems.GreenCrossHairIDs
                         where !_solarSystems.RedCrossHairIDs.Contains(systemId) && !_charLocations.ContainsValue(systemId)
                         select _solarSystems.SolarSystems[systemId].Xyz).ToList();


            // ReSharper disable once JoinDeclarationAndInitializer
            bool renderRed, renderGreen, renderYellow, renderRedGreen, renderRedYellow, renderYellowGreen;
            // ReSharper disable once JoinDeclarationAndInitializer
            Vector3[] redCoords, yellowCoords, greenCoords, redGreenCoords, redYellowCoords, yellowGreenCoords;
            // ReSharper disable once JoinDeclarationAndInitializer
            int[] redVaoContent, yellowVaoContent, greenVaoContent, redGreenVaoContent, redYellowVaoContent, yellowGreenVaoContent;

            renderRed = renderYellow = renderGreen = renderRedGreen = renderRedYellow = renderYellowGreen = false;
            redCoords = yellowCoords = greenCoords = redGreenCoords = redYellowCoords = yellowGreenCoords = new Vector3[0];
            redVaoContent = yellowVaoContent = greenVaoContent = redGreenVaoContent = redYellowVaoContent = yellowGreenVaoContent = new int[0];

            if (red.Count > 0)
            {
                renderRed = true;
                redCoords = new Vector3[red.Count];
                red.CopyTo(redCoords);
                redVaoContent = Enumerable.Range(0, red.Count).ToArray();
            }

            if (yellow.Count > 0)
            {
                renderYellow = true;
                yellowCoords = new Vector3[yellow.Count];
                yellow.CopyTo(yellowCoords);
                yellowVaoContent = Enumerable.Range(0, yellow.Count).ToArray();
            }

            if (green.Count > 0)
            {
                renderGreen = true;
                greenCoords = new Vector3[green.Count];
                green.CopyTo(greenCoords);
                greenVaoContent = Enumerable.Range(0, green.Count).ToArray();
            }

            if (redGreen.Count > 0)
            {
                renderRedGreen = true;
                redGreenCoords = new Vector3[redGreen.Count];
                redGreen.CopyTo(redGreenCoords);
                redGreenVaoContent = Enumerable.Range(0, redGreen.Count).ToArray();
            }

            if (redYellow.Count > 0)
            {
                renderRedYellow = true;
                redYellowCoords = new Vector3[redYellow.Count];
                redYellow.CopyTo(redYellowCoords);
                redYellowVaoContent = Enumerable.Range(0, redYellow.Count).ToArray();
            }

            if (yellowGreen.Count > 0)
            {
                renderYellowGreen = true;
                yellowGreenCoords = new Vector3[yellowGreen.Count];
                yellowGreen.CopyTo(yellowGreenCoords);
                yellowGreenVaoContent = Enumerable.Range(0, yellowGreen.Count).ToArray();
            }

            if (!renderRed && !renderYellow && !renderGreen && !renderRedGreen && !renderRedYellow && !renderYellowGreen) return;

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.PointSprite);
            GL.Enable(EnableCap.ProgramPointSize);

            _shaderCrosshair.SetVariable("projection", _matrixProjection);
            _shaderCrosshair.SetVariable("modelView", _matrixModelview);

            Shader.Bind(_shaderCrosshair);

            if (renderRed)
            {
                RenderColourCrossHairs(redCoords, redVaoContent, _texRedCh);
            }

            if (renderYellow)
            {
                RenderColourCrossHairs(yellowCoords, yellowVaoContent, _texYellowCh);
            }

            if (renderGreen)
            {
                RenderColourCrossHairs(greenCoords, greenVaoContent, _texGreenCh);
            }

            if (renderRedGreen)
            {
                RenderColourCrossHairs(redGreenCoords, redGreenVaoContent, _texRedGreenCh);
            }

            if (renderRedYellow)
            {
                RenderColourCrossHairs(redYellowCoords, redYellowVaoContent, _texRedYellowCh);
            }

            if (renderYellowGreen)
            {
                RenderColourCrossHairs(yellowGreenCoords, yellowGreenVaoContent, _texYellowGreenCh);
            }

            Shader.Bind(null);

            GL.Disable(EnableCap.ProgramPointSize);
            GL.Disable(EnableCap.PointSprite);
            GL.Disable(EnableCap.Texture2D);
        }
        private void RenderColourCrossHairs(Vector3[] coords, int[] vaoContent, int textureId)
        {
            int tempBuffer;
            int tempVaoBuffer;

            _shaderCrosshair.BindTexture(textureId, TextureUnit.Texture0, "tex");

            GL.GenBuffers(1, out tempBuffer);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, tempBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * coords.Length), coords,
                BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.GenBuffers(1, out tempVaoBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, tempVaoBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(int) * vaoContent.Length), vaoContent,
                BufferUsageHint.DynamicDraw);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.DrawElements(PrimitiveType.Points, coords.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.DisableClientState(ArrayCap.VertexArray);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DeleteBuffer(tempVaoBuffer);
            GL.DeleteBuffer(tempBuffer);
        }
        private void SetupQFont()
        {
            _drawing = new QFontDrawing();

            var builderConfig = new QuickFont.Configuration.QFontBuilderConfiguration(false)
            {
                TextGenerationRenderHint = QuickFont.Configuration.TextGenerationRenderHint.SizeDependent
            };

            var assembly = Assembly.GetExecutingAssembly();
            Font tempFont = new Font(LoadFontFamilyFromResource("Taco.Resources.Fonts.Taco.ttf", ref assembly), 6);
            _mainText = new QFont(tempFont, builderConfig);

            _mainTextOptions = new QFontRenderOptions()
            {
                DropShadowActive = false,
                Colour = Color.White,
                WordSpacing = .5f,
                LineSpacing = 1.3f
            };

            _fontLoaded = true;
        }
        private FontFamily LoadFontFamilyFromResource(string resourceName, ref Assembly assembly)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                try
                {
                    var buffer = new byte[stream.Length];

                    stream.Read(buffer, 0, buffer.Length);

                    var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    try
                    {
                        var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                        var fontCollection = new PrivateFontCollection();
                        fontCollection.AddMemoryFont(ptr, buffer.Length);
                        return fontCollection.Families[0];
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        private Vector2 Project(Vector3 pos, Matrix4 viewMatrix, Matrix4 projectionMatrix, int screenWidth, int screenHeight)
        {
            pos = Vector3.Transform(pos, viewMatrix);
            pos = Vector3.Transform(pos, projectionMatrix);
            pos.X /= pos.Z;
            pos.Y /= pos.Z;
            pos.X = (pos.X + 1) * screenWidth / 2;
            pos.Y = (pos.Y + 1) * screenHeight / 2;

            return new Vector2(pos.X, screenHeight - pos.Y);
        }
        private void BeginRender()
        {
            if (!_solarSystems.AllVbOsClean || !_solarSystems.IsDataClean || !_solarSystems.AreUniformsClean)
            {
                if (!_solarSystems.IsDataClean)
                    _solarSystems.RefreshVboData();

                if (!_solarSystems.AllVbOsClean)
                    RefreshVboContent();

                if (!_solarSystems.AreUniformsClean)
                    _solarSystems.BuildUniforms();
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _matrixProjection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)_w / _h, 0.1f, _cameraDistance + 10f);
            _eye = new Vector3(_lookAt.X, _lookAt.Y, _cameraDistance);
            _matrixModelview = Matrix4.Identity * Matrix4.LookAt(_eye, _lookAt, _vecY);
        }
        private void EndRender()
        {
            GL.Flush();
            glOut.SwapBuffers();
        }
        #endregion Rendering

    }
}
