using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DebugUtils;
using Common.Schema;
using Player.Net;

namespace Player.Strategy
{
    public class Controller : IController
    {
        private PlayerClient Player { get; set; }
        private State currentState;

        public Controller(PlayerClient player)
        {
            Possess(player);
        }

        public Controller() {}

        public IController Possess(PlayerClient player)
        {
            if (player == null)
            {
                throw new ArgumentException("Player cannot be null");
            }
            if(Player != null)
                Player.ReadyForAction -= Play;
            Player = player;
            Player.ReadyForAction += Play;
            currentState = BuildDfa();

            return this;
        }

        public void Play()
        {
            ConsoleDebug.Strategy(currentState.Name);
            Player.Game.PrintBoard();

            currentState = currentState.NextState();
            if (currentState.Action == null)
                Play();
            else
                currentState.Action();
        }

        private State BuildDfa()
        {
            return new DfaBuilder()
                //finding piece
                .AddState("start")
                .AddState("discover", Player.Discover)
                .AddTransition("start", "discover")
                .AddState("moving", Player.MoveToNeighborClosestToPiece)
                .AddTransition("discover", "moving", () => Player.DistToPiece() > 0 || Player.DistToPiece() == null)
                .AddTransition("moving", "discover", () => Player.DistToPiece() > 0 || Player.DistToPiece() == null)
                .AddState("onPiece", Player.PickUpPiece)
                //picking piece
                .AddTransition("discover", "onPiece", () => Player.DistToPiece() == 0)
                .AddTransition("moving", "onPiece", () => Player.DistToPiece() == 0)
                .AddState("notTested", Player.Test)
                .AddTransition("onPiece", "notTested")
                .AddState("carryingNormal", Player.LookForGoal)
                //testing piece
                .AddTransition("onPiece", "notTested", Player.HasPiece)
                .AddTransition("onPiece", "discover", () => !Player.HasPiece())
                .AddState("carryingSham", Player.DestroySham)
                .AddState("shamPicked", Player.Discover)
                .AddTransition("notTested", "carryingNormal",
                    () => Player.CarriedPiece != null && Player.CarriedPiece.type == PieceType.normal)
                .AddTransition("notTested", "shamPicked", () => Player.CarriedPiece != null && Player.CarriedPiece.type == PieceType.sham)
                .AddTransition("notTested", "discover", () => Player.CarriedPiece == null)
                //destroying sham
                .AddTransition("shamPicked", "carryingSham")
                .AddTransition("carryingSham", "discover", () => !Player.HasPiece())
                //place normal piece               
                .AddTransition("carryingNormal", "discover", () => !Player.HasPiece())

                .StartingState();
        }
    }
}
