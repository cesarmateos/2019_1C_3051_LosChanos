using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using System.Collections.Generic;
using BulletSharp;
using TGC.Core.BulletPhysics;
using TGC.Core.Textures;
using TGC.Core.Terrain;

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

        //Objetos nuevos
        private TgcScene Plaza { get; set; }
        private TgcMesh Rueda { get; set; }

        private List<TgcMesh> MayasAutoFisico1 { get; set; }
        private AutoFisico AutoFisico1 { get; set; }
        private TgcTexture SombraAuto1 { get; set; }
        private List<TgcMesh> MayasAutoFisico2 { get; set; }
        private AutoFisico AutoFisico2 { get; set; }
        // private EmisorDeParticulas Humito { get; set; }

        private FisicaMundo Fisica;
        private TgcSkyBox Cielo;


        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            //Objetos

            Plaza = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Plaza-TgcScene.xml");
            MayasAutoFisico1 = new TgcSceneLoader().loadSceneFromFile(MediaDir + "AutoPolicia-TgcScene.xml").Meshes;
            MayasAutoFisico2 = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Auto2-TgcScene.xml").Meshes;
            Rueda = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Rueda-TgcScene.xml").Meshes[0];
            SombraAuto1 = TgcTexture.createTexture(MediaDir + "Textures\\SombraAuto.png");
            //Humito = new EmisorDeParticulas(MediaDir, MediaDir, new TGCVector3(0, 0, 0),ElapsedTime);

            //Cielo                                                          (En una esquina del mapa el Frustum esta jodiendo)
            Cielo = new TgcSkyBox();
            Cielo.Center = TGCVector3.Empty;
            Cielo.Size = new TGCVector3(10000, 10000, 10000);
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

            AutoFisico1 = new AutoFisico(MayasAutoFisico1, Rueda, new TGCVector3(200, 0, 200), 270,Fisica,SombraAuto1);
            AutoFisico1.ConfigurarTeclas(Key.W, Key.S, Key.D, Key.A, Key.LeftControl, Key.Tab);
            AutoFisico2 = new AutoFisico(MayasAutoFisico2, Rueda, new TGCVector3(0, 0, 200), 270,Fisica,SombraAuto1);
            AutoFisico2.ConfigurarTeclas(Key.UpArrow, Key.DownArrow, Key.RightArrow, Key.LeftArrow, Key.RightControl, Key.Space);

        }

        public override void Update()
        {
            PreUpdate();
            //Obtenemos acceso al objeto que maneja input de mouse y teclado del framework
            var input = Input;

            AutoFisico1.Update(input);
            AutoFisico2.Update(input);
            Camara = new CamaraAtrasAF(AutoFisico1);


            //Selección de Cámaras. (FALTA TERMINAR).
            if (input.keyDown(Key.D1))
            {
                Camara = new CamaraAtrasAF(AutoFisico1);
            }
            else if (input.keyDown(Key.D2))
            {
                Camara = new CamaraAerea(AutoFisico2.Mayas[1].Position);
            }
            else if (input.keyDown(Key.D3))
            {
                Camara = new CamaraAerea(AutoFisico1.Mayas[1].Position);
            }
            else if (input.keyDown(Key.D4))
            {
                Camara = new CamaraAtrasAF(AutoFisico2);
            }

            PostUpdate();
        }

        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            //Textos en pantalla.
            DrawText.drawText("Dirección en X :" + AutoFisico1.DireccionInicial.X, 0, 20, Color.OrangeRed);
            DrawText.drawText("Dirección en Z :" + AutoFisico1.DireccionInicial.Z, 0, 30, Color.OrangeRed);
            DrawText.drawText("Posición en X :" + AutoFisico1.CuerpoRigidoAuto.CenterOfMassPosition.X, 0, 50, Color.Green);
            DrawText.drawText("Posición en Y :" + AutoFisico1.CuerpoRigidoAuto.CenterOfMassPosition.Y, 0, 60, Color.Green);
            DrawText.drawText("Posición en Z :" + AutoFisico1.CuerpoRigidoAuto.CenterOfMassPosition.Z, 0, 70, Color.Green);
            DrawText.drawText("Velocidad en X :" + AutoFisico1.CuerpoRigidoAuto.LinearVelocity + "Km/h", 0, 80, Color.Yellow);
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
            Cielo.Render();
           // Humito.Render();
            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }


        public override void Dispose()
        {
            Plaza.DisposeAll();
            AutoFisico1.Dispose();
            AutoFisico2.Dispose();
            Cielo.Dispose();
        }
    }
}
