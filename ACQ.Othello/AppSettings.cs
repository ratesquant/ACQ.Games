using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace ACQ.Othello
{
    [Serializable]
    public class AppSettings : ICloneable
    {
        Color m_board_color;
        Color m_hint_color;
        enPiece m_player;        
        int m_nDepth;

        bool m_bShowAvailableMoves;
        bool m_bShowLastMove;
        bool m_bPerfectEnding;
        bool m_bRateMoves;

        public AppSettings()
        {
            Reset();
        }

        public void CopyTo(AppSettings settings)
        {
            settings.m_board_color = m_board_color;
            settings.m_hint_color = m_hint_color;
            settings.m_player = m_player;            
            settings.m_nDepth = m_nDepth;

            settings.m_bShowAvailableMoves = m_bShowAvailableMoves;
            settings.m_bShowLastMove = m_bShowLastMove;
            settings.m_bPerfectEnding = m_bPerfectEnding;
            settings.m_bRateMoves = m_bRateMoves;
        }

        public object Clone()
        {
            AppSettings settings = new AppSettings();

            CopyTo(settings);

            return settings;
        }
        /// <summary>
        /// Reset to default
        /// </summary>
        public void Reset()
        {
            m_nDepth = 5;
            m_board_color = Color.FromArgb(32, 121, 16);
            m_hint_color = Color.FromArgb(16, 60, 8);
            m_last_move_color = Color.FromArgb(0, 200, 250);            

            m_player = enPiece.White;
            m_bShowAvailableMoves = true;
            m_bShowLastMove = true;
            m_bPerfectEnding = true;
        }

        public int Depth
        {
            get
            {
                return m_nDepth;
            }
            set
            {
                m_nDepth = value;
            }
        }


        public Color BoardColor
        {
            get
            {
                return m_board_color;
            }
        }

        public Color HintColor
        {
            get
            {
                return m_hint_color;
            }
        }

        public enPiece HumanPlayer
        {
            get
            {
                return m_player;
            }
            set
            {
               m_player = value;
            }
        }

        public bool ShowAvailableMoves
        {
            get 
            {
                return m_bShowAvailableMoves;
            }
            set 
            {
                m_bShowAvailableMoves = value;
            }
        }
        public bool PerfectEnding
        {
            get
            {
                return m_bPerfectEnding;
            }
            set
            {
                m_bPerfectEnding = value;
            }
        }
        public bool ShowLastMove
        {
            get
            {
                return m_bShowLastMove;
            }
            set
            {
                m_bShowLastMove = value;
            }
        }
        public bool RateMoves
        {
            get
            {
                return m_bRateMoves;
            }
            set
            {
                m_bRateMoves = value;
            }
        }

        private Color m_last_move_color;

        public Color LastMoveColor
        {
            get { return m_last_move_color; }
            set { m_last_move_color = value; }
        }
	
    }
}
