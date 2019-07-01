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
using TGC.Core.Sound;


namespace TGC.Group.Model
{
    public class AutoIA : Auto
    {
        public float FuerzaMotor { get; set; }

        public AutoManejable[] Enemigos { get; set; }
        //public AutoManejable Enemigo { get; set; }

        //Sonido
        public Tgc3dSound motorIA;

        public AutoIA(List<TgcMesh> mayas, TGCVector3 posicionInicial, float direccionInicialEnGrados, FisicaMundo fisica, string pathHumo, AutoManejable[] enemigos,string mediaDir, Microsoft.DirectX.DirectSound.Device sonido) : base(mayas, posicionInicial, direccionInicialEnGrados, fisica, pathHumo,mediaDir,sonido)
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

        public AutoManejable ElegirEnemigo()
        {
            if (Enemigos[0] == null)
            {
                return Enemigos[1];
            }
            else if (Enemigos[1] == null){
                return Enemigos[0];
            }
            else
            {
                if (DistanciaAlEnemigo(Enemigos[0]) < DistanciaAlEnemigo(Enemigos[1]))
                {
                    return Enemigos[0];
                }
                else
                {
                    return Enemigos[1];
                }
            }
        }

        //Vector que va desde el centro de Masa de la IA al centro de Masa del Jugador Objetivo
        public TGCVector2 VectorAlEnemigo()
        {
            return new TGCVector2(ElegirEnemigo().CuerpoRigidoAuto.CenterOfMassPosition.X - CuerpoRigidoAuto.CenterOfMassPosition.X, ElegirEnemigo().CuerpoRigidoAuto.CenterOfMassPosition.Z - CuerpoRigidoAuto.CenterOfMassPosition.Z);
        }

        //Fórmula clásica para calcular el módulo de un vector
        public float ModuloVector(TGCVector2 vector)
        {
            return FastMath.Pow(FastMath.Pow2(vector.X) + FastMath.Pow2(vector.Y), 0.5f);
        }

        //Ángulo entre la dirección a la que apunta el auto IA y otro vector
        public float AnguloAlVector(TGCVector2 vector)
        {
            return FastMath.ToDeg(FastMath.Acos((VersorDirector.X * vector.X + VersorDirector.Z * vector.Y) / (ModuloVector(new TGCVector2(VersorDirector.X, VersorDirector.Z)) * ModuloVector(vector))));
        }
        public float FuerzaAlGirar { get => FastMath.Pow(FastMath.Abs(Velocidad), 0.25f) * 1300; }
        public void Acelerar()
        {
            if (Velocidad >= 0)
            {
                Direccion = 1;
                FuerzaMotor = 15000f;
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
        //Rota un Vector 0.5f
        public TGCVector2 RotarVector(TGCVector2 vector)
        {
            var coseno = FastMath.Cos(0.5f);
            var seno = FastMath.Sin(0.5f);
            return new TGCVector2(vector.X * coseno - vector.Y * seno, vector.X * seno + vector.Y * coseno);
        }
        public void Moverse()
        {
            if (Enemigos[0] == null && Enemigos[1] == null)
            {

            }
            else
            {
                Fisica.dynamicsWorld.StepSimulation(1 / 60f, 10);
                CuerpoRigidoAuto.ActivationState = ActivationState.ActiveTag;
                CuerpoRigidoAuto.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
                CuerpoRigidoAuto.ApplyCentralImpulse(FuerzaMotor * VersorDirector.ToBulletVector3() * Direccion);
                var anguloAlEnemigo = AnguloAlVector(VectorAlEnemigo()); //Ángulo entre la Direeción a la que apunta el IA y el vector al enemigo 

                Acelerar();
                if (anguloAlEnemigo > 5) //Si el ángulo al enemgio es mayor a 5 grados gira, de lo contrario sigue derecho
                {
                    if (anguloAlEnemigo > AnguloAlVector(RotarVector(VectorAlEnemigo()))) //Si el ángulo al enemigo es mayor al ángulo al enemigo rotado 0.5f(en sentido antihorario), gira a la derecha
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
}
