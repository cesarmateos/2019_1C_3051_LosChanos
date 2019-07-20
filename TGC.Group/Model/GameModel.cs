using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.BulletPhysics;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Terrain;
using Microsoft.DirectX.Direct3D;

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
        private List<TgcMesh> MayasAutoFisico1 { get; set; }
        private List<TgcMesh> MayasAutoFisico2 { get; set; }
        private AutoManejable AutoFisico1 { get; set; }
        private List<TgcMesh> MayasIA { get; set; }
        private AutoManejable AutoFisico2 { get; set; }
        public PoliciasIA GrupoPolicias { get; set; }

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

        // Colisiones
        private bool InGame { get; set; }

        ////////////////////////////////////////////

        bool SwitchMusica = false;
        bool SwitchFX = false;
        int SwitchInicio { get; set; }
        int SwitchCamara { get; set; }
        int SwitchInvisibilidadJ1 { get; set; }
        int SwitchInvisibilidadJ2 { get; set; }

        public AutoManejable[] Jugadores { get; set; }
        private List<AutoManejable> Players { get; set; }
        private List<AutoIA> Policias { get; set; }
        public float Tiempo { get; set; }
        public float TiempoFinal { get; set; }
       
        public bool juegoDoble = false;
        public bool pantallaDoble = false;

        public Hud Hud;

        public Sonidos Sonidos;

        public ShaderInvisibilidad Invisible;
        public ShaderEnvMap EnvMap;

        public Microsoft.DirectX.Direct3D.Device D3dDevice;

        public override void Init()
        {
            Tiempo = 0;
            D3dDevice = D3DDevice.Instance.Device;

            Plaza = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Plaza-TgcScene.xml");
            MayasIA = new TgcSceneLoader().loadSceneFromFile(MediaDir + "AutoPolicia-TgcScene.xml").Meshes;
            MayasAutoFisico1 = new TgcSceneLoader().loadSceneFromFile(MediaDir + "AutoAmarillo-TgcScene.xml").Meshes;
            MayasAutoFisico2 = new TgcSceneLoader().loadSceneFromFile(MediaDir + "AutoNaranja-TgcScene.xml").Meshes;
            PathHumo = MediaDir + "Textures\\TexturaHumo.png";

            Sonidos = new Sonidos(MediaDir, DirectSound.DsDevice);
            Invisible = new ShaderInvisibilidad(D3dDevice, ShadersDir);
            EnvMap = new ShaderEnvMap(ShadersDir);

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
            for (int i = 30; i < 238; i++)
            {
                var objetos = BulletRigidBodyFactory.Instance.CreateRigidBodyFromTgcMesh(Plaza.Meshes[i]);
                Fisica.dynamicsWorld.AddRigidBody(objetos);
            }

            // Inicializo los coches
            AutoFisico1 = new AutoManejable(MayasAutoFisico1, new TGCVector3(-1000, 0, 3500), 270, Fisica, PathHumo, MediaDir, Sonidos);
            AutoFisico2 = new AutoManejable(MayasAutoFisico2, new TGCVector3(4000, 0, 3500), 270, Fisica, PathHumo, MediaDir, Sonidos);
            AutoFisico2.ConfigurarTeclas(Key.W, Key.S, Key.D, Key.A, Key.LeftControl, Key.Tab);
            AutoFisico1.ConfigurarTeclas(Key.UpArrow, Key.DownArrow, Key.RightArrow, Key.LeftArrow, Key.RightControl, Key.Space);
            AutoFisico1.Vida = 1000;
            AutoFisico2.Vida = 1000;
            Jugadores = new[] { AutoFisico1, AutoFisico2 };
            GrupoPolicias = new PoliciasIA(MayasIA, Fisica, PathHumo, Jugadores, MediaDir, Sonidos);
            Players = new List<AutoManejable> { AutoFisico1, AutoFisico2 }; // Para el sonido y las colisiones

            SwitchInicio = 1;
            SwitchCamara = 1;
            Hud = new Hud(MediaDir, Jugadores);
            
        }


        public override void Update()
        {
            PreUpdate();

            var input = Input;

            //Camaras
            Camara01 = new CamaraAtrasAF(AutoFisico1);
            Camara02 = new CamaraAtrasAF(AutoFisico2);
            Camara03 = new CamaraEspectador();

            GrupoPolicias.Update(juegoDoble);
            AutoFisico1.Update(input, Plaza, GrupoPolicias,InGame);
            AutoFisico2.Update(input, Plaza, GrupoPolicias,InGame);

            switch (SwitchCamara)
            {
                case 1:
                    {
                        Camara = Camara01;
                        JugadorActivo = AutoFisico1;
                        pantallaDoble = false;
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
                        pantallaDoble = false;
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
                        JugadorActivo = null; ;
                        pantallaDoble = true;
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
                case true:
                    {
                        Sonidos.SuenaMusica();
                        if (Input.keyPressed(Key.F8))
                        {
                            SwitchMusica = false;
                        }
                        break;
                    }
                case false:
                    {
                        Sonidos.ParaMusica();
                        if (Input.keyPressed(Key.F8))
                        {
                            SwitchMusica = true;
                        }
                        break;
                    }
            }
            switch (SwitchFX)
            {
                case true:
                    {
                        Sonidos.SuenaTribuna();
                        if (Input.keyPressed(Key.F9))
                        {
                            SwitchFX = false;
                        }
                        break;
                    }
                case false:
                    {
                        Sonidos.ParaTribuna();
                        if (Input.keyPressed(Key.F9))
                        {
                            SwitchFX = true;
                        }
                        break;
                    }
            }
            if (Input.keyPressed(Key.F3))
            {
                AutoFisico1.SwitchInvisibile();
            }

            if (Input.keyPressed(Key.F4))
            {
                AutoFisico2.SwitchInvisibile();
            }

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();
            ClearTextures();

            bool invisibilidadActivada = ((JugadorActivo == AutoFisico1 && AutoFisico1.Invisible) || (JugadorActivo == AutoFisico2 && AutoFisico2.Invisible));

            //Permito las particulas
            D3DDevice.Instance.ParticlesEnabled = true;
            D3DDevice.Instance.EnableParticles();


            switch (SwitchInicio)
            {
                case 1:
                    {
                        Hud.PantallaInicio();
                        if (Input.keyPressed(Key.C))
                        {
                            SwitchInicio = 2;

                        }
                        if (Input.keyPressed(Key.D1))
                        {
                            Jugadores[1] = null;
                            SwitchInicio = 3;
                            //SwitchMusica = true;
                            SwitchFX = true;
                            AutoFisico1.Encendido();
                            InGame = true;
                        }
                        if (Input.keyPressed(Key.D2))
                        {
                            juegoDoble = true;
                            SwitchInicio = 4;
                            //SwitchMusica = true;
                            SwitchFX = true;
                            SwitchCamara = 3;
                            AutoFisico1.Encendido();
                            AutoFisico2.Encendido();
                            InGame = true;

                        }
                        break;
                    }
                case 2:
                    {
                        Hud.PantallaControles();
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
                        AutoFisico1.FXActivado = SwitchFX;

                        Invisible.PreRender(invisibilidadActivada);

                        Plaza.RenderAll();
                        AutoFisico1.Render(ElapsedTime);
                        GrupoPolicias.Render(ElapsedTime);
                        Cielo.Render();

                        Invisible.PostRender(invisibilidadActivada, Tiempo);

                        Hud.Juego(invisibilidadActivada, JugadorActivo, juegoDoble, pantallaDoble, AutoFisico1, AutoFisico2);

                        if (AutoFisico1.Vida < 0)
                        {
                            TiempoFinal = Tiempo;
                            Sonidos.SuenaGameOver();
                            SwitchInicio = 5;
                        }

                        if (Input.keyDown(Key.F10))
                        {
                            Hud.Pausar();
                        }
                        EnvMap.Render(AutoFisico1,AutoFisico2,GrupoPolicias,Camara,juegoDoble);

                        Hud.Tiempo(FastMath.Floor(Tiempo));
                        break;
                    }
                case 4:
                    {
                        Tiempo += ElapsedTime;
                        AutoFisico1.ElapsedTime = ElapsedTime;
                        AutoFisico2.ElapsedTime = ElapsedTime;
                        AutoFisico1.FXActivado = SwitchFX;
                        AutoFisico2.FXActivado = SwitchFX;

                        Invisible.PreRender(invisibilidadActivada);

                        DrawText.drawText("Velocidad P1:" + AutoFisico1.Velocidad, 0, 90, Color.Green);


                        Plaza.RenderAll();
                        AutoFisico1.Render(ElapsedTime);
                        AutoFisico2.Render(ElapsedTime);
                        GrupoPolicias.Render(ElapsedTime);
                        Cielo.Render();

                        Invisible.PostRender(invisibilidadActivada, Tiempo);

                        Hud.Juego(invisibilidadActivada, JugadorActivo, juegoDoble, pantallaDoble, AutoFisico1, AutoFisico2);
                        if (AutoFisico1.Vida < 0)
                        {
                            Hud.GanoJ2();
                            SwitchCamara = 2;
                            Sonidos.SuenaAplausos();
                            InGame = false;
                        }
                        if (AutoFisico2.Vida < 0)
                        {
                            Hud.GanoJ1();
                            SwitchCamara = 1;
                            Sonidos.SuenaAplausos();
                            InGame = false;
                        }

                        if (Input.keyDown(Key.F10))
                        {
                            Hud.Pausar();
                        }

                        if (Input.keyDown(Key.F10))
                        {
                            Hud.Pausar();
                        }

                        EnvMap.Render(AutoFisico1, AutoFisico2, GrupoPolicias,Camara,juegoDoble);

                        Hud.Tiempo(FastMath.Floor(Tiempo));
                        break;
                    }
                case 5:
                    {
                        SwitchFX = false;
                        SwitchMusica = false;
                        InGame = false;
                        Hud.JuegoTerminado();
                        Hud.TiempoFinal(FastMath.Floor(TiempoFinal));
                        if (Input.keyPressed(Key.M))
                        {
                            SwitchInicio = 1;
                        }
                        break;
                    }
            }

            PostRender();
        }

        public override void Dispose()
        {
            Plaza.DisposeAll();
            AutoFisico1.Dispose();
            GrupoPolicias.Dispose();
            Cielo.Dispose();
            Sonidos.Dispose();
            Hud.Dispose();
            Invisible.Dispose();
        }

    }

}