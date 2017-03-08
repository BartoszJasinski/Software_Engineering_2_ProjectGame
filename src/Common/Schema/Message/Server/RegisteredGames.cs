using Common.Schema.Game;

namespace Common.Schema.Message.Server
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/", IsNullable=false)]
    public partial class RegisteredGames {
    
        private GameInfo[] gameInfoField;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("GameInfo")]
        public GameInfo[] GameInfo {
            get {
                return this.gameInfoField;
            }
            set {
                this.gameInfoField = value;
            }
        }
    }
}