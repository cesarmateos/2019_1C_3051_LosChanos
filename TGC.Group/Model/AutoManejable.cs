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
    public class AutoManejable
    {
        public FisicaMundo Fisica { get; set; }

        //private TgcMesh MayaAuto { get; set; }
        public List<TgcMesh> Mayas { get; set; }
        public RigidBody CuerpoRigidoAuto { get; set; }
        public TGCVector3 PosicionInicial { get; set; }

        //Teclas
        private Key TeclaAcelerar { get; set; }
        private Key TeclaAtras { get; set; }
        private Key TeclaDerecha { get; set; }
        private Key TeclaIzquierda { get; set; }
        private Key TeclaFreno { get; set; }
        private Key TeclaSalto { get; set; }

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
        public float Velocidad2 { get; set; }

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
        public TGCVector3 PosicionRelativaCaño1 = new TGCVector3(17, 12, 77);
        public TGCVector3 PosicionRelativaCaño2 = new TGCVector3(-17, 12, 77);
        private string PathHumo { get; set; }

        //Sonido del Auto
        public TgcMp3Player sonidoAuto;
        public string mp3Actual = null;

        //Media
        public string media { get; set; }

        /////////////////////////

        public float AlturaCuerpoRigido = 20f;

        public AutoManejable(List<TgcMesh> valor, TgcMesh rueda, TGCVector3 posicionInicial, float direccionInicialEnGrados, FisicaMundo fisica, TgcTexture sombra, string pathHumo)
        {
            Fisica = fisica;
            Mayas = valor;
            PosicionInicial = posicionInicial;
            Sombra = sombra;
            PathHumo = pathHumo;
            DireccionInicial = new TGCVector3(FastMath.Cos(FastMath.ToRad(direccionInicialEnGrados)), 0, FastMath.Sin(FastMath.ToRad(direccionInicialEnGrados)));


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
            FriccionAuto = 0.5f;
            var tamañoAuto = new TGCVector3(25, AlturaCuerpoRigido, 80);
            CuerpoRigidoAuto = BulletRigidBodyFactory.Instance.CreateBox(tamañoAuto, 100, PosicionInicial, 0, 0, 0, FriccionAuto, true);
            CuerpoRigidoAuto.Restitution = 0.4f;
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

            // Sonidos
            sonidoAuto = new TgcMp3Player();
        }

        // Modificar el archivo mp3 a ejecutar
        public void cargarMp3(string dir)
        {
            if (mp3Actual != dir || mp3Actual == null)
            {
                switch (sonidoAuto.getStatus())
                {
                    case TgcMp3Player.States.Open:
                        {
                            sonidoAuto.closeFile();
                            mp3Actual = dir;
                            sonidoAuto.FileName = dir;
                            break;
                        }
                    case TgcMp3Player.States.Playing:
                        {
                            sonidoAuto.stop();
                            mp3Actual = dir;
                            sonidoAuto.closeFile();
                            sonidoAuto.FileName = dir;
                            break;
                        }
                    default:
                        break;
                }
            }
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

        public void Update(TgcD3dInput input)
        {

            Fisica.dynamicsWorld.StepSimulation(1 / 60f, 10);
            CuerpoRigidoAuto.ActivationState = ActivationState.ActiveTag;
            CuerpoRigidoAuto.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
            float fuerzaMotor = 0;
            float fuerzaAlGirar = FastMath.Pow(FastMath.Abs(Velocidad/10), 0.25f) * 130;

            //Movimientos Adelante-Atras
            if (EnElPiso()) {
                CuerpoRigidoAuto.SetDamping(0.35f, 0.1f);
                if (input.keyDown(TeclaAcelerar))
                {
                    if (Velocidad >= 0)
                    {
                        Direccion = 1;
                        fuerzaMotor = 200f;
                        //cargarMp3(media + "Arranque Brusco.mp3");
                        //sonidoAuto.FileName = media + "Frenada.mp3";
                        //sonidoAuto.play(false);
                    }
                }
                else if (input.keyDown(TeclaAtras))
                {
                    if (Velocidad <= 5f)
                    {
                        Direccion = -1;
                        fuerzaMotor = 200f;
                    }
                }
                else
                {
                    fuerzaMotor = 0f;
                }

                //Movimientos Derecha-Izquierda
                if (input.keyDown(TeclaIzquierda))
                {
                    CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(1, 0, 0).ToBulletVector3() * fuerzaAlGirar, new TGCVector3(20, 10, -60).ToBulletVector3());
                    CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(-1, 0, 0).ToBulletVector3() * fuerzaAlGirar, new TGCVector3(20, 10, 60).ToBulletVector3());
                    GradosRuedaAlDoblar = FastMath.Max(GradosRuedaAlDoblar - 0.04f, -0.7f);
                }
                else if (input.keyDown(TeclaDerecha))
                {
                    CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(-1, 0, 0).ToBulletVector3() * fuerzaAlGirar, new TGCVector3(20, 10, -60).ToBulletVector3());
                    CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(1, 0, 0).ToBulletVector3() * fuerzaAlGirar, new TGCVector3(20, 10, 60).ToBulletVector3());
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
                    //cargarMp3(media + "Musica\\Frenada.mp3");
                    //sonidoAuto.play(false);
                }
                else
                {
                    CuerpoRigidoAuto.Friction = FriccionAuto;
                }

                //Movimientos Salto
                if (input.keyPressed(TeclaSalto))
                {
                    FuerzaSalto = 30f;
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
            if (Velocidad >= 0 && Velocidad < 20)
            {
                impulso = fuerzaMotor;
            }
            else if (Velocidad >= 20 && Velocidad < 40)
            {
                impulso = Velocidad * 0.05f * fuerzaMotor;
            }
            else if (Velocidad >= 40 && Velocidad < 60)
            {
                impulso = Velocidad * 0.035f * fuerzaMotor;
            }
            else if (Velocidad >= 60 && Velocidad < 80)
            {
                impulso = Velocidad * 0.032f * fuerzaMotor;
            }
            else if (Velocidad >= 80 && Velocidad < 100)
            {
                impulso = Velocidad * 0.03f * fuerzaMotor;
            }
            else
            {
                impulso = FastMath.Min(Velocidad * 0.028f * fuerzaMotor,900f);
            }
            CuerpoRigidoAuto.ApplyCentralImpulse(impulso* VersorDirector.ToBulletVector3() * Direccion);
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
            sonidoAuto.closeFile();
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
