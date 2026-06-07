namespace NexusIMS.API.Entities
{
    public enum TransactionType
    {
        StockIn = 1, // إضافة/شراء بضاعة
        StockOut = 2, // صرف/مبيعات
        Damage = 3 // هالك/تالف
    }
}
