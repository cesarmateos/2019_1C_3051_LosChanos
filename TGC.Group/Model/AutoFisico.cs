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
        public float GradosRotacion { get; set; }
        public float gradosGiro = FastMath.ToRad(0.4f);

        //Friccion del auto
        public float FriccionAuto { get; set; }

        public AutoFisico(List<TgcMesh> valor, TgcMesh rueda, TGCVector3 posicionInicial, FisicaMundo fisica)
        {
            Fisica = fisica;
            Mayas = valor;
            PosicionInicial = posicionInicial;


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


            var tamañoAuto = new TGCVector3(80, 10, 60);
            CuerpoRigidoAuto = BulletRigidBodyFactory.Instance.CreateBox(tamañoAuto, 1000, PosicionInicial, 0, 0, 0, FriccionAuto, true);
            CuerpoRigidoAuto.Restitution = 0.2f;
            //CuerpoRigidoAuto.Gravity = new TGCVector3(0, -100f, 0).ToBulletVector3();
            FriccionAuto = 1000f;
            CuerpoRigidoAuto.SetDamping(0.1f, 0.1f);
            CuerpoRigidoAuto.RollingFriction = 0;
            Fisica.dynamicsWorld.AddRigidBody(CuerpoRigidoAuto);
        }
        public TGCVector3 VersorDirector()
        {
            return new TGCVector3(FastMath.Cos(FastMath.ToRad(270) + GradosRotacion), 0, FastMath.Sin(FastMath.ToRad(270) + GradosRotacion));
        }
        //public void ComportamientoFisico(BulletSharp.Math.Vector3 impulso)
        //{
        //    CuerpoRigidoAuto.ActivationState = ActivationState.ActiveTag;
        //    CuerpoRigidoAuto.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
        //    CuerpoRigidoAuto.ApplyCentralImpulse(impulso);
        //    //CuerpoRigidoAuto.ApplyImpulse(impulso, new BulletSharp.Math.Vector3(0, 0, 0));
        //}

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
            var fuerza = 1000f;
            Fisica.dynamicsWorld.StepSimulation(1 / 60f, 100);
            CuerpoRigidoAuto.ActivationState = ActivationState.ActiveTag;
            CuerpoRigidoAuto.AngularVelocity = TGCVector3.Empty.ToBulletVector3();
            CuerpoRigidoAuto.ApplyCentralImpulse(fuerza * Direccion * VersorDirector().ToBulletVector3());

            if (input.keyDown(Acelerar))
            {
                Direccion = 1;
            }
            else if (input.keyDown(Atras))
            {
                Direccion = -1;
            }
            if (input.keyDown(Izquierda))
            {
                GradosRotacion += gradosGiro;
                GradosRuedaAlDoblar = FastMath.Max(GradosRuedaAlDoblar - 0.04f, -0.7f);
            }
            else if (input.keyDown(Derecha))
            {
                GradosRotacion -= gradosGiro;
                GradosRuedaAlDoblar = FastMath.Min(GradosRuedaAlDoblar + 0.04f, 0.7f);
            }
            else
            {
                GradosRuedaAlDoblar = 0;
            }
            if (input.keyDown(Freno))
            {
                FriccionAuto = 100000f;
            }
            else
            {
                FriccionAuto = 1000f;
            }
        }

        //Movimiento
        public TGCMatrix Movimiento { get => TGCMatrix.Translation(CuerpoRigidoAuto.CenterOfMassPosition.X, CuerpoRigidoAuto.CenterOfMassPosition.Y, CuerpoRigidoAuto.CenterOfMassPosition.Z); }
        public TGCMatrix Rotacion { get => TGCMatrix.RotationY(-GradosRotacion); }
        public TGCMatrix MovimientoTotal { get => Rotacion * Movimiento; }


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


            foreach (var maya in Mayas)
            {
                maya.AutoTransformEnable = false;
                maya.Position = new TGCVector3(CuerpoRigidoAuto.CenterOfMassPosition.X, CuerpoRigidoAuto.CenterOfMassPosition.Y, CuerpoRigidoAuto.CenterOfMassPosition.Z);
                maya.Transform = MovimientoTotal;
                maya.Render();
            }
            RuedaTrasIzq.Transform = TraslacionRuedaTrasIzq * MovimientoTotal;
            RuedaTrasDer.Transform = FlipRuedaDerecha * TraslacionRuedaTrasDer * MovimientoTotal;
            RuedaDelIzq.Transform = RotarRueda * TraslacionRuedaDelIzq * MovimientoTotal;
            RuedaDelDer.Transform = RotarRueda * FlipRuedaDerecha * TraslacionRuedaDelDer * MovimientoTotal;
            foreach (var maya in Ruedas)
            {
                maya.Render();
            }
        }

        public void Dispose()
        {
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
