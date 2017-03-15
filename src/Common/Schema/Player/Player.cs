namespace Common.Schema.Player
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    public partial class Player {
    
        private TeamColour teamField;
    
        private PlayerType typeField;
    
        private ulong idField;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TeamColour team {
            get {
                return this.teamField;
            }
            set {
                this.teamField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PlayerType type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ulong id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
    }
}