using System;
using System.Collections.Generic;
using Microsoft.DirectX.DirectInput;
using BulletSharp;
using TGC.Core.BulletPhysics;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.Particle;


namespace TGC.Group.Model
{
    public class AutoFisico
    {
        public FisicaMundo Fisica { get; set; }

        //private TgcMesh MayaAuto { get; set; }
        public List<TgcMesh> Mayas { get; set; }
        public RigidBody CuerpoRigidoAuto { get; set; }
        public TGCVector3 PosicionInicial { get; set; }

        //Teclas
        private Key Acelerar { get; set; }
        private Key Atras { get; set; }
        private Key Derecha { get; set; }
        private Key Izquierda { get; set; }
        private Key Freno { get; set; }
        private Key Salto { get; set; }

        //Variables de Mayas de Ruedas
        public List<TgcMesh> Ruedas { get; set; }
        public TgcMesh RuedaDelIzq { get; set; }
        public TgcMesh RuedaDelDer { get; set; }
        public TgcMesh RuedaTrasIzq { get; set; }
        public TgcMesh RuedaTrasDer { get; set; }
        public TGCVector3 PosicionRuedaDelDer = new TGCVector3(-26, 10.5f, -45f);
        public TGCVector3 PosicionRuedaDelIzq = new TGCVector3(26, 10.5f, -45f);
        public TGCVector3 PosicionRuedaTrasDer = new TGCVector3(-26, 10.5f, 44);
        public TGCVector3 PosicionRuedaTrasIzq = new TGCVector3(26, 10.5f, 44);

        public float GradosRuedaAlDoblar { get; set; }
        public TGCVector3 VersorDirector { get; set; }

        //Cosas de Giros
        public int Direccion { get; set; }
        public TGCVector3 DireccionInicial { get; set; }
        public float GradosRotacion { get; set; }
        public float gradosGiro = FastMath.ToRad(0.4f);

        //Friccion del auto
        public float FriccionAuto { get; set; }

        //Calculo de la Velocidad del Auto
        public float Velocidad {
            get => FastMath.Abs(CuerpoRigidoAuto.LinearVelocity.X) + FastMath.Abs(CuerpoRigidoAuto.LinearVelocity.Y) + FastMath.Abs(CuerpoRigidoAuto.LinearVelocity.Z)*Direccion;
        }

        //Cosas del Salto
        public float FuerzaSalto { get; set; }
        public TGCVector3 VectorSalto = new TGCVector3(0, 1, 0);

        //Cosas Sombra
        public TgcPlane PlanoSombra { get; set; }
        public TgcTexture Sombra { get; set; }
        public TgcMesh PlanoSombraMesh { get; set; }

        //Cosas Humo del Auto
        public ParticleEmitter CañoDeEscape1;
        public ParticleEmitter CañoDeEscape2;
        private readonly int CantidadParticulas = 5;
        public TGCVector3 PosicionRelativaCaño1 = new TGCVector3(17,12,77);
        public TGCVector3 PosicionRelativaCaño2 = new TGCVector3(-17, 12, 77);
        private string PathHumo { get; set; }

        /////////////////////////

        public float AlturaCuerpoRigido = 20f;

        public AutoFisico(List<TgcMesh> valor, TgcMesh rueda, TGCVector3 posicionInicial, float direccionInicialEnGrados, FisicaMundo fisica, TgcTexture sombra, string pathHumo)
        {
            Fisica = fisica;
            Mayas = valor;
            PosicionInicial = posicionInicial;
            Sombra = sombra;
            PathHumo = pathHumo;
            DireccionInicial = new TGCVector3(FastMath.Cos(FastMath.ToRad(direccionInicialEnGrados)), 0 , FastMath.Sin(FastMath.ToRad(direccionInicialEnGrados)));


            //Creamos las instancias de cada rueda
            RuedaTrasIzq = rueda.createMeshInstance("Rueda Trasera Izquierda");
            RuedaDelIzq = rueda.createMeshInstance("Rueda Delantera Izquierda");
            RuedaTrasDer = rueda.createMeshInstance("Rueda Trasera Derecha");
            RuedaDelDer = rueda.createMeshInstance("Rueda Delantera Derecha");

            //Armo una lista con las ruedas
            Ruedas = new List<TgcMesh>
            {
                RuedaTrasIzq,
                RuedaDelIzq,
                RuedaTrasDer,
                RuedaDelDer
            };

            //Cuerpo Rigido Auto
            FriccionAuto = 0.2f;
            var tamañoAuto = new TGCVector3(23, AlturaCuerpoRigido, 80);
            CuerpoRigidoAuto = BulletRigidBodyFactory.Instance.CreateBox(tamañoAuto, 1000, PosicionInicial, 0, 0, 0, FriccionAuto, true);
            CuerpoRigidoAuto.Restitution = 0.7f;
            CuerpoRigidoAuto.SetDamping(0.5f, 0.2f);
            //CuerpoRigidoAuto.RollingFriction = 1000000;
            Fisica.dynamicsWorld.AddRigidBody(CuerpoRigidoAuto);

            //Sombras
            PlanoSombra = new TgcPlane(new TGCVector3(-31.5f, 0.2f, -70), new TGCVector3(65, 0, 140), TgcPlane.Orientations.XZplane, Sombra, 1, 1);
            PlanoSombraMesh = PlanoSombra.toMesh("Sombra");
            PlanoSombraMesh.AutoTransformEnable = false;
            PlanoSombraMesh.AlphaBlendEnable = true;

            // Humo (Tengo que hacerlo doble por cada caño de escape //////////////////////////////
            // Se puede hacer que cambie la textura si acelera, etc
            TGCVector3 VelocidadParticulas = new TGCVector3(10, 5, 10); // La velocidad que se mueve sobre cada eje
            CañoDeEscape1 = new ParticleEmitter(PathHumo, CantidadParticulas)
            {
                Dispersion = 3,
                MaxSizeParticle = 1f,
                MinSizeParticle = 1f,
                Speed = VelocidadParticulas
            };
            CañoDeEscape2 = new ParticleEmitter(PathHumo, CantidadParticulas)
            {
                Dispersion = 3,
                MaxSizeParticle = 1f,
                MinSizeParticle = 1f,
                Speed = VelocidadParticulas
            };

        }
       

