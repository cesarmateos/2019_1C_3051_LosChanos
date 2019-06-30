using System;
using System.Collections.Generic;
using TGC.Core.Mathematica;
using TGC.Core.Direct3D;


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
        private CustomSprite Pausa;
        private CustomSprite Alarma;
        private float EscalaVelocimetro;
        private Drawer2D Huds;

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
            EscalaVelocimetro = 0.25f * D3DDevice.Instance.Height / VelocimetroFondo.Bitmap.Size.Height;
            TGCVector2 escalaVelocimetroVector = new TGCVector2(EscalaVelocimetro, EscalaVelocimetro);
            VelocimetroFondo.Scaling = escalaVelocimetroVector;
            VelocimetroAguja.Scaling = escalaVelocimetroVector;
            VelocimetroAguja.RotationCenter = new TGCVector2(VelocimetroAguja.Bitmap.Size.Height * EscalaVelocimetro / 2, VelocimetroAguja.Bitmap.Size.Height * EscalaVelocimetro / 2);

            Barra1.Scaling = escalaInicio;
            Barra2.Scaling = escalaInicio;
            Pausa.Scaling = escalaInicio;
            Alarma.Scaling = escalaInicio;
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
            Pausa.Dispose();
        }
        public void Juego(bool invisibilidadActivada, AutoManejable jugadorActivo, bool juegoDoble, bool pantallaDoble, AutoManejable J1,AutoManejable J2)
        {
            Huds.BeginDrawSprite();
            if (juegoDoble)
            {
                Huds.DrawSprite(Barra2);         
                if (pantallaDoble)
                {
                    HudJugador(J1, 0.03f, 0.7f, pantallaDoble);
                    HudJugador(J2, 0.85f, 0.7f, pantallaDoble);
                }
                else
                {
                    HudJugador(jugadorActivo, 0.03f, 0.7f,pantallaDoble);
                }
            }
            else
            {
                Huds.DrawSprite(Barra1);
                HudJugador(J1, 0.03f, 0.7f, pantallaDoble);
            }
            Huds.EndDrawSprite();
        }
        public void HudJugador(AutoManejable jugador, float posicionPorcentualX, float posicionPorcentualY,bool pantallaDoble)
        {
            VelocimetroAguja.Rotation = jugador.Velocidad / 50;
            VelocimetroFondo.Position = new TGCVector2(FastMath.Max(D3DDevice.Instance.Width * posicionPorcentualX, 0), FastMath.Max(D3DDevice.Instance.Height * posicionPorcentualY, 0));
            VelocimetroAguja.Position = new TGCVector2(FastMath.Max(D3DDevice.Instance.Width * posicionPorcentualX, 0), FastMath.Max(D3DDevice.Instance.Height * posicionPorcentualY, 0));
            if (jugador.Invisible && !pantallaDoble)
            {
                Huds.DrawSprite(Alarma);
            }
            Huds.DrawSprite(VelocimetroFondo);
            Huds.DrawSprite(VelocimetroAguja);
        }
        public void Pausar()
        {
            Huds.BeginDrawSprite();
            Huds.DrawSprite(Pausa);
            Huds.EndDrawSprite();
        }
    }
}
