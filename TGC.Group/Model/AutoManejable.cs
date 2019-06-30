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
using TGC.Core.Sound;


namespace TGC.Group.Model
{
    public class AutoManejable : Auto
    {
        //Teclas
        private Key TeclaAcelerar { get; set; }
        private Key TeclaAtras { get; set; }
        private Key TeclaDerecha { get; set; }
        private Key TeclaIzquierda { get; set; }
        private Key TeclaFreno { get; set; }
        private Key TeclaSalto { get; set; }

        //Cosas del Salto
        public float FuerzaSalto { get; set; }
        public TGCVector3 VectorSalto = new TGCVector3(0, 1, 0);

        // Sonidos
        public TgcStaticSound sonidoAceleracion;
        public TgcStaticSound sonidoDesaceleracion;
        public TgcStaticSound frenada;
        public TgcStaticSound choque;
        public bool Invisible = false;

        // Tiempo
        public float ElapsedTime { get; set; }
        public float Vida { get; set; }

        public AutoManejable(List<TgcMesh> mayas, TGCVector3 posicionInicial, float direccionInicialEnGrados, FisicaMundo fisica, string pathHumo,string mediaDir, Microsoft.DirectX.DirectSound.Device sonido) :base(mayas,  posicionInicial, direccionInicialEnGrados,  fisica,  pathHumo,mediaDir,sonido)
        {
            Direccion = 1;
            //Cuerpo Rigido Auto
            FriccionAuto = 0.1f;
            var tamañoAuto = new TGCVector3(25, AlturaCuerpoRigido, 80);
            CuerpoRigidoAuto = BulletRigidBodyFactory.Instance.CreateBox(tamañoAuto, 100, PosicionInicial, 0, 0, 0, FriccionAuto, true);
            CuerpoRigidoAuto.Restitution = 0.3f;
            //CuerpoRigidoAuto.RollingFriction = 1000000;
            Fisica.dynamicsWorld.AddRigidBody(CuerpoRigidoAuto);
        }        

        public void ConfigurarTeclas(Key acelerar, Key atras, Key derecha, Key izquierda, Key freno, Key salto)
        {
            TeclaAcelerar = acelerar;
            TeclaAtras = atras;
            TeclaDerecha = derecha;
            TeclaIzquierda = izquierda;
            TeclaFreno = freno;
            TeclaSalto = salto;
        }

        public bool EnElPiso()
        {
            if (CuerpoRigidoAuto.CenterOfMassPosition.Y < 21)
            {
                return true;
            }
            else
                return false;
        }

        public float FuerzaAlGirar()
        {
            if (FastMath.Abs(Velocidad) > 20)
            {
                return 400;
            }
            else
            {
                return 0;
            }
        }

        public void Update(TgcD3dInput input)
        {
            Fisica.dynamicsWorld.StepSimulation(1 / 60f, 10);
            CuerpoRigidoAuto.ActivationState = ActivationState.ActiveTag;
            CuerpoRigidoAuto.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
            float fuerzaMotor = 0;

            //Movimientos Adelante-Atras
            if (EnElPiso()) {
                CuerpoRigidoAuto.SetDamping(0.32f, 0.1f);
                if (input.keyDown(TeclaAcelerar))
                {
                    if (Velocidad >= 0)
                    {
                        Direccion = 1;
                        fuerzaMotor = 14000f * ElapsedTime;
                        sonidoAceleracion.play(true);
                    }
                }
                else if (input.keyDown(TeclaAtras))
                {
                    if (Velocidad <= 5f)
                    {
                        Direccion = -1;
                        fuerzaMotor = 300f;
                        sonidoAceleracion.play(true);
                    }
                }
                else
                {
                    fuerzaMotor = 0f;
                    sonidoAceleracion.stop();
  
                }

                //Movimientos Derecha-Izquierda
                if (input.keyDown(TeclaIzquierda))
                {
                    CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(1, 0, 0).ToBulletVector3() * FuerzaAlGirar(), new TGCVector3(20, 10, -60).ToBulletVector3());
                    CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(-1, 0, 0).ToBulletVector3() * FuerzaAlGirar(), new TGCVector3(20, 10, 60).ToBulletVector3());
                    GradosRuedaAlDoblar = FastMath.Max(GradosRuedaAlDoblar - 0.04f, -0.7f);
                }
                else if (input.keyDown(TeclaDerecha))
                {
                    CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(-1, 0, 0).ToBulletVector3() * FuerzaAlGirar(), new TGCVector3(20, 10, -60).ToBulletVector3());
                    CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(1, 0, 0).ToBulletVector3() * FuerzaAlGirar(), new TGCVector3(20, 10, 60).ToBulletVector3());
                    GradosRuedaAlDoblar = FastMath.Min(GradosRuedaAlDoblar + 0.04f, 0.7f);
                }
                else
                {
                    GradosRuedaAlDoblar = 0;
                }

                //Movimientos Freno
                if (input.keyDown(TeclaFreno))
                {
                    CuerpoRigidoAuto.Friction = 8f;
                    frenada.play(false);
                }
                else
                {
                    CuerpoRigidoAuto.Friction = FriccionAuto;
                }

                //Movimientos Salto
                if (input.keyPressed(TeclaSalto))
                {
                    FuerzaSalto = 19f;
                    CuerpoRigidoAuto.ApplyCentralImpulse(VectorSalto.ToBulletVector3() * FuerzaSalto * Velocidad);
                }
            }
            else
            {
                CuerpoRigidoAuto.SetDamping(0f, 0f);
                if (input.keyDown(TeclaIzquierda))
                {
                    GradosRuedaAlDoblar = FastMath.Max(GradosRuedaAlDoblar - 0.04f, -0.7f);
                }
                else if (input.keyDown(TeclaDerecha))
                {
                    GradosRuedaAlDoblar = FastMath.Min(GradosRuedaAlDoblar + 0.04f, 0.7f);
                }
            }
            float impulso = 0;
            if (Velocidad < 15)
            {
                impulso = fuerzaMotor;
            }
            else if (Velocidad >= 15 && Velocidad < 35)
            {
                impulso =  1.8f * fuerzaMotor;
            }
            else if (Velocidad >= 35 && Velocidad < 60)
            {
                impulso =  3.2f * fuerzaMotor;
            }
            else if (Velocidad >= 60 && Velocidad < 80)
            {
                impulso =  4.2f * fuerzaMotor;
            }
            else if (Velocidad >= 80 && Velocidad < 100)
            {
                impulso =  5.2f * fuerzaMotor;
            }
            else
            {
                impulso = FastMath.Min( 7f * fuerzaMotor,1020f);
            }
            CuerpoRigidoAuto.ApplyCentralImpulse(impulso* VersorDirector.ToBulletVector3() * Direccion);
        }
    }
}
