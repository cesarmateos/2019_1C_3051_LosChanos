using System;
using Microsoft.DirectX.DirectInput;
using BulletSharp;
using System.Collections.Generic;
using TGC.Core.BulletPhysics;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;



namespace TGC.Group.Model
{
    public class FisicaMundo
    {
        //Declaro Iniciales
        public DiscreteDynamicsWorld dynamicsWorld;
        private CollisionDispatcher dispatcher;
        private DefaultCollisionConfiguration collisionConfiguration;
        private SequentialImpulseConstraintSolver constraintSolver;
        private BroadphaseInterface overlappingPairCache;

        //Escenario
        //private List<TgcMesh> Edificios = new List<TgcMesh>();
        private RigidBody piso;
        private RigidBody ParedSur { get; set; }
        //public void CargarEdificios(List<TgcMesh> meshes)
        //{
        //    Edificios = meshes;
        //}

        public FisicaMundo()
        {

            //Implementación Iniciales
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase();
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, -100f, 0).ToBulletVector3();

            ////Se hacen los cuerpos rígidos del escenario.
            //foreach (var mesh in Edificios)
            //{
            //    var objetos = BulletRigidBodyFactory.Instance.CreateRigidBodyFromTgcMesh(mesh);
            //    dynamicsWorld.AddRigidBody(objetos);
            //}

            // Estructura del piso
            var cuerpoPiso = new StaticPlaneShape(TGCVector3.Up.ToBulletVector3(), 10);
            cuerpoPiso.LocalScaling = new TGCVector3().ToBulletVector3();
            MotionState movimientoPiso = new DefaultMotionState();
            var pisoConstruccion = new RigidBodyConstructionInfo(0, movimientoPiso, cuerpoPiso);
            piso = new RigidBody(pisoConstruccion);
            piso.Friction = 0.1f;
            piso.RollingFriction = 1;
            piso.Restitution = 0.2f;
            piso.UserObject = "floorBody";
            dynamicsWorld.AddRigidBody(piso);

            var paredSurForma = new BoxShape(new BulletSharp.Math.Vector3(-1000, 0, -100));
            paredSurForma.LocalScaling = new TGCVector3(-2000, 10, 200).ToBulletVector3();
            var paredSurConstruccion = new RigidBodyConstructionInfo(0, movimientoPiso, paredSurForma);
            ParedSur = new RigidBody(paredSurConstruccion);
            dynamicsWorld.AddRigidBody(ParedSur);

            //ParedSur = BulletRigidBodyFactory.Instance.CreateBox(new TGCVector3(5000,10,200), 10000000000, new TGCVector3(0,0,-1400), 0, 0, 0, 1f, false);
            //dynamicsWorld.AddRigidBody(ParedSur);

        }
        public void Render(float tiempo)
        {
            //    //Hacemos render de la escena.
            //    foreach (var mesh in Edificios) mesh.Render();
        }

        public void Dispose()
        {
            //Se hace dispose del modelo fisico.
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
            piso.Dispose();

            //Dispose de Meshes
            //foreach (var maya in Edificios)
            //{
            //    maya.Dispose();
            //}
        }
    }
}
