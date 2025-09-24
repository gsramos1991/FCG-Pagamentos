using FCG.Pagamentos.Business.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Infra.Data.Mappings
{
    public class PaymentEventMapping : IEntityTypeConfiguration<PaymentEvent>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<PaymentEvent> builder)
        {
            builder.ToTable("PaymentEvents");
            builder.HasKey(pe => pe.Id);
            builder.Property(pe => pe.Id)
                .IsRequired();
            builder.Property(pe => pe.PaymentId)
                .IsRequired();
            builder.Property(pe => pe.EventType)
                .IsRequired()
                .HasColumnType("VARCHAR")
                .HasMaxLength(100);
            builder.Property(pe => pe.PayLoad)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");
            builder.Property(pe => pe.Version)
                .IsRequired()
                .HasColumnType("INT");
            builder.Property(pe => pe.EventDate)
                .IsRequired()
                .HasColumnType("DATETIME2")
                .HasDefaultValueSql("GETDATE()")
                .ValueGeneratedOnAdd();

            builder.HasIndex(pe => new { pe.PaymentId, pe.Version })
            .IsUnique()
            .HasDatabaseName("UX_PaymentEvents_PaymentId_Version");

            builder.HasOne<Payment>()
                     .WithMany()
                     .HasForeignKey(pe => pe.PaymentId)
                     .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
