using Common.Schema.Player;

namespace Common.Schema.Message.Server
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/", IsNullable=false)]
    public partial class JoinGame {
    
        private string gameNameField;
    
        private TeamColour preferedTeamField;
    
        private PlayerType preferedRoleField;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string gameName {
            get {
                return this.gameNameField;
            }
            set {
                this.gameNameField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TeamColour preferedTeam {
            get {
                return this.preferedTeamField;
            }
            set {
                this.preferedTeamField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PlayerType preferedRole {
            get {
                return this.preferedRoleField;
            }
            set {
                this.preferedRoleField = value;
            }
        }
    }
}