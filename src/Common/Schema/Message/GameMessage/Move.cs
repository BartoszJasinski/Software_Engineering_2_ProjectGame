namespace Common.Schema.Message
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/", IsNullable=false)]
    public partial class Move : GameMessage {
    
        private MoveType directionField;
    
        private bool directionFieldSpecified;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public MoveType direction {
            get {
                return this.directionField;
            }
            set {
                this.directionField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool directionSpecified {
            get {
                return this.directionFieldSpecified;
            }
            set {
                this.directionFieldSpecified = value;
            }
        }
    }
}