using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ACQ.MandelbrotExplorer
{
    enum enColorPalette
    {
        Grey, Red, Blue
    }

    abstract class ColorPalette
    {
        protected int[] m_palette;
        public ColorPalette(int n_colors = 256)
        {
            m_palette = new int[n_colors];            

            Init(m_palette);
        }
        protected abstract void Init(int[] palette);

        public int this[int i]
        {
            get 
            {
                return m_palette[i];
            }            
        }

        public int this[int i, int max_i]
        {
            get
            {
                return m_palette[(i * (m_palette.Length - 1) / max_i) ];
            }
        }

        protected void Interpolate(int[] palette, Color[] lut)
        {
            for (int i = 0; i < palette.Length; i++)
            {
                double x = (double)i * (lut.Length - 1) / (palette.Length - 1);
                int lut_index1 = (int)(x);
                lut_index1 = lut_index1 < (lut.Length - 1) ? lut_index1 : lut_index1 - 1;

                double delta = x - lut_index1;

                int value_r = (int)(lut[lut_index1].R * (1 - delta) + lut[lut_index1 + 1].R * delta);
                int value_g = (int)(lut[lut_index1].G * (1 - delta) + lut[lut_index1 + 1].G * delta);
                int value_b = (int)(lut[lut_index1].B * (1 - delta) + lut[lut_index1 + 1].B * delta);

                palette[i] = Color.FromArgb(value_r, value_g, value_b).ToArgb();
            }
            palette[palette.Length - 1] = Color.FromArgb(0, 0, 0).ToArgb();
        }
    }

    class ColorPaletteRed : ColorPalette
    {
        public ColorPaletteRed(int n_colors) : base(n_colors)
        {
        }

        protected override void Init(int[] palette)
        {
            for (int i = 0; i < palette.Length; i++)
            {
                int value = (int)(128.0 * i / (palette.Length - 1));
                int value_rg = (int)(255.0 * i / (palette.Length - 1));
                palette[i] = Color.FromArgb(value + 127, value_rg, value_rg).ToArgb();
            }
            palette[palette.Length - 1] = Color.FromArgb(0, 0, 0).ToArgb();
        }
    }

    class ColorPaletteGrayscale : ColorPalette
    {
        public ColorPaletteGrayscale(int n_colors) : base(n_colors)
        {
        }

        protected override void Init(int[] palette)
        {
            for (int i = 0; i < palette.Length; i++)
            {
                double col = Math.Pow((double)i / (palette.Length - 1), 0.7);

                int value = (int) (255.0 * col);
                palette[i] = Color.FromArgb(255 - value, 255 - value, 255 - value).ToArgb();
            }
        }
    }

    class ColorPaletteBlue : ColorPalette
    {
        public ColorPaletteBlue(int n_colors) : base(n_colors)
        {
        }

        protected override void Init(int[] palette)
        {
            for (int i = 0; i < palette.Length; i++)
            {
                double col = Math.Pow((double)i / (palette.Length - 1), 0.7);

                int value = (int)(200.0 * col);
                int value_rg = (int)(255.0 * col);
                palette[i] = Color.FromArgb(value_rg, value_rg, value+55).ToArgb();
            }
            palette[palette.Length-1] = Color.FromArgb(0, 0, 0).ToArgb();
        }
    }

    class ColorPaletteJet : ColorPalette
    {
        Color[] m_lut = new Color[] { 
            Color.FromArgb(0, 0, 128), Color.FromArgb(0, 0, 255),
            Color.FromArgb(0, 128, 255), Color.FromArgb(0, 255, 255),
            Color.FromArgb(128, 255, 128), Color.FromArgb(255, 255, 0),
            Color.FromArgb(255, 128, 0), Color.FromArgb(255, 0, 0), Color.FromArgb(128, 0, 0)   };
        public ColorPaletteJet(int n_colors) : base(n_colors)
        {
            
        }
        protected override void Init(int[] palette)
        {
            Interpolate(palette, m_lut);
        }
    }

    class ColorPaletteCubeHelix : ColorPalette
    {
        Color[] m_lut = new Color[] {
            Color.FromArgb(0, 0, 0),
            Color.FromArgb(22, 12, 31), Color.FromArgb(26, 33, 62),
            Color.FromArgb(22, 61, 78), Color.FromArgb(23, 90, 73),
            Color.FromArgb(43, 11, 57), Color.FromArgb(84,121, 47),
            Color.FromArgb(135, 122, 58), Color.FromArgb(181,121, 94),
            Color.FromArgb(208, 126, 147), Color.FromArgb(212, 144, 198),
            Color.FromArgb(202, 171, 232), Color.FromArgb(193, 202, 243),
            Color.FromArgb(200, 228, 240), Color.FromArgb(224, 245, 240),Color.FromArgb(255, 255, 255)};
        public ColorPaletteCubeHelix(int n_colors) : base(n_colors)
        {
           
        }
        protected override void Init(int[] palette)
        {
            Interpolate(palette, m_lut);
        }
    }
}
