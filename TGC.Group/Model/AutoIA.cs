using System.Collections.Generic;
using BulletSharp;
using TGC.Core.BulletPhysics;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;


namespace TGC.Group.Model
{
    public class AutoIA : Auto
    {
        public float FuerzaMotor { get; set; }

        public AutoManejable[] Enemigos { get; set; }

        //Sonido
        public Tgc3dSound motorIA;

        public AutoIA(List<TgcMesh> mayas, TGCVector3 posicionInicial, float direccionInicialEnGrados, FisicaMundo fisica, string pathHumo, AutoManejable[] enemigos,string mediaDir, Sonidos sonidos) : base(mayas, posicionInicial, direccionInicialEnGrados, fisica, pathHumo,mediaDir,sonidos)
        {
            Enemigos = enemigos;

            //Cuerpo Rigido Auto
            FriccionAuto = 0.2f;
            var tamañoAuto = new TGCVector3(25, AlturaCuerpoRigido, 80);
            CuerpoRigidoAuto = BulletRigidBodyFactory.Instance.CreateBox(tamañoAuto, 1000, PosicionInicial, 0, 0, 0, FriccionAuto, true);
            CuerpoRigidoAuto.Restitution = 0.4f;
            CuerpoRigidoAuto.SetDamping(0.5f, 0.2f);
            //CuerpoRigidoAuto.RollingFriction = 1000000;
            Fisica.dynamicsWorld.AddRigidBody(CuerpoRigidoAuto);
            Direccion = 1; 
        }
        public float DistanciaAlEnemigo(AutoManejable enemigo)
        {
            return FastMath.Pow((FastMath.Pow2(enemigo.CuerpoRigidoAuto.CenterOfMassPosition.X - CuerpoRigidoAuto.CenterOfMassPosition.X) + FastMath.Pow2(enemigo.CuerpoRigidoAuto.CenterOfMassPosition.Z - CuerpoRigidoAuto.CenterOfMassPosition.Z)), 0.5f);
        }

        public List<AutoManejable> Enemgios { get; set; }

        public TGCVector3 PosicionActual()
        {
            return RuedaDelDer.Position;
        }

        //Vector que va desde el centro de Masa de la IA al centro de Masa del Jugador Objetivo
        public TGCVector2 VectorAlEnemigo(AutoManejable enemigo)
        {
            return new TGCVector2(enemigo.CuerpoRigidoAuto.CenterOfMassPosition.X - CuerpoRigidoAuto.CenterOfMassPosition.X, enemigo.CuerpoRigidoAuto.CenterOfMassPosition.Z - CuerpoRigidoAuto.CenterOfMassPosition.Z);
        }

        public float FuerzaAlGirar { get => FastMath.Pow(FastMath.Abs(Velocidad), 0.25f) * 1300; }
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

        //Rota un Vector 0.5f
        public TGCVector2 RotarVector(TGCVector2 vector)
        {
            var coseno = FastMath.Cos(0.5f);
            var seno = FastMath.Sin(0.5f);
            return new TGCVector2(vector.X * coseno - vector.Y * seno, vector.X * seno + vector.Y * coseno);
        }
        public void Moverse(bool juegoDoble)
        {
            //AutoManejable enemigo;
            if (juegoDoble)
            {
                FuerzaMotor = 15000f;
                if (Enemigos[0].Invisible && Enemigos[1].Invisible)
                {         
                    FuerzaMotor = 0f ;
                    Atacar(Enemigos[0]);
                }
                else if (Enemigos[1].Invisible)
                {
                    Atacar(Enemigos[0]);
                }
                else if (Enemigos[0].Invisible)
                {
                    Atacar(Enemigos[1]);
                }
                else
                {
                    if (DistanciaAlEnemigo(Enemigos[0]) < DistanciaAlEnemigo(Enemigos[1]))
                    {
                        Atacar(Enemigos[0]);
                    }
                    else
                    {
                        Atacar(Enemigos[1]);
                    }
                }
            }
            else
            {
                Atacar(Enemigos[0]);
                if (Enemigos[0].Invisible)
                {
                    FuerzaMotor = 0f;
                }
                else
                {
                    FuerzaMotor = 15000f;              
                }
            }
        }
        public float Cono(TGCVector2 vector)
        {
            return TGCVector2.Dot(TGCVector2.Normalize(vector), VersorDirector2D());
        }
        public TGCVector2 VersorDirector2D()
        {
            return new TGCVector2(VersorDirector.X, VersorDirector.Z);
        }
        public void Atacar(AutoManejable enemigo)
        {
            Fisica.dynamicsWorld.StepSimulation(1 / 60f, 10);
            CuerpoRigidoAuto.ActivationState = ActivationState.ActiveTag;
            CuerpoRigidoAuto.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
            CuerpoRigidoAuto.ApplyCentralImpulse(FuerzaMotor * VersorDirector.ToBulletVector3());
            var cono = Cono(VectorAlEnemigo(enemigo));

            if (cono <0.98f) {
                 if (cono < Cono(RotarVector(VectorAlEnemigo(enemigo))))
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
    }
}
