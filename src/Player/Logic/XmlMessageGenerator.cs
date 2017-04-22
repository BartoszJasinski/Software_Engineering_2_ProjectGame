using System;
using System.Collections.Generic;
using Common.Schema;

namespace Player.Logic
{
    //TODO delete few messages, because Player probably do not send all of them according to specification
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
                        timestamp = new DateTime(2016, 12, 12),
                        type = PieceType.sham,
                        playerIdSpecified = true,
                        playerId = 1
                    };
                    break;
                case 5:
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
                        PlayerDefinition = new Common.Schema.Player()
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
                        Pieces = new Piece[1] { new Piece() { id = 1, timestamp = new DateTime(2016, 1, 1), type = PieceType.unknown } },
                        GoalFields = new GoalField[1] { new GoalField() { x = 1, y = 1, team = TeamColour.blue, timestamp = new DateTime(2016, 1, 1), type = GoalFieldType.goal } },
                        TaskFields = new TaskField[1] { new TaskField() { distanceToPiece = 2, timestamp = new DateTime(2016, 1, 1), x = 2, y = 2 } }
                    };
                    break;
                case 16:
                    obj = new Game()
                    {
                        playerId = 2,
                        PlayerLocation = new Location() { x = 2, y = 5 },
                        Board = new GameBoard() { goalsHeight = 2, tasksHeight = 2, width = 3 },
                        Players = new Common.Schema.Player[1] { new Common.Schema.Player() { id = 2, team = TeamColour.red, type = PlayerType.leader } }
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
                    obj = new PlayerGood()
                    {
                        playerId = 5
                    };
                    break;
                case 20:
                    obj = new Common.Schema.Player()
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
                        teamColour = TeamColour.blue
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
                default:
                    obj = new RegisterGame()
                    {
                        NewGameInfo = new GameInfo()
                        {
                            blueTeamPlayers = 42,
                            redTeamPlayers = 24,
                            gameName = "defaultgame"
                        }
                    };
                    break;

            }

            return obj;
        }

        public static object GetXmlMessage(string message)
        {
            Dictionary<string, object> xmlMessage = new Dictionary<string, object>();

            xmlMessage.Add("GoalField", new GoalField()
            {
                playerIdSpecified = true,
                playerId = 4,
                team = TeamColour.blue,
                timestamp = new DateTime(2016, 12, 26),
                x = 5,
                y = 1
            });

            xmlMessage.Add("TaskField", new TaskField()
            {
                distanceToPiece = 3,
                pieceIdSpecified = true,
                pieceId = 3,
                playerIdSpecified = true,
                playerId = 2,
                timestamp = new DateTime(2016, 12, 26),
                x = 3,
                y = 1
            });

            xmlMessage.Add("GameBoard", new GameBoard()
            {
                goalsHeight = 3,
                tasksHeight = 4,
                width = 5
            });

            xmlMessage.Add("Location", new Location()
            {
                x = 1,
                y = 5
            });

            xmlMessage.Add("Piece", new Piece()
            {
                id = 1,
                timestamp = new DateTime(2016, 12, 12),
                type = PieceType.sham,
                playerIdSpecified = true,
                playerId = 1
            });

            xmlMessage.Add("RegisterGame", new RegisterGame()
            {
                NewGameInfo = new GameInfo()
                {
                    blueTeamPlayers = 10,
                    redTeamPlayers = 9,
                    gameName = "game"
                }
            });

            xmlMessage.Add("GameInfo", new GameInfo()
            {
                gameName = "game",
                blueTeamPlayers = 12,
                redTeamPlayers = 10
            });

            xmlMessage.Add("AuthorizeKnowledgeExchange", new AuthorizeKnowledgeExchange()
            {
                gameId = 1,
                playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f",
                withPlayerId = 1
            });

            xmlMessage.Add("Discover", new Discover()
            {
                gameId = 1,
                playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
            });

            xmlMessage.Add("Move", new Move()
            {
                playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f",
                direction = MoveType.left,
                directionSpecified = true,
                gameId = 2
            });

            xmlMessage.Add("PickUpPiece", new PickUpPiece()
            {
                playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f",
                gameId = 2
            });

            xmlMessage.Add("PlacePiece", new PlacePiece()
            {
                gameId = 2,
                playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f"
            });

            xmlMessage.Add("TestPiece", new TestPiece()
            {
                playerGuid = "c094cab7-da7b-457f-89e5-a5c51756035f",
                gameId = 1
            });

            xmlMessage.Add("AcceptExchangeRequest", new AcceptExchangeRequest()
            {
                playerId = 2,
                senderPlayerId = 3
            });

            xmlMessage.Add("ConfirmJoiningGame", new ConfirmJoiningGame()
            {
                gameId = 1,
                privateGuid = "c094cab7-da7b-457f-89e5-a5c51756035f",
                playerId = 2,
                PlayerDefinition = new Common.Schema.Player()
                {
                    id = 2,
                    team = TeamColour.red,
                    type = PlayerType.member
                }
            });

            xmlMessage.Add("Data", new Data()
            {
                gameFinished = false,
                playerId = 2,
                PlayerLocation = new Location() { x = 2, y = 1 },
                Pieces = new Piece[1] { new Piece() { id = 1, timestamp = new DateTime(2016, 1, 1), type = PieceType.unknown } },
                GoalFields = new GoalField[1] { new GoalField() { x = 1, y = 1, team = TeamColour.blue, timestamp = new DateTime(2016, 1, 1), type = GoalFieldType.goal } },
                TaskFields = new TaskField[1] { new TaskField() { distanceToPiece = 2, timestamp = new DateTime(2016, 1, 1), x = 2, y = 2 } }
            });

            xmlMessage.Add("Game", new Game()
            {
                playerId = 2,
                PlayerLocation = new Location() { x = 2, y = 5 },
                Board = new GameBoard() { goalsHeight = 2, tasksHeight = 2, width = 3 },
                Players = new Common.Schema.Player[1] { new Common.Schema.Player() { id = 2, team = TeamColour.red, type = PlayerType.leader } }
            });

            xmlMessage.Add("KnowledgeExchangeRequest", new KnowledgeExchangeRequest()
            {
                playerId = 3,
                senderPlayerId = 1
            });

            xmlMessage.Add("RejectKnowledgeExchange", new RejectKnowledgeExchange()
            {
                permanent = false,
                playerId = 1,
                senderPlayerId = 5
            });

            xmlMessage.Add("PlayerMessage", new PlayerGood()
            {
                playerId = 5
            });

            xmlMessage.Add("Common.Schema.Player", new Common.Schema.Player()
            {
                id = 1,
                team = TeamColour.blue,
                type = PlayerType.leader
            });

            xmlMessage.Add("ConfirmGameRegistration", new ConfirmGameRegistration()
            {
                gameId = 5
            });

            xmlMessage.Add("GetGames", new GetGames());

            xmlMessage.Add("JoinGame", new JoinGame()
            {
                gameName = "game",
                preferredRole = PlayerType.leader,
                teamColour = TeamColour.blue
            });

            xmlMessage.Add("RegisteredGames", new RegisteredGames()
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
            });



            return xmlMessage[message];
        }



    }
}
