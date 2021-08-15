using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ACQ.Othello
{
    [Serializable]
    public class Game
    {        
        readonly enPiece m_HumanPlayer; //color of human player        
        enPiece m_current_player = enPiece.Black; //black always moves first

        Dictionary<enPiece, ComputerPlayer> m_vComputerPlayers;
 
        Board m_board;

        LinkedList<Board> m_vBoards;

        Move m_lastMove;

        bool m_bGameOver;
        bool m_bSaved = false;

        int m_depth;
        

        public Game(enPiece humanPlayer, int depth, bool bPerfectEnd)
        {
            m_bGameOver = false;
            m_HumanPlayer = humanPlayer;
            m_depth = depth;

            m_board = new Board();
            m_vBoards = new LinkedList<Board>();

            m_vComputerPlayers = new Dictionary<enPiece, ComputerPlayer>();

            if (m_HumanPlayer == enPiece.Empty)
            {
                m_vComputerPlayers.Add(enPiece.White, new ComputerPlayerBalanced(enPiece.White, depth, 40, 16, 9, bPerfectEnd));
                m_vComputerPlayers.Add(enPiece.Black, new ComputerPlayerBalanced(enPiece.Black, depth, 30, 16, 9, bPerfectEnd));
            }
            else
            {
                enPiece piece = Board.FlipPiece(m_HumanPlayer);

                m_vComputerPlayers.Add(piece, new ComputerPlayerBalanced(piece, depth, 20, 1, 1, bPerfectEnd));
                //m_vComputerPlayers.Add(piece, new ComputerPlayerMasked(piece, depth, bPerfectEnd));
                //m_vComputerPlayers.Add(piece, new ComputerPlayerDeepSampler(piece));
            }
        }

        public Game(Dictionary<enPiece, ComputerPlayer> vComputerPlayers)
            : this(enPiece.Empty, 5, true)
        {
            m_vComputerPlayers = vComputerPlayers;
        }

        public int Depth
        {
            get
            {
                return m_depth;
            }
        }

        public Board CurrentBoard
        {
            get
            {
                return m_board;
            }
        }

        public enPiece CurrentPlayer
        {
            get
            {
                return m_current_player;
            }
        }

        public enPiece HumanPlayer
        {
            get 
            {
                return m_HumanPlayer;
            }
        }       
        
        /// <summary>
        /// Process player move
        /// </summary>
        /// <param name="i">board x position</param>
        /// <param name="j">board y position</param>
        /// <returns>true if move was made false otherwise</returns>
        public bool PlayerMove(int i, int j)
        {
            lock (m_board)
            {
                if (m_bGameOver)
                    return false;

                if (m_current_player == m_HumanPlayer)
                    return MakeMove(new Move(new Position(i, j), m_HumanPlayer), true);
            }
            return false;
        }

        public void Analyse()
        {
            List<ComputerPlayer> vExperts = new List<ComputerPlayer>();

            vExperts.Add(new ComputerPlayerBalanced(this.m_HumanPlayer, 3, 20, 1, 1, true));
            vExperts.Add(new ComputerPlayerBalanced(this.m_HumanPlayer, 4, 20, 1, 1, true));
            vExperts.Add(new ComputerPlayerBalanced(this.m_HumanPlayer, 5, 20, 1, 1, true));
            vExperts.Add(new ComputerPlayerBalanced(this.m_HumanPlayer, 6, 20, 1, 1, true));

            using (StreamWriter sw = new StreamWriter("game.analysis.txt"))
            {
                foreach (ComputerPlayer expert in vExperts)
                {
                    sw.WriteLine(expert);
                    foreach (Board board in m_vBoards)
                    {
                        int nMoveRating;
                        Move move = expert.FindBestMove(board, out nMoveRating);
                        sw.WriteLine("Best Move {0}, position {1}", move, nMoveRating);
                    }
                }
            }
        }

        /// <summary>
        /// Process computer moves
        /// </summary>
        /// <returns>true if move was made false otherwise</returns>
        public bool Play()
        {
            if (m_bGameOver)
                return false;

            lock (m_board)
            {
                if (m_vComputerPlayers.ContainsKey(m_current_player))
                {
                    int nMoveRating;
                    Move move = m_vComputerPlayers[m_current_player].FindBestMove(m_board, out nMoveRating);

                    return MakeMove(move, false);
                }
            }
            return false;
        }

        private bool MakeMove(Move move, bool bSaveBoard)
        {
            if (m_board.IsMoveLegal(move))
            {
                //Save board before the move
                if (bSaveBoard)
                    m_vBoards.AddLast(new Board(m_board));

                //Make move
                m_board.MakeMove(move);

                //Change player
                m_current_player = Board.FlipPiece(m_current_player);

                //store gameover flag
                m_bGameOver = m_board.State.GameOver;

                //if game is not over but current player has no moves then change the current player
                if (!m_bGameOver && !m_board.HasMoves(m_current_player))
                {
                    m_current_player = Board.FlipPiece(m_current_player);
                }
                m_lastMove = move;
                return true;
            }
            else
            {
                return false; //no moves were made
            }
        }

        public bool UndoMove()
        {
            if (m_vBoards.Count !=0)
            {
                m_lastMove = null;
                m_board = (Board)m_vBoards.Last.Value.Clone();
                m_vBoards.RemoveLast();

                m_bGameOver = m_board.State.GameOver;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Returns player with largest number of pieces
        /// </summary>
        public enPiece Winner
        {
            get 
            {
                if (m_board.CountPieces(enPiece.White) > m_board.CountPieces(enPiece.Black))
                    return enPiece.White;
                else if (m_board.CountPieces(enPiece.White) < m_board.CountPieces(enPiece.Black))
                    return enPiece.Black;
                else
                    return enPiece.Empty;
            }
        }

        public bool Over
        {
            get
            {
                return m_bGameOver;
            }
        }

        public Move LastMove
        {
            get
            {
                return m_lastMove;
            }
        }

        public bool Saved
        {
            get
            {
                return m_bSaved;
            }
            set
            {
                m_bSaved = true; //we can;t unsave the game so false can not be set
            }
        }        
    }
}
