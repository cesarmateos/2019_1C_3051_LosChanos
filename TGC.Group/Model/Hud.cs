using System;
using System.Collections.Generic;
using TGC.Core.Mathematica;
using TGC.Core.Direct3D;
using Microsoft.DirectX.Direct3D;


namespace TGC.Group.Model
{
    public class Hud
    {
        string MediaDir { get; set; }
        public AutoManejable[] Jugadores { get; set; }


        //Declaro Imagenes del Menu de Inicio
        private Drawer2D Inicio;
        private CustomSprite PantallaInicioFondo;
        private CustomSprite PantallaInicioMenu;
        private CustomSprite PantallaInicioControles;
        private float EscalaInicioAltura;
        private float EscalaInicioAncho;

        //Declaro Cosas de HUD
        private CustomSprite VelocimetroFondo;
        private CustomSprite VelocimetroAguja;
        private CustomSprite Barra1;
        private CustomSprite Barra2;
        private CustomSprite BarraVida;
        private CustomSprite Vida;
        private CustomSprite ChapaInvisibilidad;
        private CustomSprite Invisible;
        private CustomSprite Pausa;
        private CustomSprite Alarma;
        private float EscalaVelocimetro;
        private Drawer2D Huds;

        private CustomSprite GameOver;
        private CustomSprite GanadorJ1;
        private CustomSprite GanadorJ2;

