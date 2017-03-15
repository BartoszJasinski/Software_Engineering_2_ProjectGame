namespace Common.Schema.Board
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://theprojectgame.mini.pw.edu.pl/")]
    public partial class Piece {
    
        private ulong idField;
    
        private PieceType typeField;
    
        private System.DateTime timestampField;
    
        private ulong playerIdField;
    
        private bool playerIdFieldSpecified;
    
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
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PieceType type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
    
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime timestamp {
            get {
                return this.timestampField;
            }
            set {
                this.timestampField = value;
            }
        }
    
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
    
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool playerIdSpecified {
            get {
                return this.playerIdFieldSpecified;
            }
            set {
                this.playerIdFieldSpecified = value;
            }
        }
    }
}