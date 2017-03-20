namespace Common.Schema
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    public partial class GoalField : Field {
    
        private GoalFieldType typeField;
    
        private TeamColour teamField;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public GoalFieldType type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
    
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
    }
}