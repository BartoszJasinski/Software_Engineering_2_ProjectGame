namespace Common.Schema.Message.PlayerMessage
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/", IsNullable=false)]
    public partial class ConfirmJoiningGame : PlayerMessage {
    
        private Player.Player playerDefinitionField;
    
        private ulong gameIdField;
    
        private string privateGuidField;
    
        /// <remarks/>
        public Player.Player PlayerDefinition {
            get {
                return this.playerDefinitionField;
            }
            set {
                this.playerDefinitionField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ulong gameId {
            get {
                return this.gameIdField;
            }
            set {
                this.gameIdField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string privateGuid {
            get {
                return this.privateGuidField;
            }
            set {
                this.privateGuidField = value;
            }
        }
    }
}