        public Hud(string mediaDir, AutoManejable[] jugadores)
        {
            MediaDir = mediaDir;
            Jugadores = jugadores;

            Inicio = new Drawer2D();

            PantallaInicioFondo = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\InicioFondo.jpg", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };

            PantallaInicioMenu = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\InicioMenu.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            PantallaInicioControles = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\InicioControles.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            EscalaInicioAltura = (float)D3DDevice.Instance.Height / (float)PantallaInicioFondo.Bitmap.Size.Height;
            EscalaInicioAncho = (float)D3DDevice.Instance.Width / (float)PantallaInicioFondo.Bitmap.Size.Width;
            TGCVector2 escalaInicio = new TGCVector2(EscalaInicioAncho, EscalaInicioAltura);
            PantallaInicioFondo.Scaling = escalaInicio;
            PantallaInicioMenu.Scaling = escalaInicio;
            PantallaInicioControles.Scaling = escalaInicio;

            ///// HUD /////
            Huds = new Drawer2D();
            Barra1 = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\Barra1.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            Barra2 = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\Barra2.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            Pausa = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\Pausa.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            Alarma = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\Alarma.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            VelocimetroFondo = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\VelocimetroFondo.png", D3DDevice.Instance.Device),
            };
            VelocimetroAguja = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\VelocimetroAguja.png", D3DDevice.Instance.Device),
            };
            BarraVida = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\BarraVida.png", D3DDevice.Instance.Device),
            };
            Vida = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\Vida.png", D3DDevice.Instance.Device),
            };
            ChapaInvisibilidad = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\ChapaInvisibilidad.png", D3DDevice.Instance.Device),
            };
            Invisible = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\Invisible.png", D3DDevice.Instance.Device),
            };
            EscalaVelocimetro = 0.25f * D3DDevice.Instance.Height / VelocimetroFondo.Bitmap.Size.Height;
            TGCVector2 escalaVelocimetroVector = new TGCVector2(EscalaVelocimetro, EscalaVelocimetro);
            VelocimetroFondo.Scaling = escalaVelocimetroVector;
            VelocimetroAguja.Scaling = escalaVelocimetroVector;
            BarraVida.Scaling = escalaVelocimetroVector;
            ChapaInvisibilidad.Scaling = escalaVelocimetroVector;
            Invisible.Scaling = escalaVelocimetroVector;
            VelocimetroAguja.RotationCenter = new TGCVector2(VelocimetroAguja.Bitmap.Size.Height * EscalaVelocimetro / 2, VelocimetroAguja.Bitmap.Size.Height * EscalaVelocimetro / 2);

            Barra1.Scaling = escalaInicio;
            Barra2.Scaling = escalaInicio;
            Pausa.Scaling = escalaInicio;
            Alarma.Scaling = escalaInicio;


            GameOver = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\GameOver.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            GanadorJ1 = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\GanadorJ1.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            GanadorJ2 = new CustomSprite
            {
                Bitmap = new CustomBitmap(MediaDir + "\\Imagenes\\GanadorJ2.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(0, 0)
            };
            GameOver.Scaling = escalaInicio;
            GanadorJ1.Scaling = escalaInicio;
            GanadorJ2.Scaling = escalaInicio;
        }
        public void PantallaInicio()
        {
            Inicio.BeginDrawSprite();
            Inicio.DrawSprite(PantallaInicioFondo);
            Inicio.DrawSprite(PantallaInicioMenu);
            Inicio.EndDrawSprite();
        }
        public void PantallaControles()
        {
            Inicio.BeginDrawSprite();
            Inicio.DrawSprite(PantallaInicioFondo);
            Inicio.DrawSprite(PantallaInicioControles);
            Inicio.EndDrawSprite();
        }
        public void Dispose()
        {
            PantallaInicioFondo.Dispose();
            PantallaInicioControles.Dispose();
            PantallaInicioMenu.Dispose();
            VelocimetroFondo.Dispose();
            VelocimetroAguja.Dispose();
            BarraVida.Dispose();
            Vida.Dispose();
            ChapaInvisibilidad.Dispose();
            Invisible.Dispose();
            Pausa.Dispose();
            GameOver.Dispose();
        }
        public void Juego(bool invisibilidadActivada, AutoManejable jugadorActivo, bool juegoDoble, bool pantallaDoble, AutoManejable J1, AutoManejable J2)
        {
            Huds.BeginDrawSprite();
            if (juegoDoble)
            {    
                if (pantallaDoble)
                {
                    HudJugador(J1, 0.03f, 0.67f, pantallaDoble);
                    HudJugador(J2, 0.85f, 0.67f, pantallaDoble);
                }
                else
                {
                    HudJugador(jugadorActivo, 0.03f, 0.67f, pantallaDoble);
                }
                Huds.DrawSprite(Barra2);
            }
            else
            {
                HudJugador(J1, 0.03f, 0.67f, pantallaDoble);
                Huds.DrawSprite(Barra1);     
            }
            Huds.EndDrawSprite();
        }
        public void HudJugador(AutoManejable jugador, float posicionPorcentualX, float posicionPorcentualY, bool pantallaDoble)
        {
            var posicionEnX = D3DDevice.Instance.Width * posicionPorcentualX;
            var posicionEnY = D3DDevice.Instance.Height * posicionPorcentualY;
            var ratioVida = jugador.Vida / 1000;
            var extraPixelesVida = 118 * EscalaVelocimetro;
            Vida.Scaling = new TGCVector2(ratioVida * (EscalaVelocimetro+0.01f), EscalaVelocimetro);
            VelocimetroAguja.Rotation = jugador.Velocidad / 50;
            VelocimetroFondo.Position = new TGCVector2(FastMath.Max(posicionEnX, 0), FastMath.Max(posicionEnY, 0));
            VelocimetroAguja.Position = new TGCVector2(FastMath.Max(posicionEnX, 0), FastMath.Max(posicionEnY, 0));
            BarraVida.Position = new TGCVector2(FastMath.Max(posicionEnX, 0), FastMath.Max(D3DDevice.Instance.Height * (posicionPorcentualY+0.245f), 0));
            Vida.Position = new TGCVector2(FastMath.Max(posicionEnX+extraPixelesVida, 0), FastMath.Max(D3DDevice.Instance.Height * (posicionPorcentualY + 0.245f), 0));
            ChapaInvisibilidad.Position = new TGCVector2(FastMath.Max(posicionEnX, 0), FastMath.Max(D3DDevice.Instance.Height * (posicionPorcentualY + 0.285f), 0));
            Invisible.Position = new TGCVector2(FastMath.Max(posicionEnX, 0), FastMath.Max(D3DDevice.Instance.Height * (posicionPorcentualY + 0.285f), 0));

            Huds.DrawSprite(VelocimetroFondo);
            Huds.DrawSprite(VelocimetroAguja);
            Huds.DrawSprite(BarraVida);
            Huds.DrawSprite(Vida);
            Huds.DrawSprite(ChapaInvisibilidad);
            if (jugador.Invisible)
            {
                Huds.DrawSprite(Invisible);
                if (!pantallaDoble)
                {
                    Huds.DrawSprite(Alarma);
                }
            }
        }
        public void Pausar()
        {
            Huds.BeginDrawSprite();
            Huds.DrawSprite(Pausa);
            Huds.EndDrawSprite();
        }
        public void JuegoTerminado()
        {

            Huds.BeginDrawSprite();
            Huds.DrawSprite(GameOver);
            Huds.EndDrawSprite();
        }
        public void GanoJ1()
        {

            Huds.BeginDrawSprite();
            Huds.DrawSprite(GanadorJ1);
            Huds.EndDrawSprite();
        }
        public void GanoJ2()
        {

            Huds.BeginDrawSprite();
            Huds.DrawSprite(GanadorJ2);
            Huds.EndDrawSprite();
        }
    }
}