        public void ConfigurarTeclas(Key acelerar, Key atras, Key derecha, Key izquierda, Key freno, Key salto)
        {
            Acelerar = acelerar;
            Atras = atras;
            Derecha = derecha;
            Izquierda = izquierda;
            Freno = freno;
            Salto = salto;
        }

        public bool EnElPiso()
        {
            if (CuerpoRigidoAuto.CenterOfMassPosition.Y < 21)
            {
                return true;
            }else
                return false;
        }

        public void Update(TgcD3dInput input)
        {

            Fisica.dynamicsWorld.StepSimulation(1 / 60f, 10);
            CuerpoRigidoAuto.ActivationState = ActivationState.ActiveTag;
            CuerpoRigidoAuto.AngularVelocity = TGCVector3.Empty.ToBulletVector3();

            var DireccionCuerpoRigido = new TGCVector3(CuerpoRigidoAuto.Orientation.X, CuerpoRigidoAuto.Orientation.Y, -CuerpoRigidoAuto.Orientation.Z).ToBulletVector3();
            float fuerzaMotor= 0;
            float fuerzaAlGirar = FastMath.Pow(FastMath.Abs(Velocidad),0.25f)* 1300;

            

            //Movimientos Adelante-Atras
            if (input.keyDown(Acelerar))           
            {
                if (Velocidad >= 0)
                {
                    Direccion = 1;
                    fuerzaMotor = 7000f;           
                }
            }
            else if (input.keyDown(Atras))
            {
                if (Velocidad <= 5f)
                {
                    Direccion = -1;
                    fuerzaMotor = 4000f;
                }
            }
            else
            {
                fuerzaMotor = 0f;
            }

            //Movimientos Derecha-Izquierda
            if (input.keyDown(Izquierda))
            {
                if (EnElPiso())
                {
                    CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(1, 0, 0).ToBulletVector3() * fuerzaAlGirar, new TGCVector3(20, 10, -60).ToBulletVector3());
                    CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(-1, 0, 0).ToBulletVector3() * fuerzaAlGirar, new TGCVector3(20, 10, 60).ToBulletVector3());
                    GradosRuedaAlDoblar = FastMath.Max(GradosRuedaAlDoblar - 0.04f, -0.7f);
                }
                else
                {
                    GradosRuedaAlDoblar = FastMath.Max(GradosRuedaAlDoblar - 0.04f, -0.7f);
                }

            }
            else if (input.keyDown(Derecha))
            {
                if (EnElPiso())
                {
                    CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(-1, 0, 0).ToBulletVector3() * fuerzaAlGirar, new TGCVector3(20, 10, -60).ToBulletVector3());
                    CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(1, 0, 0).ToBulletVector3() * fuerzaAlGirar, new TGCVector3(20, 10, 60).ToBulletVector3());
                    GradosRuedaAlDoblar = FastMath.Min(GradosRuedaAlDoblar + 0.04f, 0.7f);
                }
                else
                {
                    GradosRuedaAlDoblar = FastMath.Min(GradosRuedaAlDoblar + 0.04f, 0.7f);
                }
            }
            else
            {
                GradosRuedaAlDoblar = 0;
            }

            //Movimientos Freno
            if (input.keyDown(Freno))
            {
                CuerpoRigidoAuto.Friction = 20f;
            }
            else
            {
                CuerpoRigidoAuto.Friction = FriccionAuto;
            }

            //Movimientos Salto
            if (input.keyPressed(Salto))
            {
                if (EnElPiso())
                {
                    FuerzaSalto = 300f;
                    CuerpoRigidoAuto.ApplyCentralImpulse(VectorSalto.ToBulletVector3() * FuerzaSalto * Velocidad);
                }
            }

            CuerpoRigidoAuto.ApplyCentralImpulse(fuerzaMotor * VersorDirector.ToBulletVector3() * Direccion);
        }

        //Movimiento
        public TGCMatrix Movimiento { get => new TGCMatrix(CuerpoRigidoAuto.InterpolationWorldTransform) * TGCMatrix.Translation(1, -AlturaCuerpoRigido, 1); }

        //Matrices Sombra
        public TGCMatrix MovimientoSombra { get => new TGCMatrix(CuerpoRigidoAuto.InterpolationWorldTransform) * TGCMatrix.Translation(1, -CuerpoRigidoAuto.CenterOfMassPosition.Y+0.05f,1); }
        public float EscaladoSombra { get => 1 + (CuerpoRigidoAuto.CenterOfMassPosition.Y - AlturaCuerpoRigido) / 100; }
        public TGCMatrix EscalaSombra { get => TGCMatrix.Scaling(EscaladoSombra, 0, EscaladoSombra); }
        public TGCMatrix MovimientoTotalSombra { get => EscalaSombra * MovimientoSombra; }

        //Matriz que rota las rueda izquierda, para que quede como una rueda derecha
        public TGCMatrix FlipRuedaDerecha { get => TGCMatrix.RotationZ(FastMath.ToRad(180)); }

        //Matrices que colocan a las ruedas en su lugar
        public TGCMatrix TraslacionRuedaTrasDer { get => TGCMatrix.Translation(PosicionRuedaTrasDer); }
        public TGCMatrix TraslacionRuedaDelDer { get => TGCMatrix.Translation(PosicionRuedaDelDer); }
        public TGCMatrix TraslacionRuedaTrasIzq { get => TGCMatrix.Translation(PosicionRuedaTrasIzq); }
        public TGCMatrix TraslacionRuedaDelIzq { get => TGCMatrix.Translation(PosicionRuedaDelIzq); }

        //Matriz que hace rotar a las ruedas al doblar
        public TGCMatrix RotarRueda { get => TGCMatrix.RotationY(GradosRuedaAlDoblar*Direccion); }
        
        
        //Matrices que hacen girar a las ruedas con la velocidad
        public TGCMatrix GiroAcumuladoIzq = TGCMatrix.Identity;
        public TGCMatrix GiroAcumuladoDer = TGCMatrix.Identity;
        public TGCMatrix GirarRuedaIzq { get => TGCMatrix.RotationX(-Velocidad / 130); }
        public TGCMatrix GirarRuedaDer { get => TGCMatrix.RotationX(Velocidad / 130); }


        public void Render(float tiempo)
        {   
 
            foreach (var maya in Mayas)
            {
                maya.AutoTransformEnable = false;
                maya.Transform = Movimiento;
                maya.Render();
            }

            //Matrices que acumulan los cambios
            GiroAcumuladoIzq *= GirarRuedaIzq;
            GiroAcumuladoDer *= GirarRuedaDer;

            RuedaTrasIzq.Transform = GiroAcumuladoIzq  * TraslacionRuedaTrasIzq * Movimiento;
            RuedaTrasDer.Transform = GiroAcumuladoDer * FlipRuedaDerecha * TraslacionRuedaTrasDer * Movimiento;
            RuedaDelIzq.Transform = GiroAcumuladoIzq  * RotarRueda * TraslacionRuedaDelIzq * Movimiento;
            RuedaDelDer.Transform = GiroAcumuladoDer * FlipRuedaDerecha * RotarRueda * TraslacionRuedaDelDer * Movimiento;
            foreach (var maya in Ruedas)
            {
                maya.Render();
            }

            VersorDirector = TGCVector3.TransformNormal(DireccionInicial, Movimiento);

            //Sombras
            PlanoSombraMesh.Transform = MovimientoTotalSombra;
            PlanoSombraMesh.Render();

            //Humo
            D3DDevice.Instance.ParticlesEnabled = true;
            D3DDevice.Instance.EnableParticles();
            CañoDeEscape1.Position = TGCVector3.TransformCoordinate(PosicionRelativaCaño1, Movimiento);
            CañoDeEscape2.Position = TGCVector3.TransformCoordinate(PosicionRelativaCaño2, Movimiento);
            CañoDeEscape1.render(tiempo);
            CañoDeEscape2.render(tiempo);
        }

        public void Dispose()
        {
            CañoDeEscape1.dispose();
            CañoDeEscape2.dispose();
            PlanoSombraMesh.Dispose();
            foreach (var maya in Ruedas)
            {
                maya.Dispose();
            }
            foreach (var maya in Mayas)
            {
                maya.Dispose();
            }
        }
    }
}
