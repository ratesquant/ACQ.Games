//#define PERFORMANCE_LOG

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ACQ.Core;

namespace ACQ.Othello
{
    [Serializable]
    public class Player
    {
        public delegate void MoveEventHandler(object sender, Move move);
        public event MoveEventHandler MoveEvent;

        public virtual void OnMove(object sender, Move move)
        {
            if (MoveEvent != null)
            {
                MoveEvent(this, move);
            }
        }

        protected readonly enPiece m_piece;
        public Player(enPiece piece)
        {
            m_piece = piece;
        }

        public enPiece Piece
        {
            get
            {
                return m_piece;
            }
        }
    }

    [Serializable]
    class HumanPlayer : Player
    {
        public HumanPlayer(enPiece piece)
            : base(piece)
        {
        }

        public void Move(int i, int j)
        {
            OnMove(this, new Move(i, j, m_piece)); 
        }
    }

    [Serializable]
    public class ComputerPlayerMobility : ComputerPlayer
    {
        public ComputerPlayerMobility(enPiece piece, int nDepth)
            : base(piece, nDepth, true)
        {
        }

        public override string ToString()
        {
            return String.Format("ComputerPlayerMobility {0} depth: {1}", m_piece, Depth);
        }

        protected override int GetHeuristic(Board board, BoardState state)
        {
            if (board.CountPieces(enPiece.Empty)<12)
            {
                int whiteCount = board.CountPieces(enPiece.White);
                int blackCount = board.CountPieces(enPiece.Black);

                return whiteCount - blackCount;
            }
            else
            {
                //Count Pieces
                int nMobility = (board.CountMoves(enPiece.White) - board.CountMoves(enPiece.Black));
                return nMobility;
            }
        }
    }

    [Serializable]
    public class ComputerPlayerRandom : ComputerPlayer
    {
        System.Random m_rng = new Random();

        public ComputerPlayerRandom(enPiece piece)
            : base(piece, 1, false)
        {
        }

        public override string ToString()
        {
            return String.Format("ComputerPlayerRandom {0} depth: {1}", m_piece, Depth);
        }

        protected override int GetHeuristic(Board board, BoardState state)
        {
            return m_rng.Next();
        }
    }

    [Serializable]
    public class ComputerPlayerCombination : ComputerPlayer
    {
        System.Random m_rng = new Random();

        public ComputerPlayerCombination(enPiece piece, int depth)
            : base(piece, depth, false)
        {
        }

        public override string ToString()
        {
            return String.Format("ComputerPlayerCombination {0} depth: {1}", m_piece, Depth);
        }

        protected override int GetHeuristic(Board board, BoardState state)
        {
            return m_rng.Next();
        }
    }

    [Serializable]
    public class ComputerPlayer3rd : ComputerPlayer
    {        
        public ComputerPlayer3rd(enPiece piece, int nDepth)
            : base(piece, nDepth, true)
        {
        }

        public override string ToString()
        {
            return String.Format("ComputerPlayer3rd {0} depth: {1}", m_piece, Depth);
        }

        protected override int GetHeuristic(Board board, BoardState state)
        {
            int my_tiles = 0, opp_tiles = 0, i, j, k, my_front_tiles = 0, opp_front_tiles = 0, x, y;
            double p = 0, c = 0, l = 0, m = 0, f = 0, d = 0;
            int[] X1 = new int[] {-1, -1, 0, 1, 1, 1, 0, -1};
            int[] Y1 = new int[] {0, 1, 1, 1, 0, -1, -1, -1};

            int[,] V = new int[,] { 
            {20, -3, 11, 8, 8, 11, -3, 20},
            {-3, -7, -4, 1, 1, -4, -7, -3},
            {11, -4, 2, 2, 2, 2, -4, 11},
            {8, 1, 2, -3, -3, 2, 1, 8},
            {8, 1, 2, -3, -3, 2, 1, 8},
            {11, -4, 2, 2, 2, 2, -4, 11},
            {-3, -7, -4, 1, 1, -4, -7, -3},
            {20, -3, 11, 8, 8, 11, -3, 20}};
            // Piece difference, frontier disks and disk squares
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    if (board[i, j] == enPiece.White)
                    {
                        d += V[i, j];
                        my_tiles++;
                    }
                    else if (board[i, j] == enPiece.Black)
                    {
                        d -= V[i, j];
                        opp_tiles++;
                    }
                    if (board[i, j] != enPiece.Empty)
                    {
                        for (k = 0; k < 8; k++)
                        {
                            x = i + X1[k]; y = j + Y1[k];
                            
                            if (x >= 0 && x < 8 && y >= 0 && y < 8 && board[x, y] == enPiece.Empty)
                            {
                                if (board[i, j] == enPiece.White) my_front_tiles++;
                                else opp_front_tiles++;
                                break;
                            }
                        }
                    }
                }
            }
            
