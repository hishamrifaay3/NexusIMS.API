using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NexusIMS.API.Entities;
using System.Reflection.Emit;
namespace NexusIMS.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {}
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
        public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<SalesInvoice> SalesInvoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<SalesReturn> SalesReturns { get; set; }
        public DbSet<SalesReturnItem> SalesReturnItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
 

            builder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(c => c.Description).HasMaxLength(500);
                entity.HasIndex(c => c.Name).IsUnique();
            });



            builder.Entity<Product>(entity => 
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).HasMaxLength(50).IsRequired();
                entity.Property(p => p.SKU).IsRequired();
                entity.Property(p => p.Price).HasPrecision(18,2).IsRequired();
                entity.HasIndex(p => p.SKU).IsUnique();
                // علاقة القسم بالمنتجات
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict); // ممنوع تمسح قسم جواه بضاعة
                // علاقة الموظف بالمنتجات اللي أضافها
                entity.HasOne(p => p.CreatedByUser)
                      .WithMany(u => u.Products)
                      .HasForeignKey(p => p.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Product>().ToTable(t => t.HasCheckConstraint("CK_Product_Price_Min", "Price >= 0"));


            builder.Entity<Warehouse>(entity => 
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.Name).HasMaxLength(50).IsRequired();
                entity.Property(w => w.Location).HasMaxLength(200).IsRequired();
                entity.HasIndex(w => w.Name).IsUnique();
            });

            //  إعدادات جدول الرصيد الحالي (Stock - Many-to-Many Bridge)
            builder.Entity<Stock>(entity =>
            {
                entity.HasKey(s => new { s.ProductId , s.WarehouseId});

                entity.HasOne(s => s.Warehouse)
                      .WithMany() 
                      .HasForeignKey(s => s.WarehouseId)
                      .OnDelete(DeleteBehavior.Cascade);// لو المخزن اتمسح نهائياً، رصيده يتصفر ويتحذف

                entity.HasOne(s => s.Product)
                      .WithMany()
                      .HasForeignKey(s => s.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);// لو المنتج اتمسح نهائياً، كمياته في المخازن تتحذف
            });

            builder.Entity<Stock>().ToTable(t => t.HasCheckConstraint("CK_Stock_Quantity_Min", "Quantity >= 0"));

            //  إعدادات جدول الموظفين (ApplicationUser) وعلاقتهم بالمخازن
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();

                entity.HasOne(u => u.Warehouse)
                      .WithMany(w => w.Employees)
                      .HasForeignKey(u => u.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);// لو مسحنا مخزن ميمسحش الموظفين
            });

            builder.Entity<Supplier>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).HasMaxLength(150).IsRequired();
                entity.Property(s => s.Phone).HasMaxLength(20).IsRequired();
                entity.Property(s => s.Email).HasMaxLength(100);
                entity.Property(s => s.Address).HasMaxLength(250);
                entity.HasIndex(s => s.Name).IsUnique();
            });

            //  إعدادات سجل حركات المخزن الفردية (StockTransaction)
            builder.Entity<StockTransaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                // تخزين الـ Enum كأرقام (1, 2, 3) لتوفير المساحة وسرعة البحث

                entity.HasOne(t => t.Product)
                      .WithMany()
                      .HasForeignKey(t => t.ProductId)
                      .OnDelete(DeleteBehavior.Restrict); // حماية تاريخية: ممنوع مسح منتج له سجل حركات

                entity.HasOne(t => t.Warehouse)
                      .WithMany()
                      .HasForeignKey(t => t.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(t => t.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //  إعدادات رأس فاتورة المبيعات (SalesInvoice)
            builder.Entity<SalesInvoice>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.InvoiceNumber).HasMaxLength(50).IsRequired();
                entity.Property(i => i.CustomerName).HasMaxLength(150).IsRequired();

                // تحديد دقة الأرقام العشرية للمبالغ المالية (الحد الأقصى 18 رقم منهم 2 بعد العلامة)
                entity.Property(i => i.TotalAmount).HasPrecision(18, 2);
                entity.Property(i => i.Tax).HasPrecision(18, 2);
                entity.Property(i => i.Discount).HasPrecision(18, 2);
                entity.Property(i => i.FinalAmount).HasPrecision(18, 2);

                entity.HasIndex(i => i.InvoiceNumber).IsUnique();

                entity.HasOne(i => i.User)
                      .WithMany()
                      .HasForeignKey(i => i.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Warehouse)
                      .WithMany()
                      .HasForeignKey(i => i.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // إعدادات سطور/تفاصيل الفاتورة (InvoiceItem)
            builder.Entity<InvoiceItem>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.UnitPrice).HasPrecision(18, 2).IsRequired();
                entity.Property(item => item.TotalPrice).HasPrecision(18, 2).IsRequired();

                // ربط السطر بالفاتورة الكبيرة
                entity.HasOne(item => item.SalesInvoice)
                      .WithMany(invoice => invoice.Items) // هنا حددنا الكولكشن اللي جوه الفاتورة الكبيرة
                      .HasForeignKey(item => item.SalesInvoiceId)
                      .OnDelete(DeleteBehavior.Cascade); // لو الفاتورة الكبيرة اتمسحت، سطورها تتمسح تلقائياً

                entity.HasOne(item => item.Product)
                      .WithMany()
                      .HasForeignKey(item => item.ProductId)
                      .OnDelete(DeleteBehavior.Restrict); // ممنوع تمسح منتج دخل في فاتورة مبيعات قبل كده
            });

            builder.Entity<SalesReturn>(entity =>
            {
                entity.HasKey(sr => sr.Id);

                // ربط المرتجع بالفاتورة الأصلية
                entity.HasOne(sr => sr.SalesInvoice)
                      .WithMany() // الفاتورة الواحدة ممكن يتعمل عليها كذا مرتجع جزئي
                      .HasForeignKey(sr => sr.SalesInvoiceId)
                      .OnDelete(DeleteBehavior.Restrict); 

                // ربط المرتجع بالمخزن
                entity.HasOne(sr => sr.Warehouse)
                      .WithMany()
                      .HasForeignKey(sr => sr.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict); 

                // ربط المرتجع بالموظف اللي أنشأه
                entity.HasOne(sr => sr.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(sr => sr.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SalesReturnItem>(entity =>
            {
                entity.HasKey(sri => sri.Id);

                // ربط السطر بالمرتجع الرئيسي (Cascade: لو مسحنا مستند المرتجع، تمسح سطوره تلقائياً)
                entity.HasOne(sri => sri.SalesReturn)
                      .WithMany(sr => sr.Items)
                      .HasForeignKey(sri => sri.SalesReturnId)
                      .OnDelete(DeleteBehavior.Cascade);

                // ربط السطر بالمنتج
                entity.HasOne(sri => sri.Product)
                      .WithMany()
                      .HasForeignKey(sri => sri.ProductId)
                      .OnDelete(DeleteBehavior.Restrict); // ممنوع مسح المنتج من السيستم لو مربوط بمرتجع
            });


            builder.Entity<PurchaseInvoice>(entity =>
            {
                entity.HasKey(pi => pi.Id);

                entity.HasOne(pi => pi.Supplier)
                      .WithMany()
                      .HasForeignKey(pi => pi.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pi => pi.Warehouse)
                      .WithMany()
                      .HasForeignKey(pi => pi.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pi => pi.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(pi => pi.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PurchaseInvoiceItem>(entity =>
            {
                entity.HasKey(pii => pii.Id);

                entity.HasOne(pii => pii.PurchaseInvoice)
                      .WithMany(pi => pi.Items)
                      .HasForeignKey(pii => pii.PurchaseInvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pii => pii.Product)
                      .WithMany()
                      .HasForeignKey(pii => pii.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
