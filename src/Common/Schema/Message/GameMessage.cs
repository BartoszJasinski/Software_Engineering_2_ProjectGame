namespace Common.Schema.Message
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    public abstract partial class GameMessage {
    
        private string playerGuidField;
    
        private ulong gameIdField;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string playerGuid {
            get {
                return this.playerGuidField;
            }
            set {
                this.playerGuidField = value;
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
    }
}