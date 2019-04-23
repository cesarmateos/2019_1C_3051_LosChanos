using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
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
        /*private TgcMesh Piso { get; set; }     // Resta definir si Estadio o Ciudad
        private TgcMesh Pared { get; set; }
        private TgcMesh Tribuna { get; set; }
        private TGCBox Box { get; set; }
        */

        //Objetos nuevos
        private TgcMesh Automotor { get; set; }
        private TgcScene Ciudad { get; set; }


        //Cosas del Auto
        public float velocidad = 0;
        public float aceleracion = 0;
        public float grados = 0;
        public TGCVector3 versorDirector = new TGCVector3(0, 0, 0);

        //Camaras
        private TgcCamera camaraAerea;
        private TgcCamera camaraAtras;
        private TgcCamera camaraAereaFija;


        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            //Objetos
            // Piso = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Piso-TgcScene.xml").Meshes[0];
            // Pared = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Pared-TgcScene.xml").Meshes[0];
            // Tribuna = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Tribuna-TgcScene.xml").Meshes[0];  Resta definir si Estadio o Ciudad
            Automotor = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Auto-TgcScene.xml").Meshes[0];
            Ciudad = new TgcSceneLoader().loadSceneFromFile(MediaDir + "escena tp-TgcScene.xml");
        }


        public override void Update()
        {
            PreUpdate();
            //Obtenemos acceso al objeto que maneja input de mouse y teclado del framework
            var input = Input;

            //Cosas del Automotor.
            var tiempoBotonApretado = 0.0f;
            var direccion = 1;
            var rozamiento = 0.005f;
            var gradosGiro = 0.017f;
            var giroTotal = gradosGiro * (velocidad / 10);


            //Movimiento del Automotor.
            if (input.keyDown(Key.Left) || input.keyDown(Key.A))
            {
                grados += giroTotal;
                Automotor.RotateY(-giroTotal);

            }
            else if (input.keyDown(Key.Right) || input.keyDown(Key.D))
            {
                grados -= giroTotal;
                Automotor.RotateY(+giroTotal);

            }
            if (input.keyDown(Key.Up) || input.keyDown(Key.W))
            {
                aceleracion += 0.02f;
                tiempoBotonApretado = ElapsedTime;
                direccion = 1;


            }
            else if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            {
                aceleracion -= 0.02f;
                tiempoBotonApretado = ElapsedTime;
                direccion = -1;

            }
            else
            {
                Automotor.RotateY(0);
                aceleracion = 0;
                rozamiento = 0.005f;
            }
            if (input.keyDown(Key.RightControl) || input.keyDown(Key.LeftControl))

            {
                rozamiento += 0.006f;
                tiempoBotonApretado = ElapsedTime;
                if (velocidad < 0.05f)
                {
                    velocidad = 0;
                }
            }


            //Los grados están en RADIANES
            versorDirector.X = FastMath.Cos(4.71238898f + grados);
            versorDirector.Z = FastMath.Sin(4.71238898f + grados);
            var velocidadMinima = -2;
            var velocidadMaxima = 13;
            velocidad = FastMath.Min(FastMath.Max((velocidad + (aceleracion * tiempoBotonApretado) - (rozamiento * velocidad)), velocidadMinima), velocidadMaxima);
            Automotor.Move(versorDirector * velocidad);


            //Cosas de Cámaras.
            var posicionCamaraArea = new TGCVector3(50, 2900, 0);
            var objetivoCamaraAerea = TGCVector3.Empty;

            camaraAerea = new TgcCamera();
            camaraAerea.SetCamera(posicionCamaraArea, objetivoCamaraAerea);
            camaraAtras = new TgcCamera(); // No implementada
            camaraAereaFija = new TgcCamera();
            camaraAereaFija.SetCamera(posicionCamaraArea, Automotor.Position);

            var distanciaCamaraAtras = 200;
            var alturaCamaraAtras = 50;
            var lambda = distanciaCamaraAtras / FastMath.Sqrt((FastMath.Pow2(versorDirector.X)) + FastMath.Pow2(versorDirector.Z));
            var posicionCamaraAtras = new TGCVector3(Automotor.Position.X - (lambda *direccion* versorDirector.X), alturaCamaraAtras, Automotor.Position.Z - (lambda *direccion* versorDirector.Z));
            camaraAtras.SetCamera(posicionCamaraAtras, Automotor.Position);

            //Selección de Cámaras. (FALTA TERMINAR).
            if (input.keyDown(Key.D1))
            {
                Camara = camaraAtras;
            }
            else if (input.keyDown(Key.D2))
            {
                Camara = camaraAerea;
            }
            else if (input.keyDown(Key.D3))
            {
                Camara = camaraAereaFija;
            }
            else
            {
                Camara = camaraAtras;
            }

            PostUpdate();
        }

        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            //Textos en pantalla.
            DrawText.drawText("Dirección en X :" + versorDirector.X, 0, 20, Color.OrangeRed);
            DrawText.drawText("Dirección en Z :" + versorDirector.Z, 0, 30, Color.OrangeRed);
            DrawText.drawText("Posición en X :" + Automotor.Position.X, 0, 50, Color.Green);
            DrawText.drawText("Posición en Z :" + Automotor.Position.Z, 0, 60, Color.Green);
            DrawText.drawText("Velocidad en X :" + velocidad * 15 + "Km/h", 0, 80, Color.Yellow);
            DrawText.drawText("Mantega el botón 2 para ver cámara aérea.", 0, 100, Color.White);
            DrawText.drawText("Mantega el botón 3 para ver cámara aérea fija.", 0, 115, Color.White);
            DrawText.drawText("ACELERA :                     FLECHA ARRIBA", 1500, 10, Color.Black);
            DrawText.drawText("DOBLA DERECHA :           FLECHA DERECHA", 1500, 25, Color.Black);
            DrawText.drawText("DOBLA IZQUIERDA :         FLECHA IZQUIERDA", 1500, 40, Color.Black);
            DrawText.drawText("MARCHA ATRÁS :            FLECHA ABAJO", 1500, 60, Color.Black);
            DrawText.drawText("FRENO :                        CONTROL DERECHO", 1500, 80, Color.Black);

            //Render Objetos.

            //Pared.Render();
            //Tribuna.Render();
            //Piso.Render();
            //Box.Render();

            Automotor.Render();
            Ciudad.RenderAll();

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }


        public override void Dispose()
        {
            //Box.Dispose();
            //Piso.Dispose();
            //Pared.Render();
            //Tribuna.Render();

            Automotor.Dispose();
            Ciudad.DisposeAll();
        }
    }
}