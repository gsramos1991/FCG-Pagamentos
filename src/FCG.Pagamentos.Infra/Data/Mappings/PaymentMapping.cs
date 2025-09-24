using FCG.Pagamentos.Business.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Infra.Data.Mappings
{
    public class PaymentMapping : IEntityTypeConfiguration<Payment>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");
            builder.HasKey(p => p.PaymentId);
            builder.Property(p => p.PaymentId)
                .IsRequired()
                .ValueGeneratedNever();
            builder.Property(p => p.UserId)
                .IsRequired();
            builder.Property(p => p.Currency)
                .IsRequired()
                .HasColumnType("VARCHAR(3)");
             builder.Property(p => p.StatusPayment)
                .IsRequired()
                .HasColumnType("VARCHAR(20)");
            builder.Property(p => p.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnType("DATETIME2")
                .HasDefaultValueSql("GETDATE()")
                .ValueGeneratedOnAdd();

            builder.HasIndex("UserId").HasDatabaseName("IX_PAYMENTS_USERID");
            builder.HasIndex("StatusPayment").HasDatabaseName("IX_PAYMENTS_STATUSPAYMENT");

            builder.HasMany(p => p.Items)
                .WithOne()
                .HasForeignKey(pi => pi.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
