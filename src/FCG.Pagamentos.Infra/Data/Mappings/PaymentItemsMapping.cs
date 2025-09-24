using FCG.Pagamentos.Business.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Infra.Data.Mappings
{
    public class PaymentItemsMapping : IEntityTypeConfiguration<PaymentItem>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<PaymentItem> builder)
        {
            builder.ToTable("PaymentItens");
            builder.HasKey(pi => pi.PaymentItemId);
            builder.Property(pi => pi.PaymentItemId)
                .IsRequired()
                .ValueGeneratedNever();
            builder.Property(pi => pi.PaymentId)
                .IsRequired();
            builder.Property(pi => pi.JogoId)
                .IsRequired();
            builder.Property(pi => pi.Description)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(pi => pi.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            builder.Property(pi => pi.Quantity)
                .IsRequired();
            builder.Ignore(pi => pi.TotalPrice);

            builder.HasIndex(pi => pi.PaymentId).HasDatabaseName("IX_PaymentItems_PaymentId");
            builder.HasIndex("JogoId").HasDatabaseName("IX_PaymentItems_JogoId");



        }
    }
    
}
