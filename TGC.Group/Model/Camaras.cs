using TGC.Core.Camara;
using TGC.Core.Mathematica;


namespace TGC.Group.Model
{
    public class CamaraAtrasAF : TgcCamera
    {
        private AutoManejable objetivo;
        public CamaraAtrasAF(AutoManejable nuevo_objetivo)
        {
            objetivo = nuevo_objetivo;
            this.SetCamera(PosicionCamaraAtras, CentroDelAuto);
        }
        public TGCVector3 CentroDelAuto { get => new TGCVector3(objetivo.CuerpoRigidoAuto.CenterOfMassPosition.X, objetivo.CuerpoRigidoAuto.CenterOfMassPosition.Y, objetivo.CuerpoRigidoAuto.CenterOfMassPosition.Z); }
        public float distanciaCamaraAtras = 220;
        public float alturaCamaraAtras = 70;
        public float lambda;
        public float Lambda { get => distanciaCamaraAtras / FastMath.Sqrt((FastMath.Pow2(objetivo.VersorDirector.X)) + FastMath.Pow2(objetivo.VersorDirector.Z)); set => lambda = value; }
        private TGCVector3 posicionCamaraAtras;
        public TGCVector3 PosicionCamaraAtras { get => new TGCVector3(CentroDelAuto.X - (Lambda * objetivo.Direccion * objetivo.VersorDirector.X), alturaCamaraAtras, CentroDelAuto.Z - (Lambda * objetivo.Direccion * objetivo.VersorDirector.Z)); set => posicionCamaraAtras = value; }

    }
    public class CamaraAerea : TgcCamera
    {

        public TGCVector3 objetivo;
        TGCVector3 posicionCamaraArea = new TGCVector3(50, 2900, 0);
        public CamaraAerea(TGCVector3 objetivo)
        {
            this.SetCamera(posicionCamaraArea, objetivo);
        }

    }
    public class CamaraEspectador : TgcCamera
    {
        public CamaraEspectador()
        {
            this.SetCamera(new TGCVector3(1370,2300, -3800), new TGCVector3(1370,0,400));
        }
    }

}