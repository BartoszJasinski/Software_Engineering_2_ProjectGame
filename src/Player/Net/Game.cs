using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;
using System.Threading;
using Common.Config;
using Common.DebugUtils;
using Common.Message;
using Common.IO.Console;
using Player.Strategy;

namespace Player.Net
{
    public class Game : IGame
    {
        private ulong _gameId;
        private string _guid;
        private Common.Schema.TeamColour _team;
        private Common.Schema.Player[] _players;
        private IList<Piece> _pieces;
        private GameBoard _board;
        private Common.Schema.PlayerType _type;
        private Common.SchemaWrapper.Field[,] _fields;

        private const int NO_PIECE = -1;

        public Game()
        {
            Pieces = new List<Piece>();
        }

        public ulong GameId
        {
            get { return _gameId; }
            set { _gameId = value; }
        }

        public ulong Id { get; set; }

        public Common.Schema.Player[] Players
        {
            set { _players = value; }
            get { return _players; }
        }

        public Common.SchemaWrapper.Field[,] Fields
        {
            set { _fields = value; }
            get { return _fields; }
        }


        public GameBoard Board
        {
            set { _board = value; }
            get { return _board; }
        }

        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public Common.Schema.TeamColour Team
        {
            get { return _team; }
            set { _team = value; }
        }

        public Common.Schema.Location Location { get; set; }
        

        public PlayerType Type
        {
            get { return _type; }

            set { _type = value; }
        }

        public IList<Piece> Pieces
        {
            get { return _pieces; }

            set { _pieces = value; }
        }




        public void UpdateFields(Common.SchemaWrapper.Field[] newTaskFields)
        {
            foreach (Common.SchemaWrapper.Field taskField in newTaskFields)
            {
                Fields[taskField.X, taskField.Y] = taskField;
            }
        }
        

        public void PrintBoard()
        {
            BoardPrinter.Print(Fields);
        }
    }//class
}
