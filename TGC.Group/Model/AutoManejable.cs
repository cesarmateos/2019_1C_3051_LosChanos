using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using System.Collections.Generic;

namespace TGC.Group.Model
{
    public class AutoManejable
    {
        public List<TgcMesh> Mayas { get; set; }
        private TgcMesh automovil;
        public TgcMesh Automovil { get => automovil; set => automovil = value; }
       
        public TgcMesh RuedaDelIzq { get; set; }
        public TgcMesh RuedaDelDer { get; set; }
        public TgcMesh RuedaTrasIzq { get; set; }
        public TgcMesh RuedaTrasDer { get; set; }

        public AutoManejable(TgcMesh auto, TgcMesh rueda)
        {
            automovil = auto;
            RuedaTrasIzq = rueda.createMeshInstance("Rueda Trasera Izquierda");
            RuedaDelIzq = rueda.createMeshInstance("Rueda Delantera Izquierda");
            RuedaTrasDer = rueda.createMeshInstance("Rueda Trasera Derecha");
            RuedaDelDer = rueda.createMeshInstance("Rueda Delantera Derecha");

            Mayas = new List<TgcMesh>();

            Mayas.Add(automovil);
            Mayas.Add(RuedaTrasIzq);
            Mayas.Add(RuedaDelIzq);
            Mayas.Add(RuedaTrasDer);
            Mayas.Add(RuedaDelDer);
        
        }
        public float gradosGiro = FastMath.ToRad(0.7f);
        public float velocidadMinima = -2;
        public float velocidadMaxima = 13;
        public float altura;
        public float alturaMaxima = 12;
        private int DireccionSalto { get; set; }
        public float velocidadSalto = 3.5f;
        private float direccionInicial;
        public float DireccionInicial { get => FastMath.ToRad(270); set => direccionInicial = value; }
        private float Aceleracion { get; set; }
        public bool VelocidadesCriticas { get => Velocidad < 0.05f && Velocidad > -0.05f; }
        public int Direccion { get; set; }
        public float Grados { get; set; }
        public float VelocidadInicial { get; set; }
        private float velocidad;
        public float Velocidad
        {
            get => FastMath.Min(FastMath.Max(VelocidadInicial + Aceleracion * Direccion, velocidadMinima), velocidadMaxima);
            set => velocidad = value;
        }

        public TGCVector3 VersorDirector()
        {
            return new TGCVector3(FastMath.Cos(DireccionInicial + Grados), 0, FastMath.Sin(DireccionInicial + Grados));
        }

        public float GiroTotal()
        {
            return gradosGiro * (Velocidad / 10);
        }

        //Movimiento
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
        }
        public void GiraIzquierda()
        {
            Grados += GiroTotal();
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

        public float ElapsedTime { get; set; }
        public float Gravedad { get; set;}
        public float Altura { get; set; }
        public bool AlturasCriticas { get => Altura < 0.7f && Altura > -0.7f; }

        public void Salta()
        {
            if (AlturasCriticas)
            {
                Gravedad = 1.4f;
            }
        }
        public void EfectoGravedad()
        {
            Altura += Gravedad;
            if (Gravedad !=0)
            {
                Gravedad -= 3f * ElapsedTime;
                if (AlturasCriticas && Gravedad <0)
                {
                    Gravedad = 0;
                }
                
            }
            
        }

        public TGCMatrix Traslacion { get => TGCMatrix.Translation(VersorDirector().X * Velocidad, Gravedad, VersorDirector().Z * Velocidad); }
        public TGCMatrix Rotacion { get => TGCMatrix.RotationY(-Grados); }        
        public TGCMatrix Movimiento { get => Rotacion * TraslacionAcumulada; }        
        public TGCMatrix TraslacionAcumulada = TGCMatrix.Identity;
        public TGCMatrix GiroAcumuladoIzq = TGCMatrix.Identity;
        public TGCMatrix GiroAcumuladoDer = TGCMatrix.Identity;

        public TGCMatrix EscalarRuedaDelantera { get => TGCMatrix.Scaling(1.127f, 1.127f, 1.127f); }
        public TGCMatrix GirarRuedaIzq { get => TGCMatrix.RotationX(-Velocidad/4); }
        public TGCMatrix GirarRuedaDer { get => TGCMatrix.RotationX(Velocidad / 4); }
        public TGCMatrix RotarRueda { get => TGCMatrix.RotationY(Grados); }
        public TGCMatrix FlipRuedaDerecha { get => TGCMatrix.RotationZ(FastMath.ToRad(180)); }

        public TGCMatrix TraslacionRuedaTrasDer { get => TGCMatrix.Translation(new TGCVector3(-24.5f, 7.2f, 32)); }
        public TGCMatrix TraslacionRuedaDelDer { get => TGCMatrix.Translation(new TGCVector3(-25f, 7.4f, -31)); }
        public TGCMatrix TraslacionRuedaTrasIzq { get => TGCMatrix.Translation(new TGCVector3(22, 6.8f, 32)); }
        public TGCMatrix TraslacionRuedaDelIzq { get => TGCMatrix.Translation(new TGCVector3(22.5f, 7f, -31)); }

        public void Moverse()
        {
            
            TraslacionAcumulada *= Traslacion;
            GiroAcumuladoIzq *= GirarRuedaIzq;
            GiroAcumuladoDer *= GirarRuedaDer;
            Automovil.Position += (VersorDirector() * Velocidad);
            Automovil.Transform = Movimiento;
            RuedaTrasIzq.Transform = GiroAcumuladoIzq * TraslacionRuedaTrasIzq * Movimiento;
            RuedaTrasDer.Transform = GiroAcumuladoDer *FlipRuedaDerecha * TraslacionRuedaTrasDer * Movimiento;
            RuedaDelIzq.Transform = EscalarRuedaDelantera * GiroAcumuladoIzq * RotarRueda *  TraslacionRuedaDelIzq * Movimiento;
            RuedaDelDer.Transform = EscalarRuedaDelantera * GiroAcumuladoDer * FlipRuedaDerecha * RotarRueda * TraslacionRuedaDelDer * Movimiento;

        }
        public void RenderAll()
        {
            foreach (var maya in Mayas)
            {
                maya.Render();
            }
        }
        public void DisposeAll()
        {
            foreach (var maya in Mayas)
            {
                maya.Dispose();
            }
        }
    }
   
}
