namespace WWD.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AciklamaEd")]
    public partial class AciklamaEd
    {
        [Key]
        public int AciklamaId { get; set; }

        public int? KampanyaID { get; set; }

        public string Aciklamasi { get; set; }
    }
}
