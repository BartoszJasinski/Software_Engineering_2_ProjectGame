using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MockPlayer.Logic
{
    public class RandXmlClass
    {
        static Random rng = new Random();

        [TestMethod()]
        public static object GetXmlClass()
        {
            object obj = new object();
            int rand = rng.Next(0, 25);

            //DONT LOOK BELLOW DIS LINE
            switch (rand)
            {
                case 0:
                    var gf = new GoalField();
                    gf.playerIdSpecified = true;
                    gf.playerId = 4;
                    gf.team = TeamColour.blue;
                    gf.timestamp = new DateTime(2016, 12, 26);
                    gf.x = 5;
                    gf.y = 1;
                    obj = gf;
                    break;
                case 1:
                    obj = new TaskField()
                    {
                        distanceToPiece = 3,
                        pieceIdSpecified = true,
                        pieceId = 3,
                        playerIdSpecified = true,
                        playerId = 2,
                        timestamp = new DateTime(2016, 12, 26),
                        x = 3,
                        y = 1
                    };
                    break;
                case 2:
                    obj = new GameBoard()
                    {
                        goalsHeight = 3,
                        tasksHeight = 4,
                        width = 5
                    };
                    break;
                case 3:
                    obj = new Location()
                    {
                        x = 1,
                        y = 5
                    };
                    break;
                case 4:
                    obj = new Piece()
                    {
                        id = 1,
                        timestamp = new DateTime(2016,12,12),
                        type = PieceType.sham,
                        playerIdSpecified = true,
                        playerId = 1
                    };
                    break;
                case 6:
                    obj = new GameInfo()
                    {
                        gameName = "game",
                        blueTeamPlayers = 12,
                        redTeamPlayers = 10
                    };
                    break;
                case 7:
                    obj = new AuthorizeKnowledgeExchange()
                    {
                        gameId = 1,
                        playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f",
                        withPlayerId = 1
                    };
                    break;
                case 8:
                    obj = new Discover()
                    {
                        gameId = 1,
                        playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
                    };
                    break;
                case 9:
                    obj = new Move()
                    {
                        playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f",
                        direction = MoveType.left,
                        directionSpecified = true,
                        gameId = 2
                    };
                    break;
                case 10:
                    obj = new PickUpPiece()
                    {
                        playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f",
                        gameId = 2
                    };
                    break;
                case 11:
                    obj = new PlacePiece()
                    {
                        gameId = 2,
                        playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
                    };
                    break;
                case 12:
                    obj = new TestPiece()
                    {
                        playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f",
                        gameId = 1
                    };
                    break;
                case 13:
                    obj = new AcceptExchangeRequest()
                    {
                        playerId = 2,
                        senderPlayerId = 3
                    };
                    break;
                case 14:
                    obj = new ConfirmJoiningGame()
                    {
                        gameId = 1,
                        privateGuid = "c094cab7-da7b-457f-89e5-a5c51756035f",
                        playerId = 2,
                        PlayerDefinition = new Player()
                        {
                            id = 2,
                            team = TeamColour.red,
                            type = PlayerType.member
                        }
                    }; 
                    break;
                case 15:
                    obj = new Data()
                    {
                        gameFinished = false,
                        playerId = 2,
                        PlayerLocation = new Location() { x = 2, y = 1 },
                        Pieces = new Piece[1] { new Piece() { id = 1, timestamp = new DateTime(2016,1,1), type = PieceType.unknown} },
                        GoalFields = new GoalField[1] { new GoalField() {x = 1, y = 1, team = TeamColour.blue, timestamp = new DateTime(2016,1,1), type = GoalFieldType.goal } },
                        TaskFields = new TaskField[1] { new TaskField() { distanceToPiece = 2, timestamp = new DateTime(2016,1,1), x = 2, y = 2 } }
                    };
                    break;
                case 16:
                    obj = new Game()
                    {
                        playerId = 2,
                        PlayerLocation = new Location() { x = 2, y = 5 },
                        Board = new GameBoard() { goalsHeight = 2, tasksHeight = 2, width = 3 },
                        Players = new Player[1] { new Player() {id = 2, team = TeamColour.red, type = PlayerType.leader } }
                    };
                    break;
                case 17:
                    obj = new KnowledgeExchangeRequest()
                    {
                        playerId = 3,
                        senderPlayerId = 1
                    };
                    break;
                case 18:
                    obj = new RejectKnowledgeExchange()
                    {
                        permanent = false,
                        playerId = 1,
                        senderPlayerId = 5
                    };
                    break;
                case 19:
                    obj = new PlayerMessage()
                    {
                        playerId = 5
                    };
                    break;
                case 20:
                    obj = new Player()
                    {
                        id = 1,
                        team = TeamColour.blue,
                        type = PlayerType.leader
                    };
                    break;
                case 21:
                    obj = new ConfirmGameRegistration()
                    {
                        gameId = 5
                    };
                    break;
                case 22:
                    obj = new GetGames();
                    break;
                case 23:
                    obj = new JoinGame()
                    {
                        gameName = "game",
                        preferredRole = PlayerType.leader,
                        preferredTeam = TeamColour.blue
                    };
                    break;
                case 24:
                    obj = new RegisteredGames()
                    {
                        GameInfo = new GameInfo[1]
                        {
                            new GameInfo()
                            {
                            blueTeamPlayers = 10,
                            redTeamPlayers = 9,
                            gameName = "game"
                            }
                        }
                    };
                    break;
                case 25:
                    obj = new RegisterGame()
                    {
                        NewGameInfo = new GameInfo()
                        {
                            blueTeamPlayers = 10,
                            redTeamPlayers = 9,
                            gameName = "game"
                        }
                    };
                    break;
            }

            return obj;
        }


    }
}
