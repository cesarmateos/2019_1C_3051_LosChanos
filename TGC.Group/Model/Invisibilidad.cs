using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Shaders;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Shader = Microsoft.DirectX.Direct3D.Effect;
using Device = Microsoft.DirectX.Direct3D.Device;

namespace TGC.Group.Model
{
    public class ShaderInvisibilidad
    {
        public Shader Invisibilidad { get; set; }
        private Surface g_pDepthStencil;
        private Texture g_pRenderTarget;
        private VertexBuffer g_pVBV3D;
        public Device D3DDevice;
        public Surface pOldRT;
        public Surface pSurf;
        public Surface pOldDS;

        public ShaderInvisibilidad(Device d3dDevice, string ShadersDir)
        {
            D3DDevice = d3dDevice;
            //Shader Invisibilidad
            Invisibilidad = TGCShaders.Instance.LoadEffect(ShadersDir + "\\Invisibilidad.fx");
            Invisibilidad.Technique = "DefaultTechnique";

            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
               d3dDevice.PresentationParameters.BackBufferHeight,
               DepthFormat.D24S8, MultiSampleType.None, 0, true);

            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8,
                Pool.Default);

            Invisibilidad.SetValue("g_RenderTarget", g_pRenderTarget);

            // Resolucion de pantalla
            Invisibilidad.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            Invisibilidad.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            CustomVertex.PositionTextured[] vertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            //Vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);

        }
        public void PreRender(bool invisibilidadActivada)
        {
           
            Invisibilidad.Technique = "DefaultTechnique";
            pOldRT = D3DDevice.GetRenderTarget(0);
            pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            if (invisibilidadActivada)
                D3DDevice.SetRenderTarget(0, pSurf);
            pOldDS = D3DDevice.DepthStencilSurface;

            if (invisibilidadActivada)
                D3DDevice.DepthStencilSurface = g_pDepthStencil;

            D3DDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            pSurf.Dispose();

        }
        public void PostRender(bool invisibilidadActivada, float Tiempo)
        {
            pSurf.Dispose();
            if (invisibilidadActivada)
            {
                D3DDevice.DepthStencilSurface = pOldDS;
                D3DDevice.SetRenderTarget(0, pOldRT);
                Invisibilidad.Technique = "PostProcess";
                Invisibilidad.SetValue("time", Tiempo);
                D3DDevice.VertexFormat = CustomVertex.PositionTextured.Format;
                D3DDevice.SetStreamSource(0, g_pVBV3D, 0);
                Invisibilidad.SetValue("g_RenderTarget", g_pRenderTarget);

                D3DDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                Invisibilidad.Begin(FX.None);
                Invisibilidad.BeginPass(0);
                D3DDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                Invisibilidad.EndPass();
                Invisibilidad.End();
            }
        }
        public void Dispose()
        {
            g_pRenderTarget.Dispose();
            g_pVBV3D.Dispose();
            g_pDepthStencil.Dispose();
            Invisibilidad.Dispose();
        }
    }
}
