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

        //Cosas de Giros
        public int Direccion { get; set; }
        public TGCVector3 DireccionInicial { get; set; }
        public float GradosRotacion { get; set; }
        public float gradosGiro = FastMath.ToRad(0.4f);

        //Friccion del auto
        public float FriccionAuto { get; set; }

        //Potencia 
        public float FuerzaMotor { get; set; }

        //Cosas del Salto
        public float FuerzaSalto { get; set; }
        public TGCVector3 VectorSalto { get; set; }

        //Cosas Sombra
        public TgcPlane PlanoSombra { get; set; }
        public TgcTexture Sombra { get; set; }
        public TgcMesh PlanoSombraMesh { get; set; }

        public float AlturaCuerpoRigido = 20f;

        public AutoFisico(List<TgcMesh> valor, TgcMesh rueda, TGCVector3 posicionInicial, float direccionInicialEnGrados, FisicaMundo fisica, TgcTexture sombra)
        {
            Fisica = fisica;
            Mayas = valor;
            PosicionInicial = posicionInicial;
            Sombra = sombra;
            DireccionInicial = new TGCVector3 (FastMath.Cos(FastMath.ToRad(direccionInicialEnGrados)),1,FastMath.Sin(FastMath.ToRad(direccionInicialEnGrados)));


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

            FriccionAuto = 0.5f;
            var tamañoAuto = new TGCVector3(25, AlturaCuerpoRigido, 80);
            CuerpoRigidoAuto = BulletRigidBodyFactory.Instance.CreateBox(tamañoAuto, 1000, PosicionInicial, 0, 0, 0, FriccionAuto, true);
            CuerpoRigidoAuto.Restitution = 0.7f;
            CuerpoRigidoAuto.SetDamping(0.5f, 0.5f);
            //CuerpoRigidoAuto.RollingFriction = 1000000;
            Fisica.dynamicsWorld.AddRigidBody(CuerpoRigidoAuto);

            PlanoSombra = new TgcPlane(new TGCVector3(-31.5f, 0.2f, -70), new TGCVector3(65, 0, 140), TgcPlane.Orientations.XZplane, Sombra, 1, 1);
            PlanoSombraMesh = PlanoSombra.toMesh("Sombra");

            VectorSalto = new TGCVector3(0, 1, 0);



            PlanoSombraMesh.AutoTransformEnable = false;
            PlanoSombraMesh.AlphaBlendEnable = true;
        }
        public TGCVector3 VersorDirector
        {
            get;
            set;
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

        public void Update(TgcD3dInput input)
        {

            var DireccionCuerpoRigido = new TGCVector3(CuerpoRigidoAuto.Orientation.X, CuerpoRigidoAuto.Orientation.Y, -CuerpoRigidoAuto.Orientation.Z).ToBulletVector3();
            Fisica.dynamicsWorld.StepSimulation(1 / 60f, 10);

            CuerpoRigidoAuto.ActivationState = ActivationState.ActiveTag;
            CuerpoRigidoAuto.AngularVelocity = TGCVector3.Empty.ToBulletVector3();


            if (input.keyDown(Acelerar))
            {
                Direccion = 1;
                FuerzaMotor = 7000f;
                CuerpoRigidoAuto.ApplyCentralImpulse(FuerzaMotor * VersorDirector.ToBulletVector3() * Direccion);
            }
            else if (input.keyDown(Atras))
            {
                Direccion = -1;
                FuerzaMotor = 3000f;
                CuerpoRigidoAuto.ApplyCentralImpulse(FuerzaMotor * VersorDirector.ToBulletVector3() * Direccion);
            }
            else
            {
                FuerzaMotor = 0f;
            }
            if (input.keyDown(Izquierda))
            {
                CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(1, 0, 0).ToBulletVector3() * 7000f, new TGCVector3(10, 10, -50).ToBulletVector3());
                CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(-1, 0, 0).ToBulletVector3() * 7000f, new TGCVector3(10, 10, 50).ToBulletVector3());
                GradosRuedaAlDoblar = FastMath.Max(GradosRuedaAlDoblar - 0.04f, -0.7f);
            }
            else if (input.keyDown(Derecha))
            {
                CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(-1, 0, 0).ToBulletVector3() * 7000f, new TGCVector3(10, 10, -50).ToBulletVector3());
                CuerpoRigidoAuto.ApplyImpulse(new TGCVector3(1, 0, 0).ToBulletVector3() * 7000f, new TGCVector3(10, 10, 50).ToBulletVector3());
                GradosRuedaAlDoblar = FastMath.Min(GradosRuedaAlDoblar + 0.04f, 0.7f);
            }
            else
            {
                GradosRuedaAlDoblar = 0;
            }
            if (input.keyDown(Freno))
            {
                CuerpoRigidoAuto.Friction = 5f;
            }
            else
            {
                CuerpoRigidoAuto.Friction = FriccionAuto;
            }
            if (input.keyPressed(Salto))
            {
                FuerzaSalto = 120000f;
                CuerpoRigidoAuto.ApplyCentralImpulse(VectorSalto.ToBulletVector3() * FuerzaSalto);
            }
            else
            {
                FuerzaSalto = 0f;
            }


        }

        //Movimiento
        public TGCMatrix Movimiento { get => new TGCMatrix(CuerpoRigidoAuto.InterpolationWorldTransform) * TGCMatrix.Translation(1, -AlturaCuerpoRigido, 1); }

        //Matrices Sombra
        public TGCMatrix MovimientoSombra { get => new TGCMatrix(CuerpoRigidoAuto.InterpolationWorldTransform) * TGCMatrix.Translation(1, -CuerpoRigidoAuto.CenterOfMassPosition.Y+0.05f,1); }
        public float EscaladoSombra { get => 1 - (CuerpoRigidoAuto.CenterOfMassPosition.Y - AlturaCuerpoRigido) / 20; }
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
        public TGCMatrix RotarRueda { get => TGCMatrix.RotationY(GradosRuedaAlDoblar); }


        public void Render(float tiempo)
        {

            //VersorDirector = DireccionInicial.ToBulletVector3() * CuerpoRigidoAuto.InterpolationWorldTransform;

            foreach (var maya in Mayas)
            {
                maya.AutoTransformEnable = false;
                maya.Transform = Movimiento;
                maya.Render();
            }
            RuedaTrasIzq.Transform = TraslacionRuedaTrasIzq * Movimiento;
            RuedaTrasDer.Transform = FlipRuedaDerecha * TraslacionRuedaTrasDer * Movimiento;
            RuedaDelIzq.Transform = RotarRueda * TraslacionRuedaDelIzq * Movimiento;
            RuedaDelDer.Transform = FlipRuedaDerecha * RotarRueda * TraslacionRuedaDelDer * Movimiento;
            foreach (var maya in Ruedas)
            {
                maya.Render();
            }

            VersorDirector = TGCVector3.TransformNormal(DireccionInicial, Movimiento);

            PlanoSombraMesh.Transform = MovimientoTotalSombra;
            PlanoSombraMesh.Render();
        }

        public void Dispose()
        {
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
