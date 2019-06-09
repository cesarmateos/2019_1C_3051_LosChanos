using System;
using System.Collections.Generic;
using BulletSharp;
using TGC.Core.BulletPhysics;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.Particle;


namespace TGC.Group.Model
{
    public class AutoIA
    {
        public FisicaMundo Fisica { get; set; }

        //private TgcMesh MayaAuto { get; set; }
        public List<TgcMesh> Mayas { get; set; }
        public RigidBody CuerpoRigidoAuto { get; set; }
        public TGCVector3 PosicionInicial { get; set; }

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
        public float Velocidad
        {
            get => FastMath.Abs(CuerpoRigidoAuto.LinearVelocity.X) + FastMath.Abs(CuerpoRigidoAuto.LinearVelocity.Z) * Direccion;
        }
        public float FuerzaMotor { get; set; }

        //Cosas Sombra
        public TgcPlane PlanoSombra { get; set; }
        public TgcTexture Sombra { get; set; }
        public TgcMesh PlanoSombraMesh { get; set; }

        //Cosas Humo del Auto
        public ParticleEmitter CañoDeEscape1;
        public ParticleEmitter CañoDeEscape2;
        private readonly int CantidadParticulas = 5;
        public TGCVector3 PosicionRelativaCaño1 = new TGCVector3(17, 12, 77);
        public TGCVector3 PosicionRelativaCaño2 = new TGCVector3(-17, 12, 77);
        private string PathHumo { get; set; }


        public float AlturaCuerpoRigido = 20f;

        public List<AutoManejable> Enemigos { get; set; }
        public AutoManejable Enemigo { get; set; }

        public AutoIA(List<TgcMesh> valor, TgcMesh rueda, TGCVector3 posicionInicial, float direccionInicialEnGrados, FisicaMundo fisica, TgcTexture sombra, string pathHumo, AutoManejable enemigo)
        {
            Fisica = fisica;
            Mayas = valor;
            PosicionInicial = posicionInicial;
            Sombra = sombra;
            PathHumo = pathHumo;
            DireccionInicial = new TGCVector3(FastMath.Cos(FastMath.ToRad(direccionInicialEnGrados)), 0, FastMath.Sin(FastMath.ToRad(direccionInicialEnGrados)));
            Enemigo = enemigo;


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
            var tamañoAuto = new TGCVector3(25, AlturaCuerpoRigido, 80);
            CuerpoRigidoAuto = BulletRigidBodyFactory.Instance.CreateBox(tamañoAuto, 1000, PosicionInicial, 0, 0, 0, FriccionAuto, true);
            CuerpoRigidoAuto.Restitution = 0.4f;
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
        public float DistanciaAlEnemigo(AutoManejable enemigo)
        {
            return FastMath.Pow((FastMath.Pow2(enemigo.CuerpoRigidoAuto.CenterOfMassPosition.X - CuerpoRigidoAuto.CenterOfMassPosition.X) + FastMath.Pow2(enemigo.CuerpoRigidoAuto.CenterOfMassPosition.Z - CuerpoRigidoAuto.CenterOfMassPosition.Z)),0.5f);
        }

        //public AutoManejable ElegirEnemigo(
        //{
        //    return Enemigos.Sort((a, b) => DistanciaAlEnemigo(a));
        //}

        public TGCVector2 VectorAlEnemigo()
        {
            return new TGCVector2(Enemigo.CuerpoRigidoAuto.CenterOfMassPosition.X - CuerpoRigidoAuto.CenterOfMassPosition.X, Enemigo.CuerpoRigidoAuto.CenterOfMassPosition.Z - CuerpoRigidoAuto.CenterOfMassPosition.Z);
        }
        public float ModuloVector(TGCVector2 vector)
        {
            return FastMath.Pow(FastMath.Pow2(vector.X) + FastMath.Pow2(vector.Y), 0.5f);
        }
        public float AnguloAlEnemigo()
        {
            return FastMath.ToDeg(FastMath.Acos((VersorDirector.X * VectorAlEnemigo().X + VersorDirector.Z * VectorAlEnemigo().Y) / (ModuloVector(new TGCVector2(VersorDirector.X, VersorDirector.Z)) * ModuloVector(VectorAlEnemigo()))));
        }
        public float FuerzaAlGirar { get => FastMath.Pow(FastMath.Abs(Velocidad), 0.25f) * 1300; }
        public void Acelerar()
        {
            if (Velocidad >= 0)
            {
                Direccion = 1;
                FuerzaMotor = 5000f;
            }
        }
        public void GirarDerecha()
        {
            CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(-1, 0, 0).ToBulletVector3() * FuerzaAlGirar, new TGCVector3(20, 10, -60).ToBulletVector3());
            CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(1, 0, 0).ToBulletVector3() * FuerzaAlGirar, new TGCVector3(20, 10, 60).ToBulletVector3());
            GradosRuedaAlDoblar = FastMath.Min(GradosRuedaAlDoblar + 0.04f, 0.7f);
        }
        public void GirarIzquierda()
        {
            CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(1, 0, 0).ToBulletVector3() * FuerzaAlGirar, new TGCVector3(20, 10, -60).ToBulletVector3());
            CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(-1, 0, 0).ToBulletVector3() * FuerzaAlGirar, new TGCVector3(20, 10, 60).ToBulletVector3());
            GradosRuedaAlDoblar = FastMath.Max(GradosRuedaAlDoblar - 0.04f, -0.7f);
        }
        public void NoGirar()
        {
            GradosRuedaAlDoblar = 0f;
        }
        public void Moverse()
        {
            Fisica.dynamicsWorld.StepSimulation(1 / 60f, 10);
            CuerpoRigidoAuto.ActivationState = ActivationState.ActiveTag;
            CuerpoRigidoAuto.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
            CuerpoRigidoAuto.ApplyCentralImpulse(FuerzaMotor * VersorDirector.ToBulletVector3() * Direccion);

            Acelerar();
            if(AnguloAlEnemigo()>4)
            {
                if (AnguloAlEnemigo() > 180)
                {
                    GirarDerecha();
                }
                else
                {
                    GirarIzquierda();
                }
            }
            else
            {
                NoGirar();
            }

        }


        //Movimiento
        public TGCMatrix Movimiento { get => new TGCMatrix(CuerpoRigidoAuto.InterpolationWorldTransform) * TGCMatrix.Translation(1, -AlturaCuerpoRigido, 1); }

        //Matrices Sombra
        public TGCMatrix MovimientoSombra { get => new TGCMatrix(CuerpoRigidoAuto.InterpolationWorldTransform) * TGCMatrix.Translation(1, -CuerpoRigidoAuto.CenterOfMassPosition.Y + 0.05f, 1); }
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
        public TGCMatrix RotarRueda { get => TGCMatrix.RotationY(GradosRuedaAlDoblar * Direccion); }


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

            RuedaTrasIzq.Transform = GiroAcumuladoIzq * TraslacionRuedaTrasIzq * Movimiento;
            RuedaTrasDer.Transform = GiroAcumuladoDer * FlipRuedaDerecha * TraslacionRuedaTrasDer * Movimiento;
            RuedaDelIzq.Transform = GiroAcumuladoIzq * RotarRueda * TraslacionRuedaDelIzq * Movimiento;
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
