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

        //Cosas Humo del Auto
        private string PathHumo { get; set; }

        //Media
        public string Media { get; set; }

        // Sonidos
        public TgcStaticSound sonidoAceleracion;
        public TgcStaticSound sonidoDesaceleracion;
        public TgcStaticSound frenada;
        public TgcStaticSound choque;
        
        // Tiempo
        public float ElapsedTime { get; set; }
        

        public AutoManejable(List<TgcMesh> valor, TgcMesh rueda, TGCVector3 posicionInicial, float direccionInicialEnGrados, FisicaMundo fisica, TgcTexture sombra, string pathHumo)
        {
            Fisica = fisica;
            Mayas = valor;
            PosicionInicial = posicionInicial;
            Sombra = sombra;
            PathHumo = pathHumo;
            DireccionInicial = new TGCVector3(FastMath.Cos(FastMath.ToRad(direccionInicialEnGrados)), 0, FastMath.Sin(FastMath.ToRad(direccionInicialEnGrados)));

            Direccion = 1;

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
            FriccionAuto = 0.1f;
            var tamañoAuto = new TGCVector3(25, AlturaCuerpoRigido, 80);
            CuerpoRigidoAuto = BulletRigidBodyFactory.Instance.CreateBox(tamañoAuto, 100, PosicionInicial, 0, 0, 0, FriccionAuto, true);
            CuerpoRigidoAuto.Restitution = 0.3f;
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
                    FuerzaSalto = 1900f*ElapsedTime;
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
                impulso = Velocidad * 0.09f * fuerzaMotor;
            }
            else if (Velocidad >= 35 && Velocidad < 60)
            {
                impulso = Velocidad * 0.05f * fuerzaMotor;
            }
            else if (Velocidad >= 60 && Velocidad < 80)
            {
                impulso = Velocidad * 0.055f * fuerzaMotor;
            }
            else if (Velocidad >= 80 && Velocidad < 100)
            {
                impulso = Velocidad * 0.06f * fuerzaMotor;
            }
            else
            {
                impulso = FastMath.Min(Velocidad * 0.09f * fuerzaMotor,1020f);
            }
            CuerpoRigidoAuto.ApplyCentralImpulse(impulso* VersorDirector.ToBulletVector3() * Direccion);
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
