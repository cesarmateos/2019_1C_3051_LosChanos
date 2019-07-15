using TGC.Core.Shaders;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Shader = Microsoft.DirectX.Direct3D.Effect;
using TGC.Core.Mathematica;
using TGC.Core.Direct3D;

namespace TGC.Group.Model
{
    public class ShaderEnvMap
    {
        Shader EnvMap;
        private TGCVector3 posicionLuz;

        public ShaderEnvMap(string ShadersDir)
        {

            EnvMap = TGCShaders.Instance.LoadEffect(ShadersDir + "\\EnvMap.fx");
            EnvMap.Technique = "RenderScene";
            posicionLuz = new TGCVector3(1500, 600, 1500);

        }
        public void Render(AutoManejable AutoFisico1, AutoManejable AutoFisico2, PoliciasIA GrupoPolicias, Core.Camara.TgcCamera Camara, bool juegoDoble)
        {
            //  Shader Enviroment Map --------------------------------
            //D3DDevice.Instance.Device.EndScene();
            var g_pCubeMap = new CubeTexture(D3DDevice.Instance.Device, 256, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);
            var pOldRT2 = D3DDevice.Instance.Device.GetRenderTarget(0);

            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(90.0f), 1f, 1f, 10000f).ToMatrix();

            // Genero las caras del enviroment map
            for (var nFace = CubeMapFace.PositiveX; nFace <= CubeMapFace.NegativeZ; ++nFace)
            {
                var pFace = g_pCubeMap.GetCubeMapSurface(nFace, 0);
                D3DDevice.Instance.Device.SetRenderTarget(0, pFace);
                TGCVector3 Dir, VUP;
                Color color;
                switch (nFace)
                {
                    default:
                    case CubeMapFace.PositiveX:
                        // Left
                        Dir = new TGCVector3(1, 0, 0);
                        VUP = TGCVector3.Up;
                        color = Color.Black;
                        break;

                    case CubeMapFace.NegativeX:
                        // Right
                        Dir = new TGCVector3(-1, 0, 0);
                        VUP = TGCVector3.Up;
                        color = Color.Red;
                        break;

                    case CubeMapFace.PositiveY:
                        // Up
                        Dir = TGCVector3.Up;
                        VUP = new TGCVector3(0, 0, -1);
                        color = Color.Gray;
                        break;

                    case CubeMapFace.NegativeY:
                        // Down
                        Dir = TGCVector3.Down;
                        VUP = new TGCVector3(0, 0, 1);
                        color = Color.Yellow;
                        break;

                    case CubeMapFace.PositiveZ:
                        // Front
                        Dir = new TGCVector3(0, 0, 1);
                        VUP = TGCVector3.Up;
                        color = Color.Green;
                        break;

                    case CubeMapFace.NegativeZ:
                        // Back
                        Dir = new TGCVector3(0, 0, -1);
                        VUP = TGCVector3.Up;
                        color = Color.Blue;
                        break;
                }

                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, color, 1.0f, 0);
                //Renderizar

                foreach (var mesh in AutoFisico1.Mayas)
                {
                    mesh.Effect = EnvMap;
                    mesh.Technique = "RenderScene";
                    mesh.Render();
                }
                if (juegoDoble)
                {
                    foreach (var mesh in AutoFisico2.Mayas)
                    {
                        mesh.Effect = EnvMap;
                        mesh.Technique = "RenderScene";
                        mesh.Render();
                    }
                }
            }

            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT2);
            D3DDevice.Instance.Device.Transform.View = Camara.GetViewMatrix().ToMatrix();
            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f), D3DDevice.Instance.AspectRatio, 1f, 10000f).ToMatrix();

            //D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            EnvMap.SetValue("g_txCubeMap", g_pCubeMap);

            foreach (var mesh in AutoFisico1.Mayas)
            {
                mesh.Effect = EnvMap;
                mesh.Technique = "RenderScene";
                mesh.Render();
            }
            foreach (var rueda in AutoFisico1.Ruedas)
            {
                rueda.Effect = EnvMap;
                rueda.Technique = "RenderScene";
                rueda.Render();
            }
            foreach (var mesh in GrupoPolicias.Todos[0].Mayas)
            {
                mesh.Effect = EnvMap;
                mesh.Technique = "RenderScene";
                mesh.Render();
            }
            if (juegoDoble)
            {
                foreach (var mesh in AutoFisico2.Mayas)
                {
                    mesh.Effect = EnvMap;
                    mesh.Technique = "RenderScene";
                    mesh.Render();
                }
                foreach (var rueda in AutoFisico2.Ruedas)
                {
                    rueda.Effect = EnvMap;
                    rueda.Technique = "RenderScene";
                    rueda.Render();
                }
            }
            g_pCubeMap.Dispose();
            //-------------------------------------------------------------
        }
    }
    
}
