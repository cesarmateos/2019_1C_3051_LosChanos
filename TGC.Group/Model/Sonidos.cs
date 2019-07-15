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
        private TgcStaticSound Aceleracion;
        private TgcStaticSound Frenada;
        private TgcStaticSound Choque;
        private Tgc3dSound Motor;
        public string MediaDir;
        public Microsoft.DirectX.DirectSound.Device SonidoDevice { get; set; }

        public Auto Auto { get; set; }

        public Sonidos(string mediaDir, Microsoft.DirectX.DirectSound.Device sonidoDevice)
        {
            MediaDir = mediaDir;
            SonidoDevice = sonidoDevice;

            // Sonidos
            Musica = new TgcStaticSound();
            Musica.loadSound(MediaDir + "Musica\\Running90s.wav", -1800, SonidoDevice);

            Tribuna = new TgcStaticSound();
            Tribuna.loadSound(MediaDir + "Musica\\Tribuna.wav", -400, SonidoDevice);

            Aplausos = new TgcStaticSound();
            Aplausos.loadSound(MediaDir + "Musica\\Aplausos.wav", -100, SonidoDevice);

            GameOver = new TgcStaticSound();
            GameOver.loadSound(MediaDir + "Musica\\GameOver.wav", -100, SonidoDevice);

            Aceleracion = new TgcStaticSound();
            Aceleracion.loadSound(MediaDir + "Musica\\Motor2.wav", -1700, SonidoDevice);

            Frenada = new TgcStaticSound();
            Frenada.loadSound(MediaDir + "Musica\\Frenada.wav", -2000, SonidoDevice);

            Choque = new TgcStaticSound();
            Choque.loadSound(MediaDir + "Musica\\Choque1.wav", -2000, SonidoDevice);

            //Motor = new Tgc3dSound(MediaDir + "Musica\\Motor2.wav", Auto.Mayas[0].Position, SonidoDevice);
            //Motor.MinDistance = 80f;
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
        public void SuenaGameOver()
        {
            GameOver.play();
        }
        public void SuenaAplausos()
        {
            Aplausos.play();
        }
        public void SuenaMotor(bool estado, Auto auto)
        {
            Auto = auto;
            //if (estado) {
            //    Motor.play(false);
            //}
            //else {
            //    Motor.stop();
            //}
        }
        public void ParaMotor()
        {
            Aceleracion.stop();
        }
        public void SuenaFrenada()
        {
            Frenada.play();
        }
        public void SuenaChoque()
        {
            Choque.play();
        }
        public void SuenaEncendido(Auto auto)
        {
            Tgc3dSound sonar;

            sonar = new Tgc3dSound(MediaDir + "Musica\\Encendido.wav", auto.Mayas[0].Position, SonidoDevice)
            {
                MinDistance = 80f
            };

            sonar.play();
        }
        public void Dispose()
        {
            Tribuna.dispose();
            Musica.dispose();
            Aplausos.dispose();
            GameOver.dispose();
            Aceleracion.dispose();
            Frenada.dispose();
            Choque.dispose();
        }
    }

}
