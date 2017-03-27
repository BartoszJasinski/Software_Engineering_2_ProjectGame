using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Server.Game
{
    public class GamesContainer : IGamesContainer
    {
        private Dictionary<string, IGame> games;

        public GamesContainer()
        {
            games = new Dictionary<string, IGame>();
        }

        public IEnumerator<IGame> GetEnumerator()
        {
            return games.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => games.Values.Count;

        private void Add(IGame game)
        {
            // TODO should we throw exception if game with such id/name exists?
            games[game.Name] = game;
        }

        public void RegisterGame(IGame game)
        {
            //TODO maybe we should create new
            if (GetGameByName(game.Name) != null)
                throw new NameExistsException();
            if (GetGameById(game.Id) != null)
                throw new IdExistsException();
            Add(game);
        }

        public IGame GetGameById(int id)
        {
            try
            {
                return games.Values.First(game => game.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public IGame GetGameByName(string name)
        {
            try
            {
                return games[name];
            }
            catch
            {
                return null;
            }
        }

        public void RemoveGame(IGame game)
        {
            if (!games.ContainsKey(game.Name))
                throw new GameNotFoundException();

            games.Remove(game.Name);
        }

        public int NextGameId()
        {

            return games.Count>0? games.Values.Max(game => game.Id) + 1 : 0;
        }
    }

    public class GameNotFoundException : Exception
    {
    }
}