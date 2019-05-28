using System;
using BulletSharp;
using System.Collections.Generic;
using TGC.Core.BulletPhysics;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{   
   public class FisicaMundo
    {
        private DiscreteDynamicsWorld dynamicsWorld;
        private CollisionDispatcher dispatcher;
        private DefaultCollisionConfiguration collisionConfiguration;
        private SequentialImpulseConstraintSolver constraintSolver;
        private BroadphaseInterface overlappingPairCache;

        private List<TgcMesh> meshes = new List<TgcMesh>();
        private RigidBody piso;
        private TgcMesh auto;
        private RigidBody cuerpoAuto;
        private TGCVector3 adelante;
        private TGCVector3 izquierda_derecha;

        public virtual void Init()
    {
            var cuerpoPiso = new StaticPlaneShape(TGCVector3.Up.ToBulletVector3(), 10);
            cuerpoPiso.LocalScaling = new TGCVector3().ToBulletVector3();
            var movimientoPiso = new DefaultMotionState();
            var pisoConstruccion = new RigidBodyConstructionInfo(0, movimientoPiso, cuerpoPiso);
            piso = new RigidBody(pisoConstruccion);
            piso.Friction = 1;
            piso.RollingFriction = 1;
            piso.Restitution = 1f;
            piso.UserObject = "floorBody";
            dynamicsWorld.AddRigidBody(piso);

            foreach (var mesh in meshes)
        {
            var buildingbody = BulletRigidBodyFactory.Instance.CreateRigidBodyFromTgcMesh(mesh);
            dynamicsWorld.AddRigidBody(buildingbody);
        }
    }

    }
}
