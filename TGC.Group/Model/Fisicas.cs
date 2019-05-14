using System;
using BulletSharp;
using System.Collections.Generic;
using TGC.Core.BulletPhysics;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{   
   public class FisicasEdificios
    {
        private DiscreteDynamicsWorld dynamicsWorld;
        private CollisionDispatcher dispatcher;
        private DefaultCollisionConfiguration collisionConfiguration;
        private SequentialImpulseConstraintSolver constraintSolver;
        private BroadphaseInterface overlappingPairCache;

        private List<TgcMesh> meshes = new List<TgcMesh>();

        public void Init()
    {
        foreach(var mesh in meshes)
        {
            var buildingbody = BulletRigidBodyFactory.Instance.CreateRigidBodyFromTgcMesh(mesh);
            dynamicsWorld.AddRigidBody(buildingbody);
        }
    }

    }
}
