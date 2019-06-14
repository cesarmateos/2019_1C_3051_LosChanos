using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.BulletPhysics;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
using TGC.Core.Terrain;
using TGC.Core.Textures;

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
        private List<TgcMesh> MayasAutoFisico { get; set; }
        private AutoManejable AutoFisico1 { get; set; }
        private TgcTexture SombraAuto1 { get; set; }
        private List<TgcMesh> MayasIA{ get; set; }
        private AutoManejable AutoFisico2 { get; set; }
        private AutoIA Policia01 { get; set; }
        private AutoIA Policia02 { get; set; }
        private AutoIA Policia03 { get; set; }
        private AutoIA Policia04 { get; set; }
        private AutoIA Policia05 { get; set; }

        //Declaro Cosas de HUD
        private CustomSprite VelocimetroFondo;
        private CustomSprite VelocimetroAguja;
        private float EscalaVelocimetro;
        private Drawer2D Huds;
        private Drawer2D Inicio;

        // Fisica del Mundo 
        private FisicaMundo Fisica;
        private TgcSkyBox Cielo;


        //Camaras
        private AutoManejable JugadorActivo { get; set; }
        private CamaraAtrasAF Camara01 { get; set; }
        private CamaraEspectador Camara02 { get; set; }
        private CamaraAtrasAF Camara03 { get; set; }

        // Declaro Emisor de particulas
        public string PathHumo { get; set; }

        //Declaro Imagenes del Inicio
        private CustomSprite PantallaInicioFondo;
        private CustomSprite PantallaInicioChanos;
        private CustomSprite PantallaInicioControles;
        private CustomSprite PantallaInicioJugar;
        private CustomSprite PantallaInicioControlesMenu;
        private float EscalaInicioAltura;
        private float EscalaInicioAncho;

        //Sonido
        private TgcStaticSound Musica;
        private TgcStaticSound Tribuna;
        private Tgc3dSound Encendido;

        //public Microsoft.DirectX.Direct3D.Effect Parallax;
        int SwitchMusica { get; set; }
        int SwitchFX { get; set; }
        int SwitchInicio { get; set; }
        int SwitchCamara { get; set; }

        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            //Objetos

            Plaza = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Plaza-TgcScene.xml");
            MayasIA= new TgcSceneLoader().loadSceneFromFile(MediaDir + "AutoPolicia-TgcScene.xml").Meshes;
            MayasAutoFisico = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Auto2-TgcScene.xml").Meshes;
            Rueda = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Rueda-TgcScene.xml").Meshes[0];
            SombraAuto1 = TgcTexture.createTexture(MediaDir + "Textures\\SombraAuto.png");
            PathHumo = MediaDir + "Textures\\TexturaHumo.png";
            //Parallax = TGCShaders.Instance.LoadEffect(ShadersDir + "Parallax.fx");

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
            AutoFisico1 = new AutoManejable(MayasAutoFisico, Rueda, new TGCVector3(-52, 0, 425),270,Fisica,SombraAuto1,PathHumo);
            AutoFisico1.ConfigurarTeclas(Key.W, Key.S, Key.D, Key.A, Key.LeftControl, Key.Tab);
            AutoFisico1.Media = MediaDir; // Le paso el MediaDir
            AutoFisico2 = new AutoManejable(MayasAutoFisico, Rueda, new TGCVector3(0, 0, 200),270,Fisica,SombraAuto1,PathHumo);
            AutoFisico2.ConfigurarTeclas(Key.UpArrow, Key.DownArrow, Key.RightArrow, Key.LeftArrow, Key.RightControl, Key.Space);
            AutoFisico2.Media = MediaDir; // Le paso el MediaDir
            Policia01 = new AutoIA(MayasIA, Rueda, new TGCVector3(2000, 0, 1000), 270, Fisica, SombraAuto1, PathHumo, AutoFisico1);
            Policia02 = new AutoIA(MayasIA, Rueda, new TGCVector3(1000, 0, 1000), 270, Fisica, SombraAuto1, PathHumo, AutoFisico1);
            Policia03 = new AutoIA(MayasIA, Rueda, new TGCVector3(1000, 0, 2000), 270, Fisica, SombraAuto1, PathHumo, AutoFisico1);
            Policia04 = new AutoIA(MayasIA, Rueda, new TGCVector3(-1000, 0, 1000), 270, Fisica, SombraAuto1, PathHumo, AutoFisico1);
            Policia05 = new AutoIA(MayasIA, Rueda, new TGCVector3(1000, 0, -1000), 270, Fisica, SombraAuto1, PathHumo, AutoFisico1);

            //Hud
            Huds = new Drawer2D();
            Inicio = new Drawer2D();
            VelocimetroFondo = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Sprites\\VelocimetroFondo.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(FastMath.Max(D3DDevice.Instance.Width * 0.82f, 0), FastMath.Max(D3DDevice.Instance.Height * 0.7f, 0))
            };
            VelocimetroAguja = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Sprites\\VelocimetroAguja.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(FastMath.Max(D3DDevice.Instance.Width * 0.82f, 0), FastMath.Max(D3DDevice.Instance.Height * 0.7f, 0))
            };
            EscalaVelocimetro = 0.25f * D3DDevice.Instance.Height / VelocimetroFondo.Bitmap.Size.Height;
            TGCVector2 escalaVelocimetroVector = new TGCVector2(EscalaVelocimetro, EscalaVelocimetro);
            VelocimetroFondo.Scaling = escalaVelocimetroVector;
            VelocimetroAguja.Scaling = escalaVelocimetroVector;
            VelocimetroAguja.RotationCenter = new TGCVector2(VelocimetroAguja.Bitmap.Size.Height * EscalaVelocimetro / 2, VelocimetroAguja.Bitmap.Size.Height * EscalaVelocimetro / 2);

            //Pantalla Inicio
            PantallaInicioFondo = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Sprites\\InicioFondo.jpg", D3DDevice.Instance.Device),
                Position = new TGCVector2(0,0)
            };

            PantallaInicioChanos = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Sprites\\InicioChanos.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            PantallaInicioControles = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Sprites\\InicioControles.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            PantallaInicioJugar = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Sprites\\InicioJugar.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            PantallaInicioControlesMenu = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Sprites\\InicioControlesMenu.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            EscalaInicioAltura = (float)D3DDevice.Instance.Height / (float)PantallaInicioFondo.Bitmap.Size.Height;
            EscalaInicioAncho = (float)D3DDevice.Instance.Width / (float)PantallaInicioFondo.Bitmap.Size.Width;
            TGCVector2 escalaInicio = new TGCVector2(EscalaInicioAltura, EscalaInicioAncho);
            PantallaInicioFondo.Scaling = escalaInicio;
            PantallaInicioChanos.Scaling = escalaInicio;
            PantallaInicioControles.Scaling = escalaInicio;
            PantallaInicioControlesMenu.Scaling = escalaInicio;
            PantallaInicioJugar.Scaling = escalaInicio;

            // Sonido
            // Ambiente
            int volumen1 = -500;  // RANGO DEL 0 AL -10000 (Silenciado al -10000)
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
            Encendido.play();

            SwitchInicio = 1;
            SwitchCamara = 1;
        }


        public override void Update()
        {
            PreUpdate();

            var input = Input;

            //Camaras
            Camara01 = new CamaraAtrasAF(AutoFisico1);
            Camara02 = new CamaraEspectador();
            Camara03 = new CamaraAtrasAF(AutoFisico2);
        
            Policia01.Moverse();
            Policia02.Moverse();
            Policia03.Moverse();
            Policia04.Moverse();
            Policia05.Moverse();
            AutoFisico1.Update(input);
            AutoFisico2.Update(input);

            switch (SwitchCamara)
            {
                case 1:
                    {
                        Camara = Camara01;
                        JugadorActivo = AutoFisico1;
                        if (input.keyPressed(Key.F6))
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
                        JugadorActivo = AutoFisico1;
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
                        JugadorActivo = AutoFisico1;
                        if (input.keyPressed(Key.F5))
                        {
                            SwitchCamara = 1;
                        }
                        else if (input.keyPressed(Key.F6))
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

            PostUpdate();
        }

        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            //playMusica = true;
            VelocimetroAguja.Rotation = JugadorActivo.Velocidad / 60;

            PreRender();
            ClearTextures();

            //Permito las particulas
            D3DDevice.Instance.ParticlesEnabled = true;
            D3DDevice.Instance.EnableParticles();

            switch (SwitchInicio)
            {
                case 1:
                    {
                        Inicio.BeginDrawSprite();
                        Inicio.DrawSprite(PantallaInicioFondo);
                        Inicio.DrawSprite(PantallaInicioChanos);
                        Inicio.DrawSprite(PantallaInicioJugar);
                        Inicio.DrawSprite(PantallaInicioControles);
                        Inicio.EndDrawSprite();
                        if (Input.keyPressed(Key.C))
                        {
                            SwitchInicio = 2;
                        }
                        if (Input.keyPressed(Key.J))
                        {
                            SwitchInicio = 3;
                            SwitchMusica = 1;
                            SwitchFX = 1;
                        }
                        break;
                    }
                case 2:
                    {
                        Inicio.BeginDrawSprite();
                        Inicio.DrawSprite(PantallaInicioFondo);
                        Inicio.DrawSprite(PantallaInicioControlesMenu);
                        Inicio.EndDrawSprite();
                        if (Input.keyPressed(Key.V))
                        {
                            SwitchInicio = 1;
                        }
                        break;
                    }
                case 3:
                    {

                        //Textos en pantalla.
                        DrawText.drawText("Dirección en X :" + AutoFisico1.DireccionInicial.X, 0, 20, Color.OrangeRed);
                        DrawText.drawText("Dirección en Z :" + AutoFisico1.DireccionInicial.Z, 0, 30, Color.OrangeRed);
                        DrawText.drawText("Velocidad P1:" + AutoFisico1.Velocidad, 0, 50, Color.Green);
                        DrawText.drawText("EscalaVelocimetro:" + EscalaVelocimetro, 0, 70, Color.Green);
                        DrawText.drawText("EscalaVelocimetro:" + VelocimetroAguja.Bitmap.Size.Height * EscalaVelocimetro / 2, 0, 90, Color.Green);
                        
                        Plaza.RenderAll();
                        AutoFisico1.Render(ElapsedTime);
                        AutoFisico2.Render(ElapsedTime);
                        Policia01.Render(ElapsedTime);
                        Policia02.Render(ElapsedTime);
                        Policia03.Render(ElapsedTime);
                        Policia04.Render(ElapsedTime);
                        Policia05.Render(ElapsedTime);
                        Cielo.Render();

                        //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
                        Huds.BeginDrawSprite();

                        if (Input.keyDown(Key.F10))
                        {
                            Huds.DrawSprite(PantallaInicioControlesMenu);
                        }



                        //Dibujar sprite (si hubiese mas, deberian ir todos aquí)

                        Huds.DrawSprite(VelocimetroFondo);
                        Huds.DrawSprite(VelocimetroAguja);

                        //Finalizar el dibujado de Sprites
                        Huds.EndDrawSprite();
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
            PantallaInicioChanos.Dispose();
            PantallaInicioControles.Dispose();
            PantallaInicioControlesMenu.Dispose();
            PantallaInicioJugar.Dispose();
        }
    }
}
