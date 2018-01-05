namespace WWD.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DegUrun")]
    public partial class DegUrun
    {
        public int DegUrunId { get; set; }

        public int UrunID { get; set; }

        public int? EditID { get; set; }

        public virtual Edit Edit { get; set; }
    }
}
