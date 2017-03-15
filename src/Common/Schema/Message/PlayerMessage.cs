namespace Common.Schema.Message
{
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BetweenPlayersMessage))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    public partial class PlayerMessage {
    
        private ulong playerIdField;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ulong playerId {
            get {
                return this.playerIdField;
            }
            set {
                this.playerIdField = value;
            }
        }
    }
}