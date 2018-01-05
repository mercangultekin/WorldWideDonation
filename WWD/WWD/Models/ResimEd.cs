namespace WWD.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ResimEd")]
    public partial class ResimEd
    {
        [Key]
        [Column("ResimEd")]
        public int ResimEd1 { get; set; }

        public int? KampanyaID { get; set; }

        [StringLength(350)]
        public string ResimYolB { get; set; }

        [StringLength(350)]
        public string ResimYolK { get; set; }
    }
}
