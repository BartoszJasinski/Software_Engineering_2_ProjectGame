using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MockGameMaster.Logic
{
    public class RandXmlClass
    {
        static Random rng = new Random();
        [TestMethod()]
        public static object GetXmlClass()
        {
            object obj = new object();
            int rand = rng.Next(30, 35);

            //DONT LOOK BELLOW DIS LINE
            switch (rand)
            {
                case 0:
                    obj = new GoalField();
                    break;
                case 1:
                    obj = new TaskField();
                    break;
                case 2:
                    obj = new GameBoard();
                    break;
                case 3:
                    obj = new Location();
                    break;
                case 4:
                    obj = new Piece();
                    break;
                case 5:
                    obj = new GameFinished();
                    break;
                case 6:
                    obj = new GameInfo();
                    break;
                case 7:
                    obj = new AuthorizeKnowledgeExchange();
                    break;
                case 8:
                    obj = new Discover();
                    break;
                case 9:
                    obj = new Move();
                    break;
                case 10:
                    obj = new PickUpPiece();
                    break;
                case 11:
                    obj = new PlacePiece();
                    break;
                case 12:
                    obj = new TestPiece();
                    break;
                case 13:
                    obj = new AcceptExchangeRequest();
                    break;
                case 14:
                    obj = new ConfirmJoiningGame(); 
                    break;
                case 15:
                    obj = new Data();
                    break;
                case 16:
                    obj = new Game();
                    break;
                case 17:
                    obj = new KnowledgeExchangeRequest();
                    break;
                case 18:
                    obj = new RejectKnowledgeExchange();
                    break;
                case 19:
                    obj = new PlayerMessage();
                    break;
                case 20:
                    obj = new Player();
                    break;
                case 21:
                    obj = new ConfirmGameRegistration();
                    break;
                case 22:
                    obj = new GetGames();
                    break;
                case 23:
                    obj = new JoinGame();
                    break;
                case 24:
                    obj = new RegisteredGames();
                    break;
                case 25:
                    obj = new RegisterGame();
                    break;
                default:
                    RegisterGame rg = new RegisterGame();
                    rg.NewGameInfo = new GameInfo();
                    rg.NewGameInfo.blueTeamPlayers = 42;
                    rg.NewGameInfo.redTeamPlayers = 24;
                    obj = rg;
                    break;
            }

            return obj;
        }


    }
}
