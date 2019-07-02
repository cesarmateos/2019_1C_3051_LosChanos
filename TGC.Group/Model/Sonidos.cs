using TGC.Core.Sound;


namespace TGC.Group.Model
{
    public class Sonidos
    {
        //SONIDO ///////////
        //Ambiente
        private TgcStaticSound Musica;
        private TgcStaticSound Tribuna;
        private TgcStaticSound Aplausos;
        private TgcStaticSound GameOver;
        public string MediaDir;
        public Microsoft.DirectX.DirectSound.Device SonidoDevice { get; set; }

        public Sonidos(string mediaDir, Microsoft.DirectX.DirectSound.Device sonidoDevice)
        {
            MediaDir = mediaDir;
            SonidoDevice = sonidoDevice;
            // Sonidos
            int volumen1 = -1800;  // RANGO DEL 0 AL -10000 (Silenciado al -10000)
            var pathMusica = MediaDir + "Musica\\Running90s.wav";
            Musica = new TgcStaticSound();
            Musica.loadSound(pathMusica, volumen1, SonidoDevice);

            int volumen2 = -400;
            var pathTribuna = MediaDir + "Musica\\Tribuna.wav";
            Tribuna = new TgcStaticSound();
            Tribuna.loadSound(pathTribuna, volumen2, SonidoDevice);

            var pathAplausos = MediaDir + "Musica\\Aplausos.wav";
            Aplausos = new TgcStaticSound();
            Aplausos.loadSound(pathTribuna, -100, SonidoDevice);

            var pathGameOver = MediaDir + "Musica\\GameOver.wav";
            GameOver = new TgcStaticSound();
            GameOver.loadSound(pathGameOver, -100, SonidoDevice);
        }
        public void SuenaTribuna()
        {
            Tribuna.play(true);
        }
        public void ParaTribuna()
        {
            Tribuna.stop();
        }
        public void SuenaMusica()
        {
            Musica.play(true);
        }
        public void ParaMusica()
        {
            Musica.stop();
        }
        public void SuenaGameOver() {
            GameOver.play();

        }
        public void SuenaAplausos()
    {
            Aplausos.play();
        }
        public void Dispose()
        {
            Tribuna.dispose();
            Musica.dispose();
            Aplausos.dispose();
            GameOver.dispose();
        }
    }

}
