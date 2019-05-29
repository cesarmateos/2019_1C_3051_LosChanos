using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using System.Collections.Generic;

namespace TGC.Group.Model
{
    public class AutoManejable
    {
        public List<TgcMesh> Ruedas { get; set; }

        //Carrocería del auto
        public TgcScene Automovil { get; set; }

        //Variables de Mayas de Ruedas
        public TgcMesh RuedaDelIzq { get; set; }
        public TgcMesh RuedaDelDer { get; set; }
        public TgcMesh RuedaTrasIzq { get; set; }
        public TgcMesh RuedaTrasDer { get; set; }

        //Variables de posiciones de las Ruedas
        public TGCVector3 PosicionRuedaDelDer { get; set; }
        public TGCVector3 PosicionRuedaDelIzq { get; set; }
        public TGCVector3 PosicionRuedaTrasDer { get; set; }
        public TGCVector3 PosicionRuedaTrasIzq { get; set; }

        public AutoManejable(TgcScene auto, TgcMesh rueda, TGCVector3 posicionInicial, float direccionInicial, TGCVector3 posicionRuedaDelanteraDerecha, TGCVector3 posicionRuedaDelanteraIzquierda, TGCVector3 posicionRuedaTraseraDerecha, TGCVector3 posicionRuedaTraseraIzquierda)
        {
            PosicionInicio = posicionInicial;

            //Asignamos carrocería
            Automovil = auto;

            //Creamos las instancias de cada rueda
            RuedaTrasIzq = rueda.createMeshInstance("Rueda Trasera Izquierda");
            RuedaDelIzq = rueda.createMeshInstance("Rueda Delantera Izquierda");
            RuedaTrasDer = rueda.createMeshInstance("Rueda Trasera Derecha");
            RuedaDelDer = rueda.createMeshInstance("Rueda Delantera Derecha");

            //Asginamos la posición de cada rueda
            PosicionRuedaDelDer = posicionRuedaDelanteraDerecha;
            PosicionRuedaDelIzq = posicionRuedaDelanteraIzquierda;
            PosicionRuedaTrasDer = posicionRuedaTraseraDerecha;
            PosicionRuedaTrasIzq = posicionRuedaTraseraIzquierda;

            //Asignamos la dirrección a la cual va a apuntar el auto.
            Grados = DireccionInicial - direccionInicial;

            //Armo una lista con las ruedas
            Ruedas = new List<TgcMesh>
            {
                RuedaTrasIzq,
                RuedaDelIzq,
                RuedaTrasDer,
                RuedaDelDer
            };

            //Pongo el false el AutoTransform de todas las mayas.
            rueda.AutoTransformEnable = false;
            foreach (var maya in Automovil.Meshes)
            {
                maya.AutoTransformEnable = false;
                maya.Position = posicionInicial; // Asigno posición Inicial del auto
            }
        }

        public TGCVector3 PosicionInicio { get; set; }

        // Cosas de la Velocidad
        private float Aceleracion { get; set; }
        public float VelocidadInicial { get; set; }
        public float velocidadMinima = -2;
        public float velocidadMaxima = 25;
        public bool VelocidadesCriticas { get => Velocidad < 0.05f && Velocidad > -0.05f; }
        private float velocidad; 

        public float Velocidad
        {
            get => FastMath.Min(FastMath.Max(VelocidadInicial + Aceleracion * Direccion, velocidadMinima), velocidadMaxima);
            set => velocidad = value;
        }

        //Cosas de los Giros
        private readonly float DireccionInicial = FastMath.ToRad(270);
        public int Direccion { get; set; }
        public float gradosGiro = FastMath.ToRad(0.4f);
        public float Grados { get; set; }
        public float GradosRuedaAlDoblar { get; set; }

        public TGCVector3 VersorDirector()
        {
            return new TGCVector3(FastMath.Cos(DireccionInicial + Grados), 0, FastMath.Sin(DireccionInicial + Grados));
        }

        public float GiroTotal()
        {
            if (Velocidad != 0)
            {
                return (gradosGiro * Direccion);
            }
            else
            {
                return 0;
            }

        }

        //Cosas del Salto
        public float alturaMaxima = 12;
        public float velocidadSalto = 3.5f;
        private int DireccionSalto { get; set; }
        public float ElapsedTime { get; set; }
        public float Gravedad { get; set; }
        public float Altura { get; set; }
        public bool AlturasCriticas { get => Altura < 0.7f && Altura > -0.7f; }

        public void EfectoGravedad()
        {
            Altura += Gravedad;
            if (Gravedad != 0)
            {
                Gravedad -= 3f * ElapsedTime;
                if (AlturasCriticas && Gravedad < 0)
                {
                    Gravedad = 0;
                }

            }

        }


        //MOVIMIENTOS
        public void Acelera()
        {
            if (Velocidad >= 0)
            {
                Aceleracion += 0.02f;
                Direccion = 1;
            }
        }

