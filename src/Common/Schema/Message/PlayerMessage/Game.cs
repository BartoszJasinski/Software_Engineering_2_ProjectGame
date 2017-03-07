using Common.Schema.Board;

namespace Common.Schema.Message.PlayerMessage
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/", IsNullable=false)]
    public partial class Game : PlayerMessage {
    
        private Player.Player[] playersField;
    
        private GameBoard boardField;
    
        private Location playerLocationField;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable=false)]
        public Player.Player[] Players {
            get {
                return this.playersField;
            }
            set {
                this.playersField = value;
            }
        }
    
        /// <remarks/>
        public GameBoard Board {
            get {
                return this.boardField;
            }
            set {
                this.boardField = value;
            }
        }
    
        /// <remarks/>
        public Location PlayerLocation {
            get {
                return this.playerLocationField;
            }
            set {
                this.playerLocationField = value;
            }
        }
    }
}