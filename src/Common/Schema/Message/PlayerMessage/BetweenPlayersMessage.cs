namespace Common.Schema.Message.PlayerMessage
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    public abstract partial class BetweenPlayersMessage : PlayerMessage {
    
        private ulong senderPlayerIdField;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ulong senderPlayerId {
            get {
                return this.senderPlayerIdField;
            }
            set {
                this.senderPlayerIdField = value;
            }
        }
    }
}