        public void Frena()
        {
            if (VelocidadesCriticas)
            {
                this.Parado();
            }
            else
            {
                Aceleracion -= 0.1f;
            }
        }

        public void MarchaAtras()
        {
            if (Velocidad <= 0)
            {
                Aceleracion += 0.02f;
                Direccion = -1;
            }
            else
            {
                this.Parado();
            }
        }

        public void GiraDerecha()
        {
            Grados -= GiroTotal();
            GradosRuedaAlDoblar = FastMath.Min(GradosRuedaAlDoblar + 0.04f, 0.7f);
        }

        public void GiraIzquierda()
        {
            Grados += GiroTotal();
            GradosRuedaAlDoblar = FastMath.Max(GradosRuedaAlDoblar - 0.04f, -0.7f);
        }

        public void NoGira()
        {
            GradosRuedaAlDoblar = 0;
        }

        public void Parado()  // Para poder ir para atrás o para adelante hay que estar parado, de lo contrario romperías la caja de cambios.
        {
            VelocidadInicial = Velocidad;
            Aceleracion = 0;
            if (Velocidad != 0)
            {
                if (VelocidadesCriticas)
                {
                    VelocidadInicial = 0;
                }
                else
                {
                    Aceleracion -= 0.008f;
                }
            }
        }

        public void Salta()
        {
            if (AlturasCriticas)
            {
                Gravedad = 1.4f;
            }
        }



        //Transformaciones comunes para todas las mayas del auto.
        public TGCMatrix Traslacion { get => TGCMatrix.Translation(VersorDirector().X * Velocidad, Gravedad, VersorDirector().Z * Velocidad); }
        public TGCMatrix Rotacion { get => TGCMatrix.RotationY(-Grados); }
        public TGCMatrix Movimiento { get => Rotacion * TraslacionInicial * TraslacionAcumulada; }
        public TGCMatrix TraslacionInicial { get => TGCMatrix.Translation(PosicionInicio); }
        public TGCMatrix TraslacionAcumulada = TGCMatrix.Identity;

        //Matrices que colocan a las ruedas en su lugar
        public TGCMatrix TraslacionRuedaTrasDer { get => TGCMatrix.Translation(PosicionRuedaTrasDer); }
        public TGCMatrix TraslacionRuedaDelDer { get => TGCMatrix.Translation(PosicionRuedaDelDer); }
        public TGCMatrix TraslacionRuedaTrasIzq { get => TGCMatrix.Translation(PosicionRuedaTrasIzq); }
        public TGCMatrix TraslacionRuedaDelIzq { get => TGCMatrix.Translation(PosicionRuedaDelIzq); }

        //Matriz que hace rotar a las ruedas al doblar
        public TGCMatrix RotarRueda { get => TGCMatrix.RotationY(GradosRuedaAlDoblar); }

        //Matriz que rota las rueda izquierda, para que quede como una rueda derecha
        public TGCMatrix FlipRuedaDerecha { get => TGCMatrix.RotationZ(FastMath.ToRad(180)); }

        //Matrices que hacen girar a las ruedas con la velocidad
        public TGCMatrix GiroAcumuladoIzq = TGCMatrix.Identity;
        public TGCMatrix GiroAcumuladoDer = TGCMatrix.Identity;
        public TGCMatrix GirarRuedaIzq { get => TGCMatrix.RotationX(-Velocidad / 6); }
        public TGCMatrix GirarRuedaDer { get => TGCMatrix.RotationX(Velocidad / 6); }


        public void Moverse()
        {
            //Matrices que acumulan los cambios
            TraslacionAcumulada *= Traslacion;
            GiroAcumuladoIzq *= GirarRuedaIzq;
            GiroAcumuladoDer *= GirarRuedaDer;

            //Transformaciones de las ruedas
            RuedaTrasIzq.Transform = GiroAcumuladoIzq * TraslacionRuedaTrasIzq * Movimiento;
            RuedaTrasDer.Transform = GiroAcumuladoDer * FlipRuedaDerecha * TraslacionRuedaTrasDer * Movimiento;
            RuedaDelIzq.Transform = GiroAcumuladoIzq * RotarRueda * TraslacionRuedaDelIzq * Movimiento;
            RuedaDelDer.Transform = GiroAcumuladoDer * FlipRuedaDerecha * RotarRueda * TraslacionRuedaDelDer * Movimiento;

            //Transformaciones de las piezas de la carrocería
            foreach (var maya in Automovil.Meshes)
            {
                maya.Position += (VersorDirector() * Velocidad);
                maya.Transform = Movimiento;
            }

        }

        public void RenderAll()
        {
            foreach (var maya in Ruedas)
            {
                maya.Render();
            }
            Automovil.RenderAll();
        }

        public void DisposeAll()
        {
            foreach (var maya in Ruedas)
            {
                maya.Dispose();
            }
            Automovil.DisposeAll();
        }
    }

}