            p = (100.0 * (my_tiles - opp_tiles)) / (my_tiles + opp_tiles);

            if (state.GameOver)
            {
                return (int)Math.Round(p);
            }

            f = -(100.0 * (my_front_tiles - opp_front_tiles)) / (my_front_tiles + opp_front_tiles);

            // Corner occupancy
            my_tiles = opp_tiles = 0;
            if (board[0, 0] == enPiece.White) my_tiles++;
            else if (board[0, 0] == enPiece.Black) opp_tiles++;
            if (board[0, 7] == enPiece.White) my_tiles++;
            else if (board[0, 7] == enPiece.Black) opp_tiles++;
            if (board[7, 0] == enPiece.White) my_tiles++;
            else if (board[7, 0] == enPiece.Black) opp_tiles++;
            if (board[7,7] == enPiece.White) my_tiles++;
            else if (board[7,7] == enPiece.Black) opp_tiles++;
            c = 25 * (my_tiles - opp_tiles);
            // Corner closeness
            my_tiles = opp_tiles = 0;
            if (board[0, 0] == enPiece.Empty)
            {
                if (board[0,1] == enPiece.White) my_tiles++;
                else if (board[0,1] == enPiece.Black) opp_tiles++;
                if (board[1,1] == enPiece.White) my_tiles++;
                else if (board[1,1] == enPiece.Black) opp_tiles++;
                if (board[1,0] == enPiece.White) my_tiles++;
                else if (board[1,0] == enPiece.Black) opp_tiles++;
            }
            if (board[0, 7] == enPiece.Empty)
            {
                if (board[0,6] == enPiece.White) my_tiles++;
                else if (board[0,6] == enPiece.Black) opp_tiles++;
                if (board[1,6] == enPiece.White) my_tiles++;
                else if (board[1,6] == enPiece.Black) opp_tiles++;
                if (board[1,7] == enPiece.White) my_tiles++;
                else if (board[1,7] == enPiece.Black) opp_tiles++;
            }
            if (board[7,0] == enPiece.Empty)
            {
                if (board[7,1] == enPiece.White) my_tiles++;
                else if (board[7,1] == enPiece.Black) opp_tiles++;
                if (board[6,1] == enPiece.White) my_tiles++;
                else if (board[6,1] == enPiece.Black) opp_tiles++;
                if (board[6,0] == enPiece.White) my_tiles++;
                else if (board[6, 0] == enPiece.Black) opp_tiles++;
            }
            if (board[7,7] == enPiece.Empty)
            {
                if (board[6,7] == enPiece.White) my_tiles++;
                else if (board[6,7] == enPiece.Black) opp_tiles++;
                if (board[6,6] == enPiece.White) my_tiles++;
                else if (board[6,6] == enPiece.Black) opp_tiles++;

                if (board[7,6] == enPiece.White) my_tiles++;
                else if (board[7,6] == enPiece.Black) opp_tiles++;
            }
            l = -12.5 * (my_tiles - opp_tiles);
            // Mobility            
            my_tiles = board.CountMoves(enPiece.White);
            opp_tiles = board.CountMoves(enPiece.Black);
            
            m = (100.0 * (my_tiles - opp_tiles)) / (my_tiles + opp_tiles);
            
