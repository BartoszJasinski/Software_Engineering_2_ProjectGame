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
    public partial class RegisterGame {
    
        private GameInfo newGameInfoField;
    
        /// <remarks/>
        public GameInfo NewGameInfo {
            get {
                return this.newGameInfoField;
            }
            set {
                this.newGameInfoField = value;
            }
        }
    }
}