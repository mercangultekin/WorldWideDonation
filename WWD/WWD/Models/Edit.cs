namespace WWD.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Edit")]
    public partial class Edit
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Edit()
        {
            DegUruns = new HashSet<DegUrun>();
        }

        public int EditId { get; set; }

        public int? KampanyaID { get; set; }

        public int? StuffID { get; set; }

        public string EditAciklama { get; set; }

        [StringLength(350)]
        public string KampanyaFotoK { get; set; }

        [StringLength(350)]
        public string KampanyaFotoB { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DegUrun> DegUruns { get; set; }

        public virtual Kampanya Kampanya { get; set; }

        public virtual Kampanya Kampanya1 { get; set; }

        public virtual Stuff Stuff { get; set; }
    }
}
