using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EcoCycleCore.Models;

public partial class EcoCycleContext : DbContext
{
    public EcoCycleContext()
    {
    }

    public EcoCycleContext(DbContextOptions<EcoCycleContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Canje> Canjes { get; set; }

    public virtual DbSet<Cupone> Cupones { get; set; }

    public virtual DbSet<Entrega> Entregas { get; set; }

    public virtual DbSet<Materiale> Materiales { get; set; }

    public virtual DbSet<Publicacione> Publicaciones { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EcoCycle;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Canje>(entity =>
        {
            entity.ToTable("CANJES");

            entity.HasIndex(e => e.CodigoDigital, "UQ__CANJES__ECD7D41A82363E0E").IsUnique();

            entity.Property(e => e.CanjeId).HasColumnName("canje_id");
            entity.Property(e => e.CodigoDigital)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("codigo_digital");
            entity.Property(e => e.CuponId).HasColumnName("cupon_id");
            entity.Property(e => e.FechaCanje)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_canje");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Cupon).WithMany(p => p.Canjes)
                .HasForeignKey(d => d.CuponId)
                .HasConstraintName("FK_CANJES_CUPONES");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Canjes)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_CANJES_USUARIOS");
        });

        modelBuilder.Entity<Cupone>(entity =>
        {
            entity.HasKey(e => e.CuponId);

            entity.ToTable("CUPONES");

            entity.Property(e => e.CuponId).HasColumnName("cupon_id");
            entity.Property(e => e.CostPuntos).HasColumnName("cost_puntos");
            entity.Property(e => e.DescBeneficio)
                .HasColumnType("text")
                .HasColumnName("desc_beneficio");
            entity.Property(e => e.Stock).HasColumnName("stock");
            entity.Property(e => e.TiendaNombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("tienda_nombre");
        });

        modelBuilder.Entity<Entrega>(entity =>
        {
            entity.ToTable("ENTREGAS");

            entity.Property(e => e.EntregaId).HasColumnName("entrega_id");
            entity.Property(e => e.CentroId).HasColumnName("centro_id");
            entity.Property(e => e.CiudadanoId).HasColumnName("ciudadano_id");
            entity.Property(e => e.FechaEntrega)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_entrega");
            entity.Property(e => e.PesoReal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("peso_real");
            entity.Property(e => e.PublicacionId).HasColumnName("publicacion_id");
            entity.Property(e => e.PuntosOtorgados).HasColumnName("puntos_otorgados");

            entity.HasOne(d => d.Ciudadano).WithMany(p => p.Entregas)
                .HasForeignKey(d => d.CiudadanoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ENTREGAS_USUARIOS");

            entity.HasOne(d => d.Publicacion).WithMany(p => p.Entregas)
                .HasForeignKey(d => d.PublicacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ENTREGAS_PUBLICACIONES");
        });

        modelBuilder.Entity<Materiale>(entity =>
        {
            entity.HasKey(e => e.MaterialId);

            entity.ToTable("MATERIALES");

            entity.Property(e => e.MaterialId).HasColumnName("material_id");
            entity.Property(e => e.NombreMaterial)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre_material");
            entity.Property(e => e.PuntosPorKilo)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("puntos_por_kilo");
        });

        modelBuilder.Entity<Publicacione>(entity =>
        {
            entity.HasKey(e => e.PublicacionesId);

            entity.ToTable("PUBLICACIONES");

            entity.Property(e => e.PublicacionesId).HasColumnName("publicaciones_id");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaPublicacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_publicacion");
            entity.Property(e => e.MaterialId).HasColumnName("material_id");
            entity.Property(e => e.PesoCantidad)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("peso_cantidad");
            entity.Property(e => e.Ubicacion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ubicacion");
            entity.Property(e => e.UrlImagen)
                .HasMaxLength(2083)
                .IsUnicode(false)
                .HasColumnName("url_imagen");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Material).WithMany(p => p.Publicaciones)
                .HasForeignKey(d => d.MaterialId)
                .HasConstraintName("FK_PUBLICACIONES_MATERIALES");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Publicaciones)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_PUBLICACIONES_USUARIOS");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("USUARIOS");

            entity.HasIndex(e => e.Correo, "UQ__USUARIOS__2A586E0B3A764D46").IsUnique();

            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.ContrasenaHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("contrasena_hash");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Direccion)
                .HasColumnType("text")
                .HasColumnName("direccion");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PuntosAcumulacion)
                .HasDefaultValue(0)
                .HasColumnName("puntos_acumulacion");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoUsuario)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
