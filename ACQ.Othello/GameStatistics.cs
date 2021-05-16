using System;
using System.Collections.Generic;
using System.Text;

namespace ACQ.Othello
{
    [Serializable]
    public class GameStatistics
    {
        Dictionary<DateTime, Game> m_vFinishedGames;

        public GameStatistics()
        {
            m_vFinishedGames = new Dictionary<DateTime, Game>();
        }

        public Dictionary<DateTime, Game> Games
        {
            get
            {
                return m_vFinishedGames;
            }
        }

        public int CountWins(int depth)
        {
            int nCount = 0;
            foreach (DateTime date in m_vFinishedGames.Keys)
            {
                if (m_vFinishedGames[date].Winner == m_vFinishedGames[date].HumanPlayer &&  depth == m_vFinishedGames[date].Depth)
                    nCount++;
            }
            return nCount;
        }

        public int CountGames(int depth)
        {
            int nCount = 0;
            foreach (DateTime date in m_vFinishedGames.Keys)
            {
                if (depth == m_vFinishedGames[date].Depth)
                    nCount++;
            }
            return nCount;
        }
    }
}
