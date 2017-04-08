using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Player.Strategy
{
    /// <summary>
    /// Every state has its name and action, player should perform
    /// Transitions between states are chosen by predicate
    /// </summary>
    public class State
    {
        public string Name { get; set; }
        public readonly Action Action;
        private List<KeyValuePair<State, Func<bool>>> transitions;

        public State(string name, Action action)
        {
            Name = name;
            Action = action;
            transitions = new List<KeyValuePair<State, Func<bool>>>();
        }

        public State AddTransition(State st, Func<bool> pr)
        {
            transitions.Add(new KeyValuePair<State, Func<bool>>(st, pr));
            return this;
        }

        public State NextState()
        {
            foreach (var pair in transitions)
            {
                if (pair.Value())
                    return pair.Key;
            }
            return this;
        }
    }

    public class DfaBuilder
    {
        private Dictionary<string, State> States;
        private State starting;
        public DfaBuilder()
        {
            States = new Dictionary<string, State>();
        }

        public DfaBuilder AddState(string name, Action action=null)
        {
            if (States.ContainsKey(name))
                throw new Exception("State names must be unique");
            if (action == null)
                action = () => { };
            States[name] = new State(name, action);
            if (States.Count == 1)
                starting = States[name];
            return this;
        }
        //if no predicate specified, it always goes there
        public DfaBuilder AddTransition(string from, string to, Func<bool> when =null)
        {
            if (!States.ContainsKey(from) || !States.ContainsKey(to))
                throw new Exception();
            if (when == null)
            {
                when = () => true;
            }
            States[from].AddTransition(States[to], when);
            return this;
        }

        public State StartingState()
        {
            return starting;
        }
    }
    
}