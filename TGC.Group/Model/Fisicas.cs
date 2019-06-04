using BulletSharp;
using TGC.Core.Mathematica;


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
        private RigidBody piso;

        public FisicaMundo()
        {

            //Implementación Iniciales
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase();
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, -250f, 0).ToBulletVector3();

 
            var cuerpoPiso = new StaticPlaneShape(TGCVector3.Up.ToBulletVector3(), 0);
            cuerpoPiso.LocalScaling = new TGCVector3().ToBulletVector3();
            MotionState movimientoPiso = new DefaultMotionState();
            var pisoConstruccion = new RigidBodyConstructionInfo(0, movimientoPiso, cuerpoPiso);
            piso = new RigidBody(pisoConstruccion);
            //piso.Friction = 0.1f;
            piso.RollingFriction = 0.5f;
            piso.Restitution = 0.4f;
            piso.UserObject = "floorBody";
            dynamicsWorld.AddRigidBody(piso);

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

        }
    }
}
