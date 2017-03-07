namespace Common.Schema.Board.Fields
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    public partial class TaskField : Field {
    
        private uint distanceToPieceField;
    
        private ulong pieceIdField;
    
        private bool pieceIdFieldSpecified;
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint distanceToPiece {
            get {
                return this.distanceToPieceField;
            }
            set {
                this.distanceToPieceField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ulong pieceId {
            get {
                return this.pieceIdField;
            }
            set {
                this.pieceIdField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pieceIdSpecified {
            get {
                return this.pieceIdFieldSpecified;
            }
            set {
                this.pieceIdFieldSpecified = value;
            }
        }
    }
}