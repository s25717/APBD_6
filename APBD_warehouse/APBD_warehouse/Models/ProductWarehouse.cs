namespace APBD_warehouse.Models
{
    public class ProductWarehouse
    {
        public int IdProductWarehouse { get; set; }
        public int IdWarehouse { get; set; }
        public int IdProduct { get; set; }
        public int OrderId { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
