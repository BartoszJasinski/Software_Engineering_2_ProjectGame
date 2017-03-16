namespace Common.Schema
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    public partial class GameBoard {
    
        private uint widthField;
    
        private uint tasksHeightField;
    
        private uint goalsHeightField;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint width {
            get {
                return this.widthField;
            }
            set {
                this.widthField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint tasksHeight {
            get {
                return this.tasksHeightField;
            }
            set {
                this.tasksHeightField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint goalsHeight {
            get {
                return this.goalsHeightField;
            }
            set {
                this.goalsHeightField = value;
            }
        }
    }
}