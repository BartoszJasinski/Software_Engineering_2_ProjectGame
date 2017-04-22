using System;
using System.Collections.Generic;
using Common.Schema;

namespace GameMaster.Logic
{
    //TODO delete few messages, because GM probably do not send all of them according to specification
    public class XmlMessageGenerator
    {

        static Random rng = new Random();

        public static object GetXmlMessage()
        {
            object obj = new object();
            int rand = rng.Next(0, 25);

            switch (rand)
            {
                case 0:
                    obj = new GoalField()
                    {
                        playerId = 1,
                        playerIdSpecified = true,
                        team = TeamColour.red,
                        timestamp = new DateTime(2000, 12, 1),
                        type = GoalFieldType.goal,
                        x = 10,
                        y = 5
                    };

                    break;
                case 1:
                    obj = new TaskField()
                    {
                        distanceToPiece = 1,
                        pieceId = 2,
                        playerIdSpecified = true,
                        pieceIdSpecified = true,
                        playerId = 3,
                        timestamp = new DateTime(2000, 3, 2),
                        x = 10,
                        y = 2
                    };
                    break;
                case 2:
                    obj = new GameBoard()
                    {
                        goalsHeight = 10,
                        tasksHeight = 5,
                        width = 20
                    };
                    break;
                case 3:
                    obj = new Location()
                    {
                        x = 2,
                        y = 2
                    };
                    break;
                case 4:
                    obj = new Piece()
                    {
                        id = 1,
                        playerId = 2,
                        playerIdSpecified = true,
                        timestamp = new DateTime(2000, 3, 3),
                        type = PieceType.normal
                    };
                    break;
                case 6:
                    obj = new GameInfo()
                    {
                        blueTeamPlayers = 3,
                        redTeamPlayers = 2,
                        gameName = "gamename"
                    };
                    break;
                case 7:
                    obj = new AuthorizeKnowledgeExchange()
                    {
                        gameId = 2,
                        playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f",
                        withPlayerId = 3
                    };
                    break;
                case 8:
                    obj = new Discover()
                    {
                        gameId = 3,
                        playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
                    };
                    break;
                case 9:
                    obj = new Move()
                    {
                        direction = MoveType.down,
                        directionSpecified = true,
                        gameId = 3,
                        playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
                    };
                    break;
                case 10:
                    obj = new PickUpPiece()
                    {
                        gameId = 4,
                        playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
                    };
                    break;
                case 11:
                    obj = new PlacePiece()
                    {
                        gameId = 3,
                        playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
                    };
                    break;
                case 12:
                    obj = new TestPiece()
                    {
                        gameId = 3,
                        playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
                    };
                    break;
                case 13:
                    obj = new AcceptExchangeRequest()
                    {
                        playerId = 3,
                        senderPlayerId = 5
                    };
                    break;
                case 14:
                    obj = new ConfirmJoiningGame()
                    {
                        gameId = 3,
                        playerId = 2,
                        PlayerDefinition = new Common.Schema.Player() { id = 3, team = TeamColour.red, type = PlayerType.leader },
                        privateGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
                    };
                    break;
                case 15:
                    obj = new Data()
                    {
                        gameFinished = true,
                        GoalFields = new GoalField[] { new GoalField() { team = TeamColour.red, timestamp = new DateTime(2000, 2, 2), type = GoalFieldType.goal, x = 3, y = 2 } },
                        Pieces = new Piece[] { new Piece() { id = 3, timestamp = new DateTime(2000, 2, 2), type = PieceType.normal } },
                        playerId = 3,
                        PlayerLocation = new Location() { x = 3, y = 2 },
                        TaskFields = new TaskField[] {new TaskField()
                        {
                        distanceToPiece = 1,
                        pieceId = 2,
                        playerIdSpecified = true,
                        pieceIdSpecified = true,
                        playerId = 3,
                        timestamp = new DateTime(2000, 3, 2),
                        x = 10,
                        y = 2
                        } }
                    };
                    break;
                case 16:
                    obj = new Game()
                    {
                        Board = new GameBoard() { goalsHeight = 3, tasksHeight = 4, width = 7 },
                        playerId = 3,
                        PlayerLocation = new Location() { x = 3, y = 2 },
                        Players = new Common.Schema.Player[] { new Common.Schema.Player() { id = 3, team = TeamColour.red, type = PlayerType.leader } }
                    };
                    break;
                case 17:
                    obj = new KnowledgeExchangeRequest()
                    {
                        playerId = 3,
                        senderPlayerId = 4
                    };
                    break;
                case 18:
                    obj = new RejectKnowledgeExchange()
                    {
                        playerId = 3,
                        permanent = true,
                        senderPlayerId = 4
                    };
                    break;
                case 19:
                    obj = new PlayerGood()
                    {
                        playerId = 3
                    };
                    break;
                case 20:
                    obj = new Common.Schema.Player()
                    {
                        id = 3,
                        type = PlayerType.leader,
                        team = TeamColour.blue
                    };
                    break;
                case 21:
                    obj = new ConfirmGameRegistration()
                    {
                        gameId = 2
                    };
                    break;
                case 22:
                    obj = new GetGames();
                    break;
                case 23:
                    obj = new JoinGame()
                    {
                        gameName = "gamename",
                        preferredRole = PlayerType.leader,
                        teamColour = TeamColour.red
                    };
                    break;
                case 24:
                    obj = new RegisteredGames()
                    {
                        GameInfo = new GameInfo[] { new GameInfo() { blueTeamPlayers = 3, gameName = "gamename", redTeamPlayers = 3 } }
                    };
                    break;
                case 25:
                    obj = new RegisterGame()
                    {
                        NewGameInfo = new GameInfo() { blueTeamPlayers = 3, gameName = "gamename", redTeamPlayers = 3 }
                    };
                    break;
                default:
                    RegisterGame rg = new RegisterGame();
                    rg.NewGameInfo = new GameInfo();
                    rg.NewGameInfo.blueTeamPlayers = 69;
                    rg.NewGameInfo.redTeamPlayers = 96;
                    rg.NewGameInfo.gameName = "test";
                    obj = rg;
                    break;
            }

