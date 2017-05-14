using Common.SchemaWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMaster.Logic
{
    public interface IRanking
    {
        void AddTeam(Team team);
        void AddWinForTeam(Team team);
        void Print();
    }
}
