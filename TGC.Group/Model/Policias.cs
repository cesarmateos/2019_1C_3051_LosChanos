using System;
using System.Collections.Generic;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public class PoliciasIA
    {
        private AutoIA Policia01 { get; set; }
        private AutoIA Policia02 { get; set; }
        private AutoIA Policia03 { get; set; }
        private AutoIA Policia04 { get; set; }
        private AutoIA Policia05 { get; set; }
        private AutoIA Policia06 { get; set; }
        private AutoIA Policia07 { get; set; }
        private AutoIA Policia08 { get; set; }
        private AutoIA Policia09 { get; set; }
        private AutoIA Policia10 { get; set; }
        public List<AutoIA> GrupoPolicias { get; set; }

        public PoliciasIA(List<TgcMesh> mayas, FisicaMundo fisica, string pathHumo, AutoManejable[] enemigos)
        {
            //Policia01 = new AutoIA(mayas, rueda, new TGCVector3(-1000, 0, 0), 270, fisica, sombra, pathHumo, enemigos);
            //Policia02 = new AutoIA(mayas, rueda, new TGCVector3(0, 0, 0), 270, fisica, sombra, pathHumo, enemigos);
            //Policia03 = new AutoIA(mayas, rueda, new TGCVector3(1000, 0, 0), 270, fisica, sombra, pathHumo, enemigos);
            //Policia04 = new AutoIA(mayas, rueda, new TGCVector3(2000, 0, 0), 270, fisica, sombra, pathHumo, enemigos);
            //Policia05 = new AutoIA(mayas, rueda, new TGCVector3(3000, 0, 0), 270, fisica, sombra, pathHumo, enemigos);
            //Policia06 = new AutoIA(mayas, rueda, new TGCVector3(-1000, 0, 300), 270, fisica, sombra, pathHumo, enemigos);
            //Policia07 = new AutoIA(mayas, rueda, new TGCVector3(0, 0, 300), 270, fisica, sombra, pathHumo, enemigos);
            //Policia08 = new AutoIA(mayas, rueda, new TGCVector3(1000, 0, 300), 270, fisica, sombra, pathHumo, enemigos);
            //Policia09 = new AutoIA(mayas, rueda, new TGCVector3(2000, 0, 300), 270, fisica, sombra, pathHumo, enemigos);
            //Policia10 = new AutoIA(mayas, rueda, new TGCVector3(3000, 0, 300), 270, fisica, sombra, pathHumo, enemigos);
            //GrupoPolicias = new List<AutoIA>
            //{
            //    Policia01,
            //    Policia02,
            //    Policia03,
            //    Policia04,
            //    Policia05,
            //    Policia06,
            //    Policia07,
            //    Policia08,
            //    Policia09,
            //    Policia10
            //};
        }
        public void Update()
        {
            foreach(var policia in GrupoPolicias)
            {
                policia.Moverse();
            }
        }
        public void Render(float ElapsedTime)
        {
            foreach (var policia in GrupoPolicias)
            {
                policia.Render(ElapsedTime);
            }
        }
        public void Dispose()
        {
            Policia01.Dispose();
        }
    }
}