            return obj;
        }


        public static object GetXmlMessage(string message)
        {
            Dictionary<string, object> xmlMessages = new Dictionary<string, object>();

            xmlMessages.Add("GoalField", new GoalField()
            {
                playerId = 1,
                playerIdSpecified = true,
                team = TeamColour.red,
                timestamp = new DateTime(2000, 12, 1),
                type = GoalFieldType.goal,
                x = 10,
                y = 5
            });

            xmlMessages.Add("TaskField", new TaskField()
            {
                distanceToPiece = 1,
                pieceId = 2,
                playerIdSpecified = true,
                pieceIdSpecified = true,
                playerId = 3,
                timestamp = new DateTime(2000, 3, 2),
                x = 10,
                y = 2
            });

            xmlMessages.Add("GameBoard", new GameBoard()
            {
                goalsHeight = 10,
                tasksHeight = 5,
                width = 20
            });

            xmlMessages.Add("Location", new Location()
            {
                x = 2,
                y = 2
            });

            xmlMessages.Add("Piece", new Piece()
            {
                id = 1,
                playerId = 2,
                playerIdSpecified = true,
                timestamp = new DateTime(2000, 3, 3),
                type = PieceType.normal
            });

            xmlMessages.Add("GameInfo", new GameInfo()
            {
                blueTeamPlayers = 3,
                redTeamPlayers = 2,
                gameName = "gamename"
            });

            xmlMessages.Add("AuthorizeKnowledgeExchange", new AuthorizeKnowledgeExchange()
            {
                gameId = 2,
                playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f",
                withPlayerId = 3
            });

            xmlMessages.Add("Discover", new Discover()
            {
                gameId = 3,
                playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
            });

            xmlMessages.Add("Move", new Move()
            {
                direction = MoveType.down,
                directionSpecified = true,
                gameId = 3,
                playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
            });

            xmlMessages.Add("PickUpPiece", new PickUpPiece()
            {
                gameId = 4,
                playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
            });

            xmlMessages.Add("PlacePiece", new PlacePiece()
            {
                gameId = 3,
                playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
            });

            xmlMessages.Add("TestPiece", new TestPiece()
            {
                gameId = 3,
                playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
            });

            xmlMessages.Add("AcceptExchangeRequest", new AcceptExchangeRequest()
            {
                playerId = 3,
                senderPlayerId = 5
            });

            xmlMessages.Add("ConfirmJoiningGame", new ConfirmJoiningGame()
            {
                gameId = 3,
                playerId = 2,
                PlayerDefinition = new Common.Schema.Player() { id = 3, team = TeamColour.red, type = PlayerType.leader },
                privateGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
            });

            xmlMessages.Add("Data", new Data()
            {
                gameFinished = true,
                GoalFields = new GoalField[] { new GoalField() { team = TeamColour.red, timestamp = new DateTime(2000, 2, 2), type = GoalFieldType.goal, x = 3, y = 2 } },
                Pieces = new Piece[] { new Piece() { id = 3, timestamp = new DateTime(2000, 2, 2), type = PieceType.normal } },
                playerId = 3,
                PlayerLocation = new Location() { x = 3, y = 2 },
                TaskFields = new TaskField[] {new TaskField()
                        {
                        distanceToPiece = 1,
                        pieceId = 2,
                        playerIdSpecified = true,
                        pieceIdSpecified = true,
                        playerId = 3,
                        timestamp = new DateTime(2000, 3, 2),
                        x = 10,
                        y = 2
                        } }
            });

            xmlMessages.Add("Game", new Game()
            {
                Board = new GameBoard() { goalsHeight = 3, tasksHeight = 4, width = 7 },
                playerId = 3,
                PlayerLocation = new Location() { x = 3, y = 2 },
                Players = new Common.Schema.Player[] { new Common.Schema.Player { id = 3, team = TeamColour.red, type = PlayerType.leader } }
            });

            xmlMessages.Add("KnowledgeExchangeRequest", new KnowledgeExchangeRequest()
            {
                playerId = 3,
                senderPlayerId = 4
            });

            xmlMessages.Add("RejectKnowledgeExchange", new RejectKnowledgeExchange()
            {
                playerId = 3,
                permanent = true,
                senderPlayerId = 4
            });

            xmlMessages.Add("PlayerMessage", new PlayerGood()
            {
                playerId = 3
            });

            xmlMessages.Add("Player", new Common.Schema.Player()
            {
                id = 3,
                type = PlayerType.leader,
                team = TeamColour.blue
            });

            xmlMessages.Add("ConfirmGameRegistration", new ConfirmGameRegistration()
            {
                gameId = 2
            });

            xmlMessages.Add("GetGames", new GetGames());
            xmlMessages.Add("JoinGame", new JoinGame()
            {
                gameName = "gamename",
                preferredRole = PlayerType.leader,
                teamColour = TeamColour.red
            });

            xmlMessages.Add("RegisteredGames", new RegisteredGames()
            {
                GameInfo = new GameInfo[] { new GameInfo() { blueTeamPlayers = 3, gameName = "gamename", redTeamPlayers = 3 } }
            });
            xmlMessages.Add("RegisterGame", new RegisterGame()
            {
                NewGameInfo = new GameInfo() { blueTeamPlayers = 3, gameName = "gamename", redTeamPlayers = 3 }
            });
            
            return xmlMessages[message];
        }



    }
}