            // final weighted score
            double score = (10 * p) + (801.724 * c) + (382.026 * l) + (78.922 * m) + (74.396 * f) + (10 * d);
            return (int)Math.Round(score);
        }
    }



    [Serializable]
    public class ComputerPlayer : Player
    {
        #region Members
        readonly int m_nDepth;
        readonly bool m_bPerfectFinish;
        
        [NonSerialized]
        HRTimer m_timer;

        int m_nMinMaxCount;
        int m_nMinMaxDepth;

        protected struct Node
        {
            public Board board;
            public int depth;
            public bool max;
            public int alpha;
            public int beta;
        };

        #endregion

        #region Constructors
        public ComputerPlayer(enPiece piece)
            : this(piece, 4, true) { }


        public ComputerPlayer(enPiece piece, int nDepth, bool bPerfectFinish)
            : base(piece)
        {
            m_nDepth = nDepth;            
            
            m_timer = new HRTimer();

            m_bPerfectFinish = bPerfectFinish;
        }

        #endregion

        public override string ToString()
        {
            return String.Format("ComputerPlayer {0} depth: {1}", m_piece, m_nDepth);
        }
        public int Depth
        {
            get
            {
                return m_nDepth;
            }
        }

        public virtual Move FindBestMove(Board board, out int nMoveRating)
        {
            bool bSimpleMinMax = true;

            m_timer.tic();

            Move best_move = null;
            Node node = new Node();

            m_nMinMaxCount = 0;
            m_nMinMaxDepth = m_nDepth;

            if (m_bPerfectFinish && board.CountPieces(enPiece.Empty) <= 11)
                m_nMinMaxDepth = 12;

            int nBestMoveRating;

            //Simple minimax interface
            if (bSimpleMinMax)
            {
                node.depth = 0;
                node.max = (m_piece == enPiece.White);
                node.board = (Board)board.Clone();
                nBestMoveRating = minimax_ab(node, Int32.MinValue, Int32.MaxValue, ref best_move);
            }
            else
            {
                //Move advanced interface that allows to analyse all moves
                nBestMoveRating = (m_piece == enPiece.White) ? Int32.MinValue : Int32.MaxValue;

                node.depth = 1;
                node.max = !(m_piece == enPiece.White);

                List<Move> vMoves = board.GetAvailableMoves(m_piece);

                //Cycle over ALL moves        		
                foreach (Move move in vMoves)
                {
                    node.board = (Board)board.Clone();
                    node.board.MakeBlindMove(move);

                    nMoveRating = minimax_ab_advanced(node, Int32.MinValue, Int32.MaxValue);

                    Console.WriteLine("Move {0} {1}", move, nMoveRating);

                    if ((m_piece == enPiece.White) && nMoveRating > nBestMoveRating)
                    {
                        nBestMoveRating = nMoveRating;
                        best_move = move;
                    }
                    else if ((m_piece == enPiece.Black) && nMoveRating < nBestMoveRating)
                    {
                        nBestMoveRating = nMoveRating;
                        best_move = move;
                    }
                }
            }

            nMoveRating = nBestMoveRating;

            //Print log message before returning 
            double time = m_timer.toc();
            string log_message = String.Format("Rating: {0} depth {1} in {2:F4} sec, total nodes: {3} ( {4:F0} nodes/sec) {5}", nBestMoveRating, m_nMinMaxDepth, time, m_nMinMaxCount, m_nMinMaxCount / time, best_move);

            Console.WriteLine(log_message);

#if PERFORMANCE_LOG
            using (StreamWriter sr = new StreamWriter("perf.log.txt", true))
            {
                sr.Write(DateTime.Now);
                sr.WriteLine(log_message);
            }
#endif
            return best_move;
        }       

        #region MiniMaxLogic
        /// <summary>
        /// some peoples suggest using: nCount + 3*nMobility + 10*Mask + 5*stable
        /// </summary>
        /// <param name="board"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected virtual int GetHeuristic(Board board, BoardState state)
        {
            //Count Pieces
            int whiteCount = board.CountPieces(enPiece.White);
            int blackCount = board.CountPieces(enPiece.Black);
            return whiteCount - blackCount;
        }
        private int minimax_ab(Node node, int nAlpha, int nBeta, ref Move best_move)
        {
            node.alpha = nAlpha;
            node.beta = nBeta;

            BoardState state = node.board.State;

            //check if this is the last node
            if (node.depth > m_nMinMaxDepth || state.GameOver)
            {
                m_nMinMaxCount++;
                return GetHeuristic(node.board, state);
            }
            else
            {
                //if node is white and there are no moves for white change node to black
                if (node.max && !state.WhiteMoves)
                    node.max = false;

                //if node is black and there are no moves for black change node to white
                if (!node.max && !state.BlackMoves)
                    node.max = true;

                //calculate all posible moves 
                int nResult;

                //Create new node and copy all node moves
                Node new_node;
                new_node.depth = node.depth + 1;
                new_node.alpha = node.alpha;
                new_node.beta = node.beta;

                //Get Positions From the Board
                enPiece piece = node.max ? enPiece.White : enPiece.Black;

                List<Move> vMoves = node.board.GetAvailableMoves(piece);

                int best_score = node.max ? Int32.MinValue : Int32.MaxValue;

                //Cycle over ALL new nodes        		
                foreach (Move move in vMoves)
                {
                    new_node.max = !node.max;
                    new_node.board = (Board)node.board.Clone();
                    new_node.board.MakeBlindMove(move);

                    nResult = minimax_ab(new_node, node.alpha, node.beta, ref best_move);

                    if (node.max)
                    {
                        //best_score = Math.Max(best_score, nResult);
                        //max node Alpha = max(Alpha, Result)
                        if (node.alpha < nResult)
                        {
                            node.alpha = nResult;
                            if (node.depth == 0)
                                best_move = move;
                        }
                        //Beta is the rating of the best move from min perspective
                        //min will choose move with Beta thus if our Alpha is higher then he will never let us play this move 
                        if (node.beta <= node.alpha)
                        {
                           // System.Diagnostics.Debug.Assert(best_score == node.alpha);
                            return node.alpha;
                        }
                    }
                    else
                    {
                        //best_score = Math.Min(best_score, nResult);
                        //min node Beta = min(Beta, Result)
                        if (node.beta > nResult)
                        {
                            node.beta = nResult;
                            if (node.depth == 0)
                                best_move = move;
                        }
                        //Cut off                         
                        if (node.beta <= node.alpha)
                        {
                            //System.Diagnostics.Debug.Assert(best_score == node.beta);
                            return node.beta;
                        }
                    }

                }//end of cycle 
                return node.max ? node.alpha : node.beta;
            }
        }
        private int minimax_ab_advanced(Node node, int nAlpha, int nBeta)
        {
            node.alpha = nAlpha;
            node.beta = nBeta;

            BoardState state = node.board.State;

            //check if this is the last node
            if (node.depth > m_nMinMaxDepth || state.GameOver)
            {
                m_nMinMaxCount++;
                return GetHeuristic(node.board, state);
            }
            else
            {
                //if node is white and there are no moves for white change node to black
                if (node.max && !state.WhiteMoves)
                    node.max = false;

                //if node is black and there are no moves for black change node to white
                if (!node.max && !state.BlackMoves)
                    node.max = true;
                //calculate all posible moves 
                int nResult;

                //Create new node and copy all node moves
                Node new_node;
                new_node.depth = node.depth + 1;
                new_node.alpha = node.alpha;
                new_node.beta = node.beta;

                //Get Positions From the Board
                enPiece piece = node.max ? enPiece.White : enPiece.Black;

                List<Move> vMoves = node.board.GetAvailableMoves(piece);

                //Cycle over ALL new nodes        		
                foreach (Move move in vMoves)
                {
                    new_node.max = !node.max;
                    new_node.board = (Board)node.board.Clone();
                    new_node.board.MakeBlindMove(move);

                    nResult = minimax_ab_advanced(new_node, node.alpha, node.beta);                    

                    if (node.max)
                    {
                        //max node Alpha = max(Alpha, Result)
                        if (node.alpha < nResult)
                            node.alpha = nResult;
                        //Beta is the rating of the best move from min perspective
                        //min will choose move with Beta thus if our Alpha is higher then he will never let us play this move 
                        if (node.beta <= node.alpha)
                            return node.alpha;

                    }
                    else
                    {
                        //min node Beta = min(Beta, Result)
                        if (node.beta > nResult)
                            node.beta = nResult;
                        //Cut off                         
                        if (node.beta <= node.alpha)
                            return node.beta;
                    }

                }//end of cycle 
                return node.max ? node.alpha : node.beta;
            }
        }
        #endregion
    }

    [Serializable]
    public class ComputerPlayerMasked : ComputerPlayer
    {
        #region Members       
        readonly int[,] m_vMask;
        #endregion

        #region Constructors
        public ComputerPlayerMasked(enPiece piece, int nDepth, bool bPerfectFinish)
            : base(piece, nDepth, bPerfectFinish)
        {
            //mask table was taken from http://samsoft.org.uk/reversi/
            //used by Microsoft in their reversi program
            //Sum of all elements is 352
            m_vMask = new int[,] { 
                { 99,  -8,  8,  6,  6,  8,  -8, 99 },
                { -8, -24, -4, -3, -3, -4, -24, -8 },
                {  8,  -4,  7,  4,  4,  7,  -4,  8 },
				{  6,  -3,  4,  0,  0,  4,  -3,  6 },
				{  6,  -3,  4,  0,  0,  4,  -3,  6 },
				{  8,  -4,  7,  4,  4,  7,  -4,  8 },
				{ -8, -24, -4, -3, -3, -4, -24, -8 },
				{ 99,  -8,  8,  6,  6,  8,  -8, 99 },
            };
        }

        #endregion



        protected override int GetHeuristic(Board board, BoardState state)
        {
            int nMask = MaskBasedScore(board);

            return nMask;
        }

        public override string ToString()
        {
            return String.Format("ComputerPlayerMasked {0} depth: {1}", m_piece, Depth);
        }

        public int MaskBasedScore(Board board)
        {
            int nMaskScore = 0;
            for (int i = 0; i < board.Size; i++)
            {
                for (int j = 0; j < board.Size; j++)
                {
                    nMaskScore += (int)board[i, j] * m_vMask[i, j];
                }
            }
            return nMaskScore;
        }
    }

    [Serializable]
    public class ComputerPlayerCount : ComputerPlayer
    {        
        #region Constructors
        public ComputerPlayerCount(enPiece piece, int nDepth, bool bPerfectFinish)
            : base(piece, nDepth, bPerfectFinish)
        {          
        }

        #endregion

        protected override int GetHeuristic(Board board, BoardState state)
        {
            int whiteCount = board.CountPieces(enPiece.White);
            int blackCount = board.CountPieces(enPiece.Black);
            int nCount = whiteCount - blackCount;
            
            return nCount;
        }

        public override string ToString()
        {
            return String.Format("ComputerPlayerCount {0} depth: {1}", m_piece, Depth);
        }       
    }

    [Serializable]
    public class ComputerPlayerBalanced : ComputerPlayer
    {
        #region Members
        readonly int m_nMobilityWeight;
        readonly int m_nMaskWeight;
        readonly int m_nCountWeight;        
        readonly int[,] m_vMask;
        #endregion

        #region Constructors
        public ComputerPlayerBalanced(enPiece piece)
            : this(piece, 4, 20, 1, 1, true){}
       
        public ComputerPlayerBalanced(enPiece piece, int nDepth, int nMobilityWeight, int nMaskWeight, int nCountWeight, bool bPerfectFinish)
            : base(piece, nDepth, bPerfectFinish)
        {
            m_nMobilityWeight = nMobilityWeight;            
            m_nMaskWeight = nMaskWeight;
            m_nCountWeight = nCountWeight;            
            
            //mask table was taken from http://samsoft.org.uk/reversi/
            //used by Microsoft in their reversi program
            //Sum of all elements is 352
            m_vMask = new int[,] { 
                { 99,  -8,  8,  6,  6,  8,  -8, 99 },
                { -8, -24, -4, -3, -3, -4, -24, -8 },
                {  8,  -4,  7,  4,  4,  7,  -4,  8 },
				{  6,  -3,  4,  0,  0,  4,  -3,  6 },
				{  6,  -3,  4,  0,  0,  4,  -3,  6 },
				{  8,  -4,  7,  4,  4,  7,  -4,  8 },
				{ -8, -24, -4, -3, -3, -4, -24, -8 },
				{ 99,  -8,  8,  6,  6,  8,  -8, 99 },
            };
        }

        #endregion



        protected override int GetHeuristic(Board board, BoardState state)
        {
            //Count Pieces
            int whiteCount = board.CountPieces(enPiece.White);
            int blackCount = board.CountPieces(enPiece.Black);
            int nCount = whiteCount - blackCount;

            if (state.GameOver)
                return 10000 * nCount;

            //Mobility  
            int nMobility = (board.CountMoves(enPiece.White) - board.CountMoves(enPiece.Black));

            //Mask
            int nMask = MaskBasedScore(board);

            return m_nMobilityWeight * nMobility + m_nMaskWeight * nMask + m_nCountWeight * nCount;
        }

        public override string ToString()
        {
            return String.Format("ComputerPlayerBalanced {0} depth: {1}, mob_w {2}, mask_w {3}, count_w {4}", m_piece, Depth, m_nMobilityWeight, m_nMaskWeight, m_nCountWeight);
        }      

        public int MaskBasedScore(Board board)
        {
            int nMaskScore = 0;
            for (int i = 0; i < board.Size; i++)
            {
                for (int j = 0; j < board.Size; j++)
                {
                    nMaskScore += (int)board[i, j] * m_vMask[i, j];
                }
            }
            return nMaskScore;
        }
    }
}

