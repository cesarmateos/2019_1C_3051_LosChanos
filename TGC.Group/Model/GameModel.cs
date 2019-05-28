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

        //Objetos viejos


        //Objetos nuevos
        private TgcScene Plaza { get; set; }
        private TgcScene Auto1 { get; set; }
        private TgcScene Auto2 { get; set; }
        private TgcMesh Rueda { get; set; }

        private AutoManejable Jugador1 { get; set; }
        private AutoManejable Perseguidor { get; set; }

        private FisicasEdificios Edificios { get; set; }

        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            //Objetos

            Plaza = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Plaza-TgcScene.xml");
            Auto1 = new TgcSceneLoader().loadSceneFromFile(MediaDir + "AutoPolicia-TgcScene.xml");
            Auto2 = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Auto2-TgcScene.xml");
            Rueda = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Rueda-TgcScene.xml").Meshes[0];

            //Edificios = new ColisionesEdificios();
            //Edificios = FisicasEdificios.Init(this);


            Jugador1 = new AutoManejable(Auto1, Rueda, new TGCVector3(0, 0, 0), FastMath.ToRad(270), new TGCVector3(-26, 10.5f, -45f), new TGCVector3(26, 10.5f, -45f), new TGCVector3(-26, 10.5f, 44), new TGCVector3(26, 10.5f, 44));
            Perseguidor = new AutoManejable(Auto2, Rueda, new TGCVector3(0, 0, 0), FastMath.ToRad(270), new TGCVector3(-26, 10.5f, -45f), new TGCVector3(26, 10.5f, -45f), new TGCVector3(-26, 10.5f, 44), new TGCVector3(26, 10.5f, 44));
        }

        public override void Update()
        {
            PreUpdate();
            //Obtenemos acceso al objeto que maneja input de mouse y teclado del framework
            var input = Input;
            Camara = new CamaraAtras(Jugador1);


            //Selección de Cámaras. (FALTA TERMINAR).
            if (input.keyDown(Key.D1))
            {
                Camara = new CamaraAtras(Jugador1);
            }
            else if (input.keyDown(Key.D2))
            {
                Camara = new CamaraAerea(Auto1.Meshes[0]);
            }
            else if (input.keyDown(Key.D3))
            {
                Camara = new CamaraFija();
            }

            //Movimiento del Automotor 1.
            if (input.keyDown(Key.Left))
            {
                Jugador1.GiraIzquierda();
            }
            else if (input.keyDown(Key.Right))
            {
                Jugador1.GiraDerecha();
            }
            else
            {
                Jugador1.NoGira();
            }

            if (input.keyDown(Key.Up))
            {
                Jugador1.Acelera();

            }
            else if (input.keyDown(Key.Down))
            {
                Jugador1.MarchaAtras();
            }
            else
            {
                Jugador1.Parado();
            }

            if (input.keyDown(Key.RightControl))
            {
                Jugador1.Frena();
            }

            if (input.keyPressed(Key.Space))
            {
                Jugador1.Salta();
            }

            Jugador1.ElapsedTime = ElapsedTime;
            Jugador1.Moverse();
            Jugador1.EfectoGravedad();



            //Movimiento del Perseguidor. (PRIMER ACERCAMIENTO A OTRO AUTO - FALTA  PONER IA, POR AHORA SE MUEVE MANUALMENTE)
            if (input.keyDown(Key.A))
            {
                Perseguidor.GiraIzquierda();
            }
            else if (input.keyDown(Key.D))
            {
                Perseguidor.GiraDerecha();
            }
            else
            {
                Perseguidor.NoGira();
            }

            if (input.keyDown(Key.W))
            {
                Perseguidor.Acelera();

            }
            else if (input.keyDown(Key.S))
            {
                Perseguidor.MarchaAtras();
            }
            else
            {
                Perseguidor.Parado();
            }

            if (input.keyDown(Key.LeftControl))
            {
                Perseguidor.Frena();
            }

            if (input.keyPressed(Key.Tab))
            {
                Perseguidor.Salta();
            }

            Perseguidor.ElapsedTime = ElapsedTime;
            Perseguidor.Moverse();
            Perseguidor.EfectoGravedad();


            PostUpdate();
        }

        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            //Textos en pantalla.
            DrawText.drawText("Dirección en X :" + Jugador1.VersorDirector().X, 0, 20, Color.OrangeRed);
            DrawText.drawText("Dirección en Z :" + Jugador1.VersorDirector().Z, 0, 30, Color.OrangeRed);
            DrawText.drawText("Posición en X :" + Jugador1.Automovil.Meshes[0].Position.X, 0, 50, Color.Green);
            DrawText.drawText("Posición en Z :" + Jugador1.Automovil.Meshes[0].Position.Z, 0, 60, Color.Green);
            DrawText.drawText("Velocidad en X :" + Jugador1.Velocidad * 8 + "Km/h", 0, 80, Color.Yellow);
            DrawText.drawText("Mantega el botón 2 para ver cámara aérea.", 0, 100, Color.White);
            DrawText.drawText("Mantega el botón 3 para ver cámara aérea fija.", 0, 115, Color.White);

            DrawText.drawText("ACELERA :                     FLECHA ARRIBA", 1500, 10, Color.Black);
            DrawText.drawText("DOBLA DERECHA :           FLECHA DERECHA", 1500, 25, Color.Black);
            DrawText.drawText("DOBLA IZQUIERDA :         FLECHA IZQUIERDA", 1500, 40, Color.Black);
            DrawText.drawText("MARCHA ATRÁS :            FLECHA ABAJO", 1500, 60, Color.Black);
            DrawText.drawText("FRENO :                        CONTROL DERECHO", 1500, 80, Color.Black);
            DrawText.drawText("SALTAR :                     BARRA ESPACIADORA", 1500, 100, Color.Black);


            Plaza.RenderAll();
            Jugador1.RenderAll();
            Perseguidor.RenderAll();

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }


        public override void Dispose()
        {
 
            Jugador1.DisposeAll();
            Perseguidor.RenderAll();
            Plaza.DisposeAll();
        }
    }
}
