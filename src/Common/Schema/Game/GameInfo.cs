namespace Common.Schema
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    public partial class GameInfo {
    
        private string nameField;
    
        private ulong redTeamPlayersField;
    
        private ulong blueTeamPlayersField;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ulong redTeamPlayers {
            get {
                return this.redTeamPlayersField;
            }
            set {
                this.redTeamPlayersField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ulong blueTeamPlayers {
            get {
                return this.blueTeamPlayersField;
            }
            set {
                this.blueTeamPlayersField = value;
            }
        }
    }
}