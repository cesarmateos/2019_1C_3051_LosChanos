using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Shaders;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using System.Collections.Generic;
using TGC.Core.BulletPhysics;
using TGC.Core.Textures;
using TGC.Core.Terrain;
using TGC.Core.Particle;

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

        private FisicaMundo Fisica;
        private TgcSkyBox Cielo;

        private AutoManejable JugadorActivo { get; set; }

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

        //public Microsoft.DirectX.Direct3D.Effect Parallax;

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

            Cielo.SkyEpsilon = 25f;
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
            AutoFisico2 = new AutoManejable(MayasAutoFisico, Rueda, new TGCVector3(0, 0, 200),270,Fisica,SombraAuto1,PathHumo);
            AutoFisico2.ConfigurarTeclas(Key.UpArrow, Key.DownArrow, Key.RightArrow, Key.LeftArrow, Key.RightControl, Key.Space);
            Policia01 = new AutoIA(MayasIA, Rueda, new TGCVector3(2000, 0, 1000), 270, Fisica, SombraAuto1, PathHumo, AutoFisico1);
            Policia02 = new AutoIA(MayasIA, Rueda, new TGCVector3(1000, 0, 1000), 270, Fisica, SombraAuto1, PathHumo, AutoFisico1);
            Policia03 = new AutoIA(MayasIA, Rueda, new TGCVector3(1000, 0, 2000), 270, Fisica, SombraAuto1, PathHumo, AutoFisico1);
            Policia04 = new AutoIA(MayasIA, Rueda, new TGCVector3(-1000, 0, 1000), 270, Fisica, SombraAuto1, PathHumo, AutoFisico1);
            Policia05 = new AutoIA(MayasIA, Rueda, new TGCVector3(1000, 0, -1000), 270, Fisica, SombraAuto1, PathHumo, AutoFisico1);

            //Hud
            Huds = new Drawer2D();
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
            VelocimetroAguja.RotationCenter = new TGCVector2(127.5f, 127.5f);

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
            Policia01.Moverse();
        }

        public override void Update()
        {
            PreUpdate();
            //Obtenemos acceso al objeto que maneja input de mouse y teclado del framework
            var input = Input;
            Policia01.Moverse();
            Policia02.Moverse();
            Policia03.Moverse();
            Policia04.Moverse();
            Policia05.Moverse();
            AutoFisico1.Update(input);
            AutoFisico2.Update(input);
            Camara = new CamaraAtrasAF(AutoFisico1);
            JugadorActivo = AutoFisico1;


            //Selección de Cámaras. (FALTA TERMINAR).
            if (input.keyDown(Key.D1))
            {
                Camara = new CamaraAtrasAF(AutoFisico1);
                JugadorActivo = AutoFisico1;
            }
            else if (input.keyDown(Key.D2))
            {
                Camara = new CamaraAerea(AutoFisico2.Mayas[1].Position);
                JugadorActivo = AutoFisico2;
            }
            else if (input.keyDown(Key.D3))
            {
                Camara = new CamaraAerea(AutoFisico1.Mayas[1].Position);
                JugadorActivo = AutoFisico1;
            }
            else if (input.keyDown(Key.D4))
            {
                Camara = new CamaraAtrasAF(AutoFisico2);
                JugadorActivo = AutoFisico2;
            }
            else if (input.keyDown(Key.D5))
            {
                Camara = new CamaraFija();
            }
            else
            {
                JugadorActivo = AutoFisico1;
            }

            PostUpdate();
        }

        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.

            VelocimetroAguja.Rotation = JugadorActivo.Velocidad / 100;

            PreRender();

            //Permito las particulas
            D3DDevice.Instance.ParticlesEnabled = true;
            D3DDevice.Instance.EnableParticles();


            //Textos en pantalla.
            DrawText.drawText("Dirección en X :" + AutoFisico1.DireccionInicial.X, 0, 20, Color.OrangeRed);
            DrawText.drawText("Dirección en Z :" + AutoFisico1.DireccionInicial.Z, 0, 30, Color.OrangeRed);
            DrawText.drawText("Posición en X :" + AutoFisico1.CuerpoRigidoAuto.CenterOfMassPosition.X, 0, 50, Color.Green);
            DrawText.drawText("Mantega el botón 2 para ver cámara aérea.", 0, 100, Color.White);
            DrawText.drawText("Mantega el botón 3 para ver cámara PERSEGUIDOR.", 0, 115, Color.White);

            DrawText.drawText("ACELERA :                     FLECHA ARRIBA", 1000, 10, Color.Black);
            DrawText.drawText("DOBLA DERECHA :           FLECHA DERECHA", 1000, 25, Color.Black);
            DrawText.drawText("DOBLA IZQUIERDA :         FLECHA IZQUIERDA", 1000, 40, Color.Black);
            DrawText.drawText("MARCHA ATRÁS :            FLECHA ABAJO", 1000, 60, Color.Black);
            DrawText.drawText("FRENO :                        CONTROL DERECHO", 1000, 80, Color.Black);
            DrawText.drawText("SALTAR :                     BARRA ESPACIADORA", 1000, 100, Color.Black);

            DrawText.drawText("ACELERA :                    W", 1500, 10, Color.Black);
            DrawText.drawText("DOBLA DERECHA :           D", 1500, 25, Color.Black);
            DrawText.drawText("DOBLA IZQUIERDA :         A", 1500, 40, Color.Black);
            DrawText.drawText("MARCHA ATRÁS :            S", 1500, 60, Color.Black);
            DrawText.drawText("FRENO :                        CONTROL IZQUIERDO", 1500, 80, Color.Black);
            DrawText.drawText("SALTAR :                     TAB", 1500, 100, Color.Black);


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

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            //Huds.DrawSprite(PantallaInicioFondo);
            //Huds.DrawSprite(PantallaInicioChanos);
            //Huds.DrawSprite(PantallaInicioJugar);
            //Huds.DrawSprite(PantallaInicioControles);
            Huds.DrawSprite(VelocimetroFondo);
            Huds.DrawSprite(VelocimetroAguja);

            //Finalizar el dibujado de Sprites
            Huds.EndDrawSprite();


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
        }
    }
}
