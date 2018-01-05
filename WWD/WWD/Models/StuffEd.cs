namespace WWD.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("StuffEd")]
    public partial class StuffEd
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StuffEdId { get; set; }

        public int Miktari { get; set; }

        [StringLength(50)]
        public string Birim { get; set; }

        [StringLength(100)]
        public string StuffName { get; set; }

        public int? KampanyaID { get; set; }

        public int? gizli { get; set; }
    }
}
