namespace Common.Schema.Message.PlayerMessage
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/", IsNullable=false)]
    public partial class RejectKnowledgeExchange : BetweenPlayersMessage {
    
        private bool permanentField;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool permanent {
            get {
                return this.permanentField;
            }
            set {
                this.permanentField = value;
            }
        }
    }
}