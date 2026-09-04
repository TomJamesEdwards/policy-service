using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolicyService.Domain.Policies;

namespace PolicyService.Infrastructure.Persistence.Configurations;

internal sealed class PolicyConfiguration
    : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");

        builder.HasKey(policy => policy.Reference);

        builder.Property(policy => policy.Reference)
            .HasMaxLength(50)
            .ValueGeneratedNever();

        builder.Property(policy => policy.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(policy => policy.StartDate)
            .IsRequired();

        builder.Property(policy => policy.EndDate)
            .IsRequired();

        builder.Property(policy => policy.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(policy => policy.AutoRenew)
            .IsRequired();

        builder.Property(policy => policy.HasClaims)
            .IsRequired();

        ConfigureProperty(builder);
        ConfigurePolicyholders(builder);
        ConfigurePayments(builder);
    }

    private static void ConfigureProperty(
        EntityTypeBuilder<Policy> builder)
    {
        builder.OwnsOne(
            policy => policy.Property,
            property =>
            {
                property.Property(value => value.AddressLine1)
                    .HasColumnName("AddressLine1")
                    .HasMaxLength(200)
                    .IsRequired();

                property.Property(value => value.AddressLine2)
                    .HasColumnName("AddressLine2")
                    .HasMaxLength(200);

                property.Property(value => value.AddressLine3)
                    .HasColumnName("AddressLine3")
                    .HasMaxLength(200);

                property.Property(value => value.Postcode)
                    .HasColumnName("Postcode")
                    .HasMaxLength(16)
                    .IsRequired();
            });

        builder.Navigation(policy => policy.Property)
            .IsRequired();
    }

    private static void ConfigurePolicyholders(
        EntityTypeBuilder<Policy> builder)
    {
        builder.OwnsMany(
            policy => policy.Policyholders,
            policyholder =>
            {
                policyholder.ToTable("Policyholders");

                policyholder.WithOwner()
                    .HasForeignKey("PolicyReference");

                policyholder.Property<string>("PolicyReference")
                    .HasMaxLength(50);

                policyholder.Property<Guid>("Id")
                    .ValueGeneratedOnAdd();

                policyholder.HasKey(
                    "PolicyReference",
                    "Id");

                policyholder.Property(value => value.FirstName)
                    .HasMaxLength(100)
                    .IsRequired();

                policyholder.Property(value => value.LastName)
                    .HasMaxLength(100)
                    .IsRequired();

                policyholder.Property(value => value.DateOfBirth)
                    .IsRequired();
            });

        builder.Navigation(policy => policy.Policyholders)
            .HasField("_policyholders")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigurePayments(
        EntityTypeBuilder<Policy> builder)
    {
        builder.OwnsMany(
            policy => policy.Payments,
            payment =>
            {
                payment.ToTable("Payments");

                payment.WithOwner()
                    .HasForeignKey("PolicyReference");

                payment.Property<string>("PolicyReference")
                    .HasMaxLength(50);

                payment.HasKey(
                    "PolicyReference",
                    nameof(Payment.Reference));

                payment.Property(value => value.Reference)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                payment.Property(value => value.Type)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                payment.Property(value => value.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();
            });

        builder.Navigation(policy => policy.Payments)
            .HasField("_payments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}