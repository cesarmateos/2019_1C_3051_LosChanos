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

        //Objetos
        private TgcMesh Piso { get; set; }
        private TgcMesh Automotor { get; set; }
        private TGCBox Box { get; set; }


        //Cosas del Auto
        public float velocidad = 0;
        public float aceleracion = 0;
        public float grados = 0;
        public TGCVector3 direccion = new TGCVector3(0, 0, 0);

        //Camaras
        private TgcCamera camaraAerea;
        private TgcCamera camaraAtras;

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        /// 
        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            //Cosas de la Caja.
            var pathTexturaCaja = MediaDir + Game.Default.TexturaCaja;
            TgcTexture texture = TgcTexture.createTexture(pathTexturaCaja);
            TGCVector3 tamanioCaja = new TGCVector3(80, 80, 100);
            //No sé por qué la caja sigue en 0,0,0
            TGCVector3 posicionCaja = new TGCVector3(40, 40, -400);
            Box = TGCBox.fromSize(posicionCaja, tamanioCaja, texture);

            //Objetos
            Automotor = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Auto-TgcScene.xml").Meshes[0];
            Piso = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Piso-TgcScene.xml").Meshes[0];
        }


        public override void Update()
        {
            PreUpdate();
            //Obtenemos acceso al objeto que maneja input de mouse y teclado del framework
            var input = Input;

            //Cosas del Automotor.
            var tiempoBotonApretado = 0.0f;
            var rozamiento = 1.8f;
            var gradosGiro = 0.017f;
            var giroTotal = gradosGiro * (velocidad / 10);

            //Cosas de Cámaras.
            var posicionCamaraArea = new TGCVector3(50, 2900, 0);
            var objetivoCamaraAerea = TGCVector3.Empty;
            camaraAerea = new TgcCamera();
            camaraAerea.SetCamera(posicionCamaraArea, objetivoCamaraAerea);
            camaraAtras = new TgcCamera();
            var distanciaCamaraAtras = 200;
            var alturaCamaraAtras = 50;
            var lambda = distanciaCamaraAtras / FastMath.Sqrt((FastMath.Pow2(direccion.X)) + FastMath.Pow2(direccion.Z));
            var posicionCamaraAtras = new TGCVector3(Automotor.Position.X -(lambda * direccion.X),alturaCamaraAtras, Automotor.Position.Z - (lambda * direccion.Z));           
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
            else
            {
                Camara = camaraAtras;
            }
           

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
                aceleracion += 0.03f;
                tiempoBotonApretado = ElapsedTime;


            }
            else if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            {
                aceleracion -= 0.05f;
                tiempoBotonApretado = ElapsedTime;

            }
            else
            {
                Automotor.RotateY(0);
                aceleracion = 0;
            }

            //Los grados están en RADIANES
            direccion.X = FastMath.Cos(4.71238898f + grados);
            direccion.Z = FastMath.Sin(4.71238898f + grados);
            var velocidadMinima = 0;
            var velocidadMaxima = 10;
            velocidad = FastMath.Min(FastMath.Max((velocidad + (aceleracion * tiempoBotonApretado) - (rozamiento * ElapsedTime)), velocidadMinima), velocidadMaxima);
            Automotor.Move(direccion * velocidad);

            PostUpdate();
        }

        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            //Textos en pantalla.
            DrawText.drawText("Dirección en X :" + direccion.X, 0, 20, Color.OrangeRed);
            DrawText.drawText("Dirección en Z :" + direccion.Z, 0, 30, Color.OrangeRed);
            DrawText.drawText("Posición en X :" + Automotor.Position.X, 0, 50, Color.Green);
            DrawText.drawText("Posición en Z :" + Automotor.Position.Z, 0, 60, Color.Green);
            DrawText.drawText("Velocidad en X :" + velocidad + "Km/h", 0, 80, Color.Yellow);
            DrawText.drawText("Mantega el botón 2 para ver cámara área.", 0, 100, Color.White);

            //Render Objetos.
            Automotor.Render();
            Piso.Render();
            Box.Render();

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }


        public override void Dispose()
        {
            Box.Dispose();
            Automotor.Dispose();
            Piso.Dispose();
        }
    }
}