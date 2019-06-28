using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.BulletPhysics;
using BulletSharp.Math;
using BulletSharp.SoftBody;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
using TGC.Core.Terrain;
using TGC.Core.Textures;
using TGC.Core.Geometry;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Collision;
using TGC.Core.BoundingVolumes;


namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        //Declaro Cosas del Escenario
        private TgcScene Plaza { get; set; }
        private TgcMesh Rueda { get; set; }
        private List<TgcMesh> MayasAutoFisico1 { get; set; }
        private List<TgcMesh> MayasAutoFisico2 { get; set; }
        private AutoManejable AutoFisico1 { get; set; }
        private TgcTexture SombraAuto1 { get; set; }
        private List<TgcMesh> MayasIA{ get; set; }
        private AutoManejable AutoFisico2 { get; set; }
        private AutoIA Policia01 { get; set; }
        private AutoIA Policia02 { get; set; }
        private AutoIA Policia03 { get; set; }
        private AutoIA Policia04 { get; set; }
        private AutoIA Policia05 { get; set; }
        private AutoIA Policia06 { get; set; }
        private AutoIA Policia07 { get; set; }
        private AutoIA Policia08 { get; set; }
        private AutoIA Policia09 { get; set; }
        private AutoIA Policia10 { get; set; }

        //Declaro Cosas de HUD
        private CustomSprite VelocimetroFondo;
        private CustomSprite VelocimetroAguja;
        private CustomSprite Barra1;
        private CustomSprite Barra2;
        private CustomSprite Pausa;
        private CustomSprite Alarma;
        private float EscalaVelocimetro;
        private Drawer2D Huds;

        //Declaro Imagenes del Menu de Inicio
        private Drawer2D Inicio;
        private CustomSprite PantallaInicioFondo;
        private CustomSprite PantallaInicioMenu;
        private CustomSprite PantallaInicioControles;
        private float EscalaInicioAltura;
        private float EscalaInicioAncho;

        // Fisica del Mundo 
        private FisicaMundo Fisica;
        private TgcSkyBox Cielo;

        //Camaras
        private AutoManejable JugadorActivo { get; set; }
        private CamaraAtrasAF Camara01 { get; set; }
        private CamaraAtrasAF Camara02 { get; set; }
        private CamaraEspectador Camara03 { get; set; }

        // Declaro Emisor de particulas
        public string PathHumo { get; set; }

        //SONIDO ///////////
        //Ambiente
        private TgcStaticSound Musica;
        private TgcStaticSound Tribuna;
        private Tgc3dSound Encendido;

        // Colisiones
        private bool Choque { get; set; }
        private bool inGame { get; set; }

        ////////////////////////////////////////////

        int SwitchMusica { get; set; }
        int SwitchFX { get; set; }
        int SwitchInicio { get; set; }
        int SwitchCamara { get; set; }
        int SwitchInvisibilidadJ1 { get; set; }
        int SwitchInvisibilidadJ2 { get; set; }

        private AutoManejable[] Jugadores { get; set; }
        private List<AutoManejable> Players { get; set; }
        private List<AutoIA> Policias { get; set; }

        //public Microsoft.DirectX.Direct3D.Effect Parallax;
        public Microsoft.DirectX.Direct3D.Effect Invisibilidad { get; set; }
        public float Tiempo { get; set; }
        private Surface g_pDepthStencil; // Depth-stencil buffer
        private Texture g_pRenderTarget;
        private VertexBuffer g_pVBV3D;

        public bool juegoDoble = false;

        public override void Init()
        {
            Tiempo = 0;
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            Plaza = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Plaza-TgcScene.xml");
            MayasIA= new TgcSceneLoader().loadSceneFromFile(MediaDir + "AutoPolicia-TgcScene.xml").Meshes;
            MayasAutoFisico1 = new TgcSceneLoader().loadSceneFromFile(MediaDir + "AutoAmarillo-TgcScene.xml").Meshes;
            MayasAutoFisico2 = new TgcSceneLoader().loadSceneFromFile(MediaDir + "AutoNaranja-TgcScene.xml").Meshes;
            Rueda = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Rueda-TgcScene.xml").Meshes[0];
            SombraAuto1 = TgcTexture.createTexture(MediaDir + "Textures\\SombraAuto.png");
            PathHumo = MediaDir + "Textures\\TexturaHumo.png";
            //Parallax = TGCShaders.Instance.LoadEffect(ShadersDir + "Parallax.fx");

            //Shader Invisibilidad
            string compilationErrors;
            Invisibilidad = Microsoft.DirectX.Direct3D.Effect.FromFile(d3dDevice, ShadersDir + "\\Invisibilidad.fx", null, null, ShaderFlags.PreferFlowControl,
                null, out compilationErrors);
            if (Invisibilidad == null)
            {
                throw new System.Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            Invisibilidad.Technique = "DefaultTechnique";

            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
               d3dDevice.PresentationParameters.BackBufferHeight,
               DepthFormat.D24S8, MultiSampleType.None, 0, true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8,
                Pool.Default);

            Invisibilidad.SetValue("g_RenderTarget", g_pRenderTarget);

            // Resolucion de pantalla
            Invisibilidad.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            Invisibilidad.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            CustomVertex.PositionTextured[] vertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            //vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);


            //Cielo
            Cielo = new TgcSkyBox
            {
                Center = TGCVector3.Empty,
                Size = new TGCVector3(10000, 10000, 10000)
            };
            var cieloPath = MediaDir + "Cielo\\";

            Cielo.setFaceTexture(TgcSkyBox.SkyFaces.Up, cieloPath + "cloudtop_up.jpg");
            Cielo.setFaceTexture(TgcSkyBox.SkyFaces.Down, cieloPath + "cloudtop_down.jpg");
            Cielo.setFaceTexture(TgcSkyBox.SkyFaces.Left, cieloPath + "cloudtop_left.jpg");
            Cielo.setFaceTexture(TgcSkyBox.SkyFaces.Right, cieloPath + "cloudtop_right.jpg");
            Cielo.setFaceTexture(TgcSkyBox.SkyFaces.Front, cieloPath + "cloudtop_front.jpg");
            Cielo.setFaceTexture(TgcSkyBox.SkyFaces.Back, cieloPath + "cloudtop_back.jpg");

            Cielo.SkyEpsilon = 11f;
            Cielo.Init();


            // Implemento la fisica 
            Fisica = new FisicaMundo();
            for (int i = 30; i<238; i++)
            {
                var objetos = BulletRigidBodyFactory.Instance.CreateRigidBodyFromTgcMesh(Plaza.Meshes[i]);
                Fisica.dynamicsWorld.AddRigidBody(objetos);
            }


            // Inicializo los coches
            AutoFisico1 = new AutoManejable(MayasAutoFisico1, Rueda, new TGCVector3(-1000, 0, 3500),270,Fisica,SombraAuto1,PathHumo);
            AutoFisico2 = new AutoManejable(MayasAutoFisico2, Rueda, new TGCVector3(4000, 0, 3500), 270, Fisica, SombraAuto1, PathHumo);
            AutoFisico2.ConfigurarTeclas(Key.W, Key.S, Key.D, Key.A, Key.LeftControl, Key.Tab);
            AutoFisico2.Media = MediaDir;
            AutoFisico1.ConfigurarTeclas(Key.UpArrow, Key.DownArrow, Key.RightArrow, Key.LeftArrow, Key.RightControl, Key.Space);
            
            AutoFisico1.Media = MediaDir;
            
            Jugadores = new[] { AutoFisico1, AutoFisico2 };
            Players = new List<AutoManejable> { AutoFisico1, AutoFisico2 }; // Para el sonido y las colisiones
            Policia01 = new AutoIA(MayasIA, Rueda, new TGCVector3(-1000, 0, 0), 270, Fisica, SombraAuto1, PathHumo, Jugadores);
            Policia02 = new AutoIA(MayasIA, Rueda, new TGCVector3(0, 0, 0), 270, Fisica, SombraAuto1, PathHumo, Jugadores);
            Policia03 = new AutoIA(MayasIA, Rueda, new TGCVector3(1000, 0, 0), 270, Fisica, SombraAuto1, PathHumo, Jugadores);
            Policia04 = new AutoIA(MayasIA, Rueda, new TGCVector3(2000, 0, 0), 270, Fisica, SombraAuto1, PathHumo, Jugadores);
            Policia05 = new AutoIA(MayasIA, Rueda, new TGCVector3(3000, 0, 0), 270, Fisica, SombraAuto1, PathHumo, Jugadores);
            Policia06 = new AutoIA(MayasIA, Rueda, new TGCVector3(-1000, 0, 300), 270, Fisica, SombraAuto1, PathHumo, Jugadores);
            Policia07 = new AutoIA(MayasIA, Rueda, new TGCVector3(0, 0, 300), 270, Fisica, SombraAuto1, PathHumo, Jugadores);
            Policia08 = new AutoIA(MayasIA, Rueda, new TGCVector3(1000, 0, 300), 270, Fisica, SombraAuto1, PathHumo, Jugadores);
            Policia09 = new AutoIA(MayasIA, Rueda, new TGCVector3(2000, 0, 300), 270, Fisica, SombraAuto1, PathHumo, Jugadores);
            Policia10 = new AutoIA(MayasIA, Rueda, new TGCVector3(3000, 0, 300), 270, Fisica, SombraAuto1, PathHumo, Jugadores);
            Policias = new List<AutoIA> { Policia01, Policia02, Policia03, Policia04, Policia05, Policia06, Policia07, Policia08, Policia09, Policia10 };

            // Inicializo las listas de BB y los BB
            foreach(var mesh in MayasAutoFisico1)
            {

            }

            ///// HUD /////
            Huds = new Drawer2D();
            Inicio = new Drawer2D();
            Barra1 = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\Barra1.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            Barra2 = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\Barra2.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            Pausa = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\Pausa.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            Alarma = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\Alarma.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            VelocimetroFondo = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\VelocimetroFondo.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(FastMath.Max(D3DDevice.Instance.Width * 0.82f, 0), FastMath.Max(D3DDevice.Instance.Height * 0.7f, 0))
            };
            VelocimetroAguja = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\VelocimetroAguja.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(FastMath.Max(D3DDevice.Instance.Width * 0.82f, 0), FastMath.Max(D3DDevice.Instance.Height * 0.7f, 0))
            };
            EscalaVelocimetro = 0.25f * D3DDevice.Instance.Height / VelocimetroFondo.Bitmap.Size.Height;
            TGCVector2 escalaVelocimetroVector = new TGCVector2(EscalaVelocimetro, EscalaVelocimetro);
            VelocimetroFondo.Scaling = escalaVelocimetroVector;
            VelocimetroAguja.Scaling = escalaVelocimetroVector;
            VelocimetroAguja.RotationCenter = new TGCVector2(VelocimetroAguja.Bitmap.Size.Height * EscalaVelocimetro / 2, VelocimetroAguja.Bitmap.Size.Height * EscalaVelocimetro / 2);

            PantallaInicioFondo = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\InicioFondo.jpg", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };

            PantallaInicioMenu = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\InicioMenu.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            PantallaInicioControles = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\InicioControles.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            EscalaInicioAltura = (float)D3DDevice.Instance.Height / (float)PantallaInicioFondo.Bitmap.Size.Height;
            EscalaInicioAncho = (float)D3DDevice.Instance.Width / (float)PantallaInicioFondo.Bitmap.Size.Width;
            TGCVector2 escalaInicio = new TGCVector2(EscalaInicioAncho, EscalaInicioAltura);
            PantallaInicioFondo.Scaling = escalaInicio;
            PantallaInicioMenu.Scaling = escalaInicio;
            PantallaInicioControles.Scaling = escalaInicio;
            Barra1.Scaling = escalaInicio;
            Barra2.Scaling = escalaInicio;
            Pausa.Scaling = escalaInicio;
            Alarma.Scaling = escalaInicio;

            // Sonido
            // Ambiente
            int volumen1 = -1800;  // RANGO DEL 0 AL -10000 (Silenciado al -10000)
            var pathMusica = MediaDir + "Musica\\Running90s.wav";
            Musica = new TgcStaticSound();
            Musica.loadSound(pathMusica, volumen1, DirectSound.DsDevice);

            int volumen2 = -400;
            var pathTribuna = MediaDir + "Musica\\Tribuna.wav";
            Tribuna = new TgcStaticSound();
            Tribuna.loadSound(pathTribuna, volumen2, DirectSound.DsDevice);

            // Encendido
            Encendido = new Tgc3dSound(MediaDir + "Musica\\Encendido.wav", Rueda.Position, DirectSound.DsDevice)
            {
                MinDistance = 80f
            };

            // Jugadores
            foreach (var auto in Players)
            {
                
                auto.sonidoAceleracion = new TgcStaticSound();
                auto.sonidoDesaceleracion = new TgcStaticSound();
                auto.frenada = new TgcStaticSound();
                auto.choque = new TgcStaticSound();

                auto.sonidoDesaceleracion.loadSound(MediaDir + "Musica\\Desacelerando.wav", -2000, DirectSound.DsDevice);
                auto.sonidoAceleracion.loadSound(MediaDir + "Musica\\Motor1.wav", -2000, DirectSound.DsDevice);
                auto.frenada.loadSound(MediaDir + "Musica\\Frenada.wav", -2000, DirectSound.DsDevice);

                // Todavia no esta el colisionador, asi que no se usa este todavia
                auto.choque.loadSound(MediaDir + "Musica\\Choque1.wav", -2000, DirectSound.DsDevice);

                //auto.sonidoAceleracion = new Tgc3dSound(MediaDir + "Musica\\Motor1.wav", auto.pos , DirectSound.DsDevice);
                //auto.sonidoDesaceleracion = new Tgc3dSound(MediaDir + "Musica\\Desacelerando.wav", auto.pos, DirectSound.DsDevice);
                //auto.frenada = new Tgc3dSound(MediaDir + "Musica\\Frenada.wav", auto.pos, DirectSound.DsDevice);
            }


            SwitchInicio = 1;
            SwitchCamara = 1;
        }


        public override void Update()
        {
            PreUpdate();

            var input = Input;

            //Camaras
            Camara01 = new CamaraAtrasAF(AutoFisico1);
            Camara02 = new CamaraAtrasAF(AutoFisico2);
            Camara03 = new CamaraEspectador();
        
            Policia01.Moverse();
            Policia02.Moverse();
            Policia03.Moverse();
            Policia04.Moverse();
            Policia05.Moverse();
            Policia06.Moverse();
            Policia07.Moverse();
            Policia08.Moverse();
            Policia09.Moverse();
            Policia10.Moverse();
            AutoFisico1.Update(input);
            AutoFisico2.Update(input);

            foreach (var Policia in Policias)
            {
                if(TgcCollisionUtils.testAABBAABB(AutoFisico1.BBFinal,Policia.BBFinal) && inGame)
                {
                    AutoFisico1.choque.play(false);
                }
            }

            switch (SwitchCamara)
            {
                case 1:
                    {
                        Camara = Camara01;
                        JugadorActivo = AutoFisico1;
                        if (input.keyPressed(Key.F6) && juegoDoble)
                        {
                            SwitchCamara = 2;
                        }
                        else if (input.keyPressed(Key.F7))
                        {
                            SwitchCamara = 3;      
                        }
                        break;
                    }
                case 2:
                    {
                        Camara = Camara02;
                        JugadorActivo = AutoFisico2;
                        if (input.keyPressed(Key.F5))
                        {
                            SwitchCamara = 1;
                        }
                        else if (input.keyPressed(Key.F7))
                        {
                            SwitchCamara = 3;
                        }
                        break;
                    }
                case 3:
                    {
                        Camara = Camara03;
                        JugadorActivo = null;
                        if (input.keyPressed(Key.F5))
                        {
                            SwitchCamara = 1;
                        }
                        else if (input.keyPressed(Key.F6) && juegoDoble)
                        {
                            SwitchCamara = 2;
                        }
                        break;
                    }
            }
            
            switch (SwitchMusica)
            {
                case 1:
                    {   
                        Musica.play(true);
                        if (Input.keyPressed(Key.F8))
                        {
                            SwitchMusica = 2;
                        }
                        break;
                    }
                case 2:
                    {
                        Musica.stop();
                        if (Input.keyPressed(Key.F8))
                        {
                            SwitchMusica = 1;
                        }
                            break;
                    }
            }
            switch (SwitchFX)
            {
                case 1:
                    {
                        Tribuna.play(true);
                        if (Input.keyPressed(Key.F9))
                        {
                            SwitchFX = 2;
                        }
                        break;
                    }
                case 2:
                    {
                        Tribuna.stop();
                        if (Input.keyPressed(Key.F9))
                        {
                            SwitchFX = 1;
                        }
                        break;
                    }
            }

            switch (SwitchInvisibilidadJ1)
            {
                case 1:
                    {
                        Jugadores[0] = AutoFisico1;
                        if (Input.keyPressed(Key.F3))
                        {
                            SwitchInvisibilidadJ1 = 2;
                        }
                        break;
                    }
                case 2:
                    {
                        Jugadores[0] = null;
                        if (Input.keyPressed(Key.F3))
                        {
                            SwitchInvisibilidadJ1 = 1;
                        }
                        break;
                    }
                default:
                    {
                        if (Input.keyPressed(Key.F3))
                        {
                            SwitchInvisibilidadJ1 = 2;
                        }
                        break;
                    }
            }
            if (juegoDoble)
            {
                switch (SwitchInvisibilidadJ2)
                {
                    case 1:
                        {
                            Jugadores[1] = AutoFisico2;
                            if (Input.keyPressed(Key.F4))
                            {
                                SwitchInvisibilidadJ2 = 2;
                            }
                            break;
                        }
                    case 2:
                        {
                            Jugadores[1] = null;
                            if (Input.keyPressed(Key.F4))
                            {
                                SwitchInvisibilidadJ2 = 1;
                            }
                            break;
                        }
                    default:
                        {
                            if (Input.keyPressed(Key.F4))
                            {
                                SwitchInvisibilidadJ2 = 2;
                            }
                            break;
                        }
                }
            }
            PostUpdate();
        }

        public override void Render()
        {

            PreRender();
            ClearTextures();

            bool invisibilidadActivada = (SwitchInvisibilidadJ1 == 2 && JugadorActivo == AutoFisico1) || (SwitchInvisibilidadJ2 == 2 && JugadorActivo == AutoFisico2);

            //Permito las particulas
            D3DDevice.Instance.ParticlesEnabled = true;
            D3DDevice.Instance.EnableParticles();


            switch (SwitchInicio)
            {
                case 1:
                    {
                        Inicio.BeginDrawSprite();
                        Inicio.DrawSprite(PantallaInicioFondo);
                        Inicio.DrawSprite(PantallaInicioMenu);
                        Inicio.EndDrawSprite();
                        if (Input.keyPressed(Key.C))
                        {
                            SwitchInicio = 2;
                            
                        }
                        if (Input.keyPressed(Key.D1))
                        {
                            Jugadores[1] = null;
                            SwitchInicio = 3;
                            SwitchMusica = 1;
                            SwitchFX = 1;
                            Encendido.play();
                            inGame = true;
                        }
                        if (Input.keyPressed(Key.D2))
                        {
                            juegoDoble = true;  
                            SwitchInicio = 3;
                            SwitchMusica = 1;
                            SwitchFX = 1;
                            Encendido.play();
                            inGame = true;

                        }
                        break;
                    }
                case 2:
                    {
                        Inicio.BeginDrawSprite();
                        Inicio.DrawSprite(PantallaInicioFondo);
                        Inicio.DrawSprite(PantallaInicioControles);
                        Inicio.EndDrawSprite();
                        if (Input.keyPressed(Key.V))
                        {
                            SwitchInicio = 1;
                        }
                        break;
                    }
                case 3:
                    {
                        var device = D3DDevice.Instance.Device;


                        Tiempo += ElapsedTime;
                        AutoFisico1.ElapsedTime = ElapsedTime;                      
                        //Cargar variables de shader

                        // dibujo la escena una textura
                        Invisibilidad.Technique = "DefaultTechnique";
                        // guardo el Render target anterior y seteo la textura como render target
                        var pOldRT = device.GetRenderTarget(0);
                        var pSurf = g_pRenderTarget.GetSurfaceLevel(0);
                        if (invisibilidadActivada)
                            device.SetRenderTarget(0, pSurf);
                        // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
                        var pOldDS = device.DepthStencilSurface;
                        // Probar de comentar esta linea, para ver como se produce el fallo en el ztest
                        // por no soportar usualmente el multisampling en el render to texture.
                        if (invisibilidadActivada)
                            device.DepthStencilSurface = g_pDepthStencil;

                        device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

                        //DrawText.drawText("Velocidad Lineal en X :" + AutoFisico1.CuerpoRigidoAuto.LinearVelocity.X, 0, 20, Color.OrangeRed);
                        //DrawText.drawText("Velocidad Lineal en Y :" + AutoFisico1.CuerpoRigidoAuto.LinearVelocity.Y, 0, 30, Color.OrangeRed);
                        DrawText.drawText("Velocidad Lineal en Z :" + juegoDoble, 0, 40, Color.OrangeRed);
                        DrawText.drawText("Velocidad P1:" + AutoFisico1.Velocidad, 0, 50, Color.Green);
                        //DrawText.drawText("Velocidad en Centro:" + AutoFisico1.CuerpoRigidoAuto.GetVelocityInLocalPoint(AutoFisico1.CuerpoRigidoAuto.CenterOfMassPosition), 0, 70, Color.Black);
                        //DrawText.drawText("Velocidad P1:" + AutoFisico1.CuerpoRigidoAuto.InterpolationLinearVelocity, 0, 90, Color.Green);

                        DrawText.drawText("Choque: " + Choque, 0, 130, Color.Black);
                        DrawText.drawText("Auto1: " + AutoFisico1.listBB.Count, 0, 150, Color.Black);

                        // MUESTRO LOS BB DE LA PLAZA
                        /*
                        foreach (var mesh in Plaza.Meshes)
                        {
                            mesh.BoundingBox.Render();
                        }
                        */
                        
                        // MUESTRO LOS BB DE LOS AUTOS
                        /*
                        foreach (var auto in Policias)
                        {
                            foreach(var mesh in auto.Mayas)
                            {
                                mesh.BoundingBox.Render();
                                mesh.BoundingBox.transform(auto.Movimiento); 
                            }
                        }
                        */
                        

                        if (juegoDoble)
                        {
                            AutoFisico2.ElapsedTime = ElapsedTime;
                            AutoFisico2.Render(ElapsedTime);
                        }

                        Plaza.RenderAll();
                        AutoFisico1.Render(ElapsedTime);
                        Policia01.Render(ElapsedTime);
                        Policia02.Render(ElapsedTime);
                        Policia03.Render(ElapsedTime);
                        Policia04.Render(ElapsedTime);
                        Policia05.Render(ElapsedTime);
                        Policia06.Render(ElapsedTime);
                        Policia07.Render(ElapsedTime);
                        Policia08.Render(ElapsedTime);
                        Policia09.Render(ElapsedTime);
                        Policia10.Render(ElapsedTime);

                        Cielo.Render();

                        //HUD
                        Huds.BeginDrawSprite();
                        if (juegoDoble)
                        {
                            Huds.DrawSprite(Barra2);
                        }
                        else
                        {
                            Huds.DrawSprite(Barra1);
                        }
                        if (invisibilidadActivada)
                        {
                            Huds.DrawSprite(Alarma);
                        }
                        if (JugadorActivo != null)
                        {
                            VelocimetroAguja.Rotation = JugadorActivo.Velocidad / 50;
                            

                            if (Input.keyDown(Key.F10))
                            {
                                Huds.DrawSprite(Pausa);
                            }

                            Huds.DrawSprite(VelocimetroFondo);
                            Huds.DrawSprite(VelocimetroAguja);

                           
                        }
                        Huds.EndDrawSprite();

                        pSurf.Dispose();

                        if (invisibilidadActivada)
                        {
                            // restuaro el render target y el stencil
                            device.DepthStencilSurface = pOldDS;
                            device.SetRenderTarget(0, pOldRT);
                            Invisibilidad.Technique = "PostProcess";
                            Invisibilidad.SetValue("time", Tiempo);
                            device.VertexFormat = CustomVertex.PositionTextured.Format;
                            device.SetStreamSource(0, g_pVBV3D, 0);
                            Invisibilidad.SetValue("g_RenderTarget", g_pRenderTarget);

                            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                            Invisibilidad.Begin(FX.None);
                            Invisibilidad.BeginPass(0);
                            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                            Invisibilidad.EndPass();
                            Invisibilidad.End();

                        }
                        RenderAxis();
                        RenderFPS();
                        break;
                    }             
            }

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        public override void Dispose()
        {
            Plaza.DisposeAll();
            AutoFisico1.Dispose();
            Policia01.Dispose();
            Cielo.Dispose();
            VelocimetroFondo.Dispose();
            VelocimetroAguja.Dispose();
            Musica.dispose();
            Tribuna.dispose();
            Encendido.dispose();
            PantallaInicioFondo.Dispose();
            PantallaInicioControles.Dispose();
            PantallaInicioMenu.Dispose();
            Pausa.Dispose();

            foreach(var auto in Policias)
            {
                //auto.motorIA.dispose();
            }
            foreach (var auto in Players)
            {
                auto.sonidoAceleracion.dispose();
                auto.sonidoDesaceleracion.dispose();
                auto.frenada.dispose();

            }

            Invisibilidad.Dispose();
            g_pRenderTarget.Dispose();
            g_pVBV3D.Dispose();
            g_pDepthStencil.Dispose();

        }

    }

    }
    

