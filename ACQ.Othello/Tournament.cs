using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ACQ.Othello
{
    class GameTask : ACQ.Core.ITask
    {
        Dictionary<enPiece, ComputerPlayer> m_players;
        int m_score = 0;

        public GameTask(Dictionary<enPiece, ComputerPlayer> players)
        {
            m_players = players;
        }

        public void run()
        {
            Game game = new Game(m_players);

            while (!game.Over)
            {
                game.Play();
            }
            //record score
            int nWhite = game.CurrentBoard.CountPieces(enPiece.White);
            int nBlack = game.CurrentBoard.CountPieces(enPiece.Black);

            m_score = nWhite - nBlack;
        }

        public int Score
        {
            get
            {
                return m_score;
            }
        }
    }
    /// <summary>
    /// Tournament between computer players and benchmark
    /// </summary>
    class Tournament
    {
        Dictionary<enPiece, List<ComputerPlayer>> m_vPlayers = new Dictionary<enPiece,List<ComputerPlayer>>();
        ComputerPlayer m_vBenchmark;

        public Tournament()            
        {
            int depth = 1;

            foreach (enPiece piece in new enPiece[] { enPiece.Black, enPiece.White })
            {
                List<ComputerPlayer> players = new List<ComputerPlayer>();
                
                for (int i = 0; i < 2; i++)
                    players.Add(new ComputerPlayerBalanced(piece, depth, 20, 1, i, true));

                for (int i = 0; i < 2; i++)
                    players.Add(new ComputerPlayerBalanced(piece, depth, 20, i, 0, true));

                players.Add(new ComputerPlayerCount(piece, depth, true));
                players.Add(new ComputerPlayerMasked(piece, depth, true));
                players.Add(new ComputerPlayerCombination(piece, depth));
                players.Add(new ComputerPlayerMobility(piece, depth));
                players.Add(new ComputerPlayer(piece, depth, true));
                players.Add(new ComputerPlayer3rd(piece, depth));
                players.Add(new ComputerPlayerRandom(piece));

                m_vPlayers[piece] = players;
            }
        }

        public void PlayInThreads()
        {
            ACQ.Core.BlockinQueue queue = new ACQ.Core.BlockinQueue(4);

            int nCount = m_vPlayers[enPiece.White].Count;

            int[,] vScore = new int[nCount, nCount];

            GameTask[,] vGames = new GameTask[nCount, nCount];

            for (int i = 0; i < nCount; i++)
            {
                for (int j = 0; j < nCount; j++)
                {
                    Dictionary<enPiece, ComputerPlayer> players = new Dictionary<enPiece, ComputerPlayer>();

                    players.Add(enPiece.White, m_vPlayers[enPiece.White][i]);
                    players.Add(enPiece.Black, m_vPlayers[enPiece.Black][j]);

                    GameTask task = new GameTask(players);

                    queue.EnqueueItem(task);

                    vGames[i, j] = task;
                }
            }

            queue.Shutdown(true);

            for (int i = 0; i < nCount; i++)
            {
                for (int j = 0; j < nCount; j++)
                {
                    vScore[i, j] = vGames[i, j].Score;
                }
            }

            PrintScore(vScore, "Tournament.Scores.txt");
        }

        public void Play()
        {
            int nCount = m_vPlayers[enPiece.White].Count;

            int[,] vScore = new int[nCount, nCount];

            for (int i = 0; i < nCount; i++)
            {
                for (int j = 0; j < nCount; j++)
                {
                    Dictionary<enPiece, ComputerPlayer> players = new Dictionary<enPiece, ComputerPlayer>();

                    players.Add(enPiece.White, m_vPlayers[enPiece.White][i]);
                    players.Add(enPiece.Black, m_vPlayers[enPiece.Black][j]);

                    Game game = new Game(players);

                    while (!game.Over)
                    {
                        game.Play();
                    }
                    //record score
                    int nWhite = game.CurrentBoard.CountPieces(enPiece.White);
                    int nBlack = game.CurrentBoard.CountPieces(enPiece.Black);

                    vScore[i, j] = nWhite - nBlack;
                }
            }

            PrintScore(vScore, "Tournament.Scores.txt");
        }

        private void PrintScore(int[,] vScore, string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine("Score (white - black), rows - > white, column -> black");
                for (int j = 0; j < m_vPlayers[enPiece.White].Count; j++)
                {
                    sw.WriteLine("{0,4}: {1}", j, m_vPlayers[enPiece.White][j].ToString());
                }
                sw.WriteLine("");

                sw.Write("P\t"); 
                for (int j = 0; j < vScore.GetLength(1); j++)
                {
                    sw.Write("{0,4}\t", j);                                    
                }
                sw.WriteLine();

                for (int i = 0; i < vScore.GetLength(0); i++)
                {
                    sw.Write("{0,4}\t", i); 

                    for (int j = 0; j < vScore.GetLength(1); j++)
                    {
                        sw.Write("{0,4}\t", vScore[i, j]);                                    
                    }
                    sw.WriteLine();
                }
            }
        }
    }
}
