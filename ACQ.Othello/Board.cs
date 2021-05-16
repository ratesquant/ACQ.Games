using System;
using System.Collections.Generic;
using System.Text;

namespace ACQ.Othello
{
    [Serializable]
    public enum enPiece
    {
        Black = -1,
        White = 1,
        Empty = 0
    };

    [Serializable]
    public class Position
    {
        int x;
        int y;
        public Position(int i, int j)
        {
            x = i;
            y = j;
        }
        public Position(Position p)
        {
            x = p.x;
            y = p.y;
        }
        public int X
        {
            get
            {
                return x;
            }
        }
        public int Y
        {
            get
            {
                return y;
            }
        }

        public static bool operator ==(Position pos1, Position pos2)
        {
            if (pos1.x == pos2.x && pos1.y == pos2.y)
                return true;
            else
                return false;
        }
        public static bool operator !=(Position pos1, Position pos2)
        {
            return !(pos1 == pos2);
        }
        public override int GetHashCode()
        {
            return x ^ y;
        }
        public override string ToString()
        {
            return String.Format("({0}; {1})", x, y);
        }
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Position p = obj as Position;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }
        public bool Equals(Position p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }
    };

    [Serializable]
    public class Move
    {
        enPiece m_piece;
        Position m_to;

        public Move(Position to, enPiece piece)
        {
            m_piece = piece;
            m_to = to;
        }
        public Move(int i, int j, enPiece piece)
            : this(new Position(i, j), piece){}

        public Move(Move move)
        {
            m_piece = move.m_piece;
            m_to = move.m_to;
        }

        public Position To
        {
            get
            {
                return m_to;
            }
        }

        public enPiece Piece
        {
            get
            {
                return m_piece;
            }

        }

        public override string ToString()
        {
            return String.Format("{0} to {1}", m_piece, m_to);
        }
    };

    [Serializable]
    public class BoardState
    {
        private bool m_bBlackMoves;
        private bool m_bWhiteMoves;

        public BoardState(Board board)
        {
            m_bBlackMoves = board.HasMoves(enPiece.Black);
            m_bWhiteMoves = board.HasMoves(enPiece.White);
        }
        
        public bool GameOver
        {
            get
            {
                if (m_bBlackMoves || m_bWhiteMoves)
                    return false;
                else
                    return true;
            }
        }

        public bool BlackMoves
        {
            get 
            {
                return m_bBlackMoves;
            }
        }
        public bool WhiteMoves
        {
            get
            {
                return m_bWhiteMoves;
            }
        }
    }
    
    [Serializable]
    public class Board : ICloneable
    {
        #region Members
        const int m_nTotalDir = 8; 
        static readonly int[] dx;
        static readonly int[] dy;

        readonly int m_size = 8;

        enPiece[,] m_board;
        List<Move> m_MoveHist;
        #endregion

        #region Constructors
        static Board()
        {
            dx = new int[] { 0, 0, 1, -1, 1, -1, -1, 1 };
            dy = new int[] { 1, -1, 0, 0, 1, 1, -1, -1 };
        }
        public Board()
        {
            m_board = new enPiece[m_size, m_size];
            m_MoveHist = new List<Move>();
            Init();
        }
        public Board(Board board)
        {
            m_size = board.m_size;
            m_board = (enPiece[,])board.m_board.Clone();

            m_MoveHist = new List<Move>(board.m_MoveHist);
            m_MoveHist.Capacity = m_size * m_size;            
        }
        public object Clone()
        {
            return new Board(this);
        }
        public void Init()
        {
            for (int i = 0; i < m_size; i++)
                for (int j = 0; j < m_size; j++)
                    m_board[i, j] = enPiece.Empty;

            //center
            int cp = m_size / 2;

            m_board[cp - 1, cp - 1] = enPiece.White;
            m_board[cp, cp] = enPiece.White;

            m_board[cp - 1, cp] = enPiece.Black;
            m_board[cp, cp - 1] = enPiece.Black;
        }
        #endregion

        #region BoardInfo

        public int Size
        {
            get
            {
                return m_size;
            } 
        }

        public enPiece this[int i, int j]
        {
            get
            {
                return m_board[i, j];
            }
        }

        public int CountPieces(enPiece piece)
        {
            int nCount = 0;

            for (int i = 0; i < m_size; i++)
                for (int j = 0; j < m_size; j++)
                    if (m_board[i, j] == piece)
                        nCount++;
            return nCount;
        }

        //count available moves
        public int CountMoves(enPiece piece)
        {
            int nCount = 0;

            for (int i = 0; i < m_size; i++)
            {
                for (int j = 0; j < m_size; j++)
                {
                    if (m_board[i, j] == enPiece.Empty)
                    {
                        if (IsMoveLegal(i, j, piece))
                            nCount++;
                    }
                }
            }
            return nCount;
        }

        //Check if the figure index is on board
        public bool IsOnBoard(Position pos)
        {
            return (pos.X >= 0 && pos.X < m_size && pos.Y >= 0 && pos.Y < m_size);
        }

        //check if there are any moves available
        public bool HasMoves(enPiece piece)
        {
            for (int i = 0; i < m_size; i++)
            {
                for (int j = 0; j < m_size; j++)
                {
                    if (m_board[i, j] == enPiece.Empty)
                    {
                        if (IsMoveLegal(i, j, piece))
                            return true;
                    }
                }
            }
            return false;
        }
        public BoardState State
        {
            get
            {
                return new BoardState(this);
            }
        }
        #endregion

        #region MakingMoves

        public bool IsMoveLegal(Move move)
        {
            //if not on board   - then not legal
            if (!IsOnBoard(move.To))
                return false;

            //if cell is not free then not legal
            if (m_board[move.To.X, move.To.Y] != enPiece.Empty)
                return false;

            return IsMoveLegal(move.To.X, move.To.Y, move.Piece);
        }
             
        //Calculate all possible moves
        //this is INTERNAL function 
        //1. the move coordinates should be on the board  
        //2. board cell should be empty 
        // this function does not check these things!!!
        private bool IsMoveLegal(int move_x, int move_y, enPiece piece)
        {
            //All Game Logic is here
            bool bIsFound;
            int x, y, k;

            enPiece move_fig = piece;

            int nCount;
            for (k = 0; k < m_nTotalDir; k++)
            {
                x = move_x + dx[k];
                y = move_y + dy[k];

                nCount = 0;
                bIsFound = false;

                //move in that direction until (end of board || empty cell || same color figure)
                while (Search(x, y, move_fig, ref bIsFound))
                {
                    x += dx[k];
                    y += dy[k];
                    nCount++;
                }

                if (bIsFound && nCount != 0)
                    return true;

            }

            return false;
        }

        private bool Search(int i, int j, enPiece test_piece, ref bool bIsFound)
        {
            //terminate search, since not on board
            if (i < 0 || i >= m_size || j < 0 || j >= m_size)
                return false;

            enPiece piece = m_board[i, j];

            //terminate search, since cell is empty
            if (piece == enPiece.Empty)
                return false;

            //figure of the same color
            if (piece == test_piece)
            {
                bIsFound = true;
                return false;
            }

            //continue
            return true;
        }

        public List<Move> GetAvailableMoves(enPiece piece)
        {
            List<Move> vMoves = new List<Move>();	                    

            for (int i = 0; i < m_size; i++)
            {
                for (int j = 0; j < m_size; j++)
                {
                    if (m_board[i, j] == enPiece.Empty)
                    {
                        if (IsMoveLegal(i, j, piece))
                        {
                            vMoves.Add(new Move(i,j,piece));
                        }
                    }
                }
            }
            return vMoves;
        }

        public void MakeBlindMove(Move move)
        {
            m_MoveHist.Add(move);
            m_board[move.To.X, move.To.Y] = move.Piece;

            //Game Logic is here
            int x, y, k;

            int nCount;
            bool bIsFound;
            for (k = 0; k < m_nTotalDir; k++)
            {
                x = move.To.X + dx[k];
                y = move.To.Y + dy[k];
                nCount = 0;

                //move in that direction until meet (end of board || empty cell || oposite figure)
                bIsFound = false;

                while (Search(x, y, move.Piece, ref bIsFound))
                {
                    x += dx[k];
                    y += dy[k];
                    nCount++;
                }

                if (bIsFound)
                {
                    while (nCount>0)
                    {
                        x -= dx[k];
                        y -= dy[k];
                        nCount--;
                        m_board[x, y] = move.Piece;
                    }
                }
            }
        }

        public bool MakeMove(Move move)
        {
            bool bIsLegal = IsMoveLegal(move);

            if (!bIsLegal)
            {
                return bIsLegal;
            }

            MakeBlindMove(move);

            return bIsLegal;
        }

        

        #endregion

        public void PrintMoves()
        {
            foreach (Move move in m_MoveHist)            
                Console.Write("{0} ", move);

            Console.WriteLine();
        }
               
        public static enPiece FlipPiece(enPiece piece)
        {
            if (piece == enPiece.White)
                return enPiece.Black;
            else if (piece == enPiece.Black)
                return enPiece.White;
            else
                return piece;
        }        
    };
}
