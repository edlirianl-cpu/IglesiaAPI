using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using IglesiaAPI.DTOs;

namespace IglesiaAPI.Infrastructure.Reports
{
    public class PdfGenerator
    {
        // =============================================
        // PALETA DE COLORES
        // =============================================
        private readonly string AzulOscuro = "#1A237E";
        private readonly string AzulMedio = "#283593";
        private readonly string VerdeExito = "#2E7D32";
        private readonly string RojoEgreso = "#C62828";
        private readonly string GrisFondo = "#F5F5F5";
        private readonly string GrisBorde = "#E0E0E0";
        private readonly string Blanco = "#FFFFFF";

        private readonly string RutaLogo = Path.Combine(
            Directory.GetCurrentDirectory(), "Assets", "Logo.png");

        // =============================================
        // 1. REPORTE CONSOLIDADO
        // =============================================
        public byte[] GenerarInforme(ReporteConsolidadoDTO data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x
                        .FontSize(10)
                        .FontColor("#212121")
                        .FontFamily("Arial"));

                    page.Header().Element(c => Encabezado(c, "REPORTE CONSOLIDADO NACIONAL", data.NombreSede, data.Periodo));

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        // --- CARDS DE RESUMEN ---
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Element(e => CardIndicador(e, "TOTAL MIEMBROS", data.TotalGeneral.ToString(), AzulOscuro));
                            row.Spacing(8);
                            row.RelativeItem().Element(e => CardIndicador(e, "BAUTIZADOS", data.TotalBautizados.ToString(), VerdeExito));
                            row.Spacing(8);
                            row.RelativeItem().Element(e => CardIndicador(e, "SELLADOS", data.TotalSellados.ToString(), AzulMedio));
                            row.Spacing(8);
                            row.RelativeItem().Element(e => CardIndicador(e, "NIÑOS", data.TotalNiños.ToString(), "#F57F17"));
                        });

                        col.Item().PaddingTop(20);

                        // --- TABLA 1: ESTADÍSTICAS GENERALES ---
                        col.Item().Element(c => TituloSeccion(c, "RESUMEN ESTADÍSTICO GENERAL"));
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(CeldaEncabezado).Text("DESCRIPCIÓN");
                                h.Cell().Element(CeldaEncabezado).AlignCenter().Text("TOTAL");
                            });

                            FilaTabla(table, "Iglesias / Sedes", data.TotalIglesias.ToString(), false);
                            FilaTabla(table, "Total Bautizados", data.TotalBautizados.ToString(), true);
                            FilaTabla(table, "Total Simpatizantes", data.TotalSimpatizantes.ToString(), false);
                            FilaTabla(table, "Total Niños", data.TotalNiños.ToString(), true);
                            FilaTabla(table, "Total Sellados", data.TotalSellados.ToString(), false);
                            FilaTablaTotal(table, "TOTAL GENERAL", data.TotalGeneral.ToString());
                        });

                        col.Item().PaddingTop(20);

                        // --- TABLA 2: RESUMEN PARCIAL DEL AÑO ---
                        col.Item().Element(c => TituloSeccion(c, $"RESUMEN PARCIAL {DateTime.Today.Year}"));
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(CeldaEncabezado).Text("DESCRIPCIÓN");
                                h.Cell().Element(CeldaEncabezado).AlignCenter().Text("TOTAL");
                            });

                            FilaTabla(table, $"Bautizados {DateTime.Today.Year}", data.BautizadosAnio.ToString(), false);
                            FilaTabla(table, $"Sellados {DateTime.Today.Year}", data.SealladosAnio.ToString(), true);
                            FilaTabla(table, $"Apartados {DateTime.Today.Year}", data.ApartadosAnio.ToString(), false);
                            FilaTabla(table, "Salieron a otras congregaciones", data.TrasladosAnio.ToString(), true);
                            FilaTabla(table, "Fallecidos", data.FallecidosAnio.ToString(), false);
                            FilaTabla(table, "Llegaron de otras congregaciones", data.LlegaronAnio.ToString(), true);
                        });

                        col.Item().PaddingTop(20);

                        // --- TABLA 3: RESUMEN POR EDAD ---
                        col.Item().Element(c => TituloSeccion(c, "RESUMEN POR EDAD"));
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(CeldaEncabezado).Text("RANGO");
                                h.Cell().Element(CeldaEncabezado).AlignCenter().Text("TOTAL");
                            });

                            FilaTabla(table, "Hombres menores de 35", data.HombresMenores35.ToString(), false);
                            FilaTabla(table, "Hombres entre 35 y 63", data.Hombres35a63.ToString(), true);
                            FilaTabla(table, "Hombres mayores de 64", data.HombresMayores64.ToString(), false);
                            FilaTabla(table, "Damas menores de 35", data.DamasMenores35.ToString(), true);
                            FilaTabla(table, "Damas entre 35 y 63", data.Damas35a63.ToString(), false);
                            FilaTabla(table, "Damas mayores de 64", data.DamasMayores64.ToString(), true);
                            FilaTabla(table, "Niñas hasta 12", data.NiñasHasta12.ToString(), false);
                            FilaTabla(table, "Niños hasta 12", data.NiñosHasta12.ToString(), true);
                        });

                        // --- TABLA 4: MOVIMIENTOS FINANCIEROS ---
                        if (data.Movimientos.Any())
                        {
                            col.Item().PaddingTop(20);
                            col.Item().Element(c => TituloSeccion(c, "MOVIMIENTOS FINANCIEROS"));
                            col.Item().PaddingTop(8).Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(65);
                                    c.RelativeColumn();
                                    c.ConstantColumn(80);
                                    c.ConstantColumn(80);
                                    c.ConstantColumn(70);
                                });

                                table.Header(h =>
                                {
                                    h.Cell().Element(CeldaEncabezado).Text("Fecha");
                                    h.Cell().Element(CeldaEncabezado).Text("Concepto");
                                    h.Cell().Element(CeldaEncabezado).AlignRight().Text("Ingreso");
                                    h.Cell().Element(CeldaEncabezado).AlignRight().Text("Egreso");
                                    h.Cell().Element(CeldaEncabezado).AlignCenter().Text("Estado");
                                });

                                bool alt = false;
                                foreach (var mov in data.Movimientos)
                                {
                                    var bg = alt ? GrisFondo : Blanco;
                                    table.Cell().Element(c => FilaFondo(c, bg)).Text(mov.Fecha.ToString("dd/MM/yy")).FontSize(9);
                                    table.Cell().Element(c => FilaFondo(c, bg)).Column(c2 =>
                                    {
                                        c2.Item().Text(mov.Descripcion).FontSize(9);
                                        if (!string.IsNullOrEmpty(mov.LoteReferencia) && mov.LoteReferencia != "N/A")
                                            c2.Item().Text($"Lote: {mov.LoteReferencia}").FontSize(7).FontColor(Colors.Grey.Medium);
                                    });
                                    table.Cell().Element(c => FilaFondo(c, bg)).AlignRight()
                                        .Text(mov.Ingreso > 0 ? mov.Ingreso.ToString("N2") : "-")
                                        .FontSize(9).FontColor(VerdeExito);
                                    table.Cell().Element(c => FilaFondo(c, bg)).AlignRight()
                                        .Text(mov.Egreso > 0 ? mov.Egreso.ToString("N2") : "-")
                                        .FontSize(9).FontColor(RojoEgreso);
                                    table.Cell().Element(c => FilaFondo(c, bg)).AlignCenter()
                                        .Text(mov.Estado).FontSize(8);
                                    alt = !alt;
                                }

                                // Totales
                                table.Cell().ColumnSpan(2).Element(c => CeldaTotales(c)).Text("TOTALES").SemiBold();
                                table.Cell().Element(c => CeldaTotales(c)).AlignRight()
                                    .Text(data.TotalIngresos.ToString("N2")).FontColor(VerdeExito).SemiBold();
                                table.Cell().Element(c => CeldaTotales(c)).AlignRight()
                                    .Text(data.TotalEgresos.ToString("N2")).FontColor(RojoEgreso).SemiBold();
                                table.Cell().Element(c => CeldaTotales(c)).Text("");
                            });
                        }
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // =============================================
        // 2. HOJA DE VIDA
        // =============================================
        public byte[] GenerarHojaVida(HojaVidaDTO data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor("#212121").FontFamily("Arial"));

                    page.Header().Element(c => Encabezado(c, "HOJA DE VIDA", data.NombreSede, $"No. Registro: {data.NoRegistro}"));

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        // --- FOTO Y DATOS BÁSICOS ---
                        col.Item().Row(row =>
                        {
                            // Foto
                            var rutaFoto = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "fotos", data.FotoPath);
                            if (!string.IsNullOrEmpty(data.FotoPath) && File.Exists(rutaFoto))
                                row.ConstantItem(100).Image(rutaFoto).FitArea();
                            else
                                row.ConstantItem(100).Border(1).BorderColor(GrisBorde)
                                    .AlignCenter().AlignMiddle()
                                    .Text("SIN FOTO").FontSize(8).FontColor(Colors.Grey.Medium);

                            row.Spacing(15);

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(data.NombreCompleto).FontSize(16).Bold().FontColor(AzulOscuro);
                                c.Item().PaddingTop(4).Text($"Categoría: {data.Categoria}  |  Estado: {data.Estado}").FontSize(9);
                                if (data.EsSellado)
                                    c.Item().PaddingTop(2).Text("✓ MIEMBRO SELLADO").FontSize(9).FontColor(VerdeExito).SemiBold();
                                c.Item().PaddingTop(4).Text($"Sede: {data.NombreSede}").FontSize(9);
                            });
                        });

                        col.Item().PaddingTop(15);

                        // --- DATOS PERSONALES ---
                        col.Item().Element(c => TituloSeccion(c, "DATOS PERSONALES"));
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            FilaDatos(table, "Fecha de Nacimiento", data.FechaNacimiento?.ToString("dd/MM/yyyy") ?? "N/D");
                            FilaDatos(table, "Sexo", data.Sexo);
                            FilaDatos(table, "Estado Civil", data.EstadoCivil);
                            FilaDatos(table, "Nacionalidad", data.Nacionalidad);
                            FilaDatos(table, "Teléfono", data.Telefono);
                            FilaDatos(table, "Correo", data.Correo);
                            FilaDatos(table, "Provincia", data.Provincia);
                            FilaDatos(table, "Ciudad", data.Ciudad);
                            FilaDatos(table, "Dirección", data.Direccion);
                        });

                        col.Item().PaddingTop(15);

                        // --- DATOS ECLESIALES ---
                        col.Item().Element(c => TituloSeccion(c, "DATOS ECLESIALES"));
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            FilaDatos(table, "Fecha de Bautismo", data.FechaBautizado?.ToString("dd/MM/yyyy") ?? "N/D");
                            FilaDatos(table, "Categoría", data.Categoria);
                            FilaDatos(table, "Estado", data.Estado);
                            FilaDatos(table, "Es Sellado", data.EsSellado ? "Sí" : "No");
                        });

                        col.Item().PaddingTop(15);

                        // --- PERFIL PROFESIONAL ---
                        col.Item().Element(c => TituloSeccion(c, "PERFIL PROFESIONAL"));
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            FilaDatos(table, "Nivel Académico", data.NivelAcademico);
                            FilaDatos(table, "Profesión", data.Profesion);
                        });

                        col.Item().PaddingTop(15);

                        // --- NÚCLEO FAMILIAR ---
                        col.Item().Element(c => TituloSeccion(c, "NÚCLEO FAMILIAR"));
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            FilaDatos(table, "Cónyuge", data.Conyugue);
                            FilaDatos(table, "Madre", data.Madre);
                            FilaDatos(table, "Padre", data.Padre);
                            FilaDatos(table, "Hijos", data.Hijos);
                        });
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // =============================================
        // 3. CUMPLEAÑEROS DEL MES
        // =============================================
        public byte[] GenerarCumpleaneros(List<CumpleaneroDTO> data, int mes)
        {
            var nombreMes = new System.Globalization.DateTimeFormatInfo().GetMonthName(mes).ToUpper();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor("#212121").FontFamily("Arial"));

                    page.Header().Element(c => Encabezado(c, "CUMPLEAÑEROS DEL MES", nombreMes, $"Total: {data.Count} personas"));

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(30);
                                c.RelativeColumn(3);
                                c.ConstantColumn(40);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(CeldaEncabezado).AlignCenter().Text("Día");
                                h.Cell().Element(CeldaEncabezado).Text("Nombre Completo");
                                h.Cell().Element(CeldaEncabezado).AlignCenter().Text("Edad");
                                h.Cell().Element(CeldaEncabezado).Text("Teléfono");
                                h.Cell().Element(CeldaEncabezado).Text("Sede");
                            });

                            bool alt = false;
                            foreach (var m in data)
                            {
                                var bg = alt ? GrisFondo : Blanco;
                                table.Cell().Element(c => FilaFondo(c, bg)).AlignCenter().Text(m.FechaNacimiento.Day.ToString()).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(m.NombreCompleto).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).AlignCenter().Text(m.Edad.ToString()).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(m.Telefono).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(m.NombreSede).FontSize(9);
                                alt = !alt;
                            }
                        });
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // =============================================
        // 4. LISTA DE BAUTIZADOS
        // =============================================
        public byte[] GenerarListaBautizados(List<BautizadoDTO> data, string periodo)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor("#212121").FontFamily("Arial"));

                    page.Header().Element(c => Encabezado(c, "LISTA DE BAUTIZADOS", periodo, $"Total: {data.Count} personas"));

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(50);
                                c.RelativeColumn(3);
                                c.ConstantColumn(80);
                                c.ConstantColumn(50);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(CeldaEncabezado).Text("Registro");
                                h.Cell().Element(CeldaEncabezado).Text("Nombre Completo");
                                h.Cell().Element(CeldaEncabezado).Text("Fecha Bautismo");
                                h.Cell().Element(CeldaEncabezado).AlignCenter().Text("Sexo");
                                h.Cell().Element(CeldaEncabezado).Text("Sede");
                            });

                            bool alt = false;
                            foreach (var m in data)
                            {
                                var bg = alt ? GrisFondo : Blanco;
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(m.NoRegistro).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(m.NombreCompleto).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(m.FechaBautizado.ToString("dd/MM/yyyy")).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).AlignCenter().Text(m.Sexo).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(m.NombreSede).FontSize(9);
                                alt = !alt;
                            }
                        });
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // =============================================
        // 5. INVENTARIO DE ACTIVOS
        // =============================================
        public byte[] GenerarInventario(List<InventarioDetalleDTO> data, string sede)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontColor("#212121").FontFamily("Arial"));

                    page.Header().Element(c => Encabezado(c, "INVENTARIO DE ACTIVOS", sede, $"Total artículos: {data.Count}"));

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.ConstantColumn(40);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.ConstantColumn(70);
                                c.ConstantColumn(70);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(CeldaEncabezado).Text("Artículo");
                                h.Cell().Element(CeldaEncabezado).Text("Categoría");
                                h.Cell().Element(CeldaEncabezado).AlignCenter().Text("Cant.");
                                h.Cell().Element(CeldaEncabezado).Text("Estado");
                                h.Cell().Element(CeldaEncabezado).Text("Ubicación");
                                h.Cell().Element(CeldaEncabezado).Text("Responsable");
                                h.Cell().Element(CeldaEncabezado).AlignRight().Text("V. Unit.");
                                h.Cell().Element(CeldaEncabezado).AlignRight().Text("V. Total");
                            });

                            bool alt = false;
                            foreach (var i in data)
                            {
                                var bg = alt ? GrisFondo : Blanco;
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(i.NombreArticulo);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(i.Categoria);
                                table.Cell().Element(c => FilaFondo(c, bg)).AlignCenter().Text(i.Cantidad.ToString());
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(i.Estado);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(i.Ubicacion);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(i.Responsable);
                                table.Cell().Element(c => FilaFondo(c, bg)).AlignRight().Text(i.ValorUnitario.ToString("N2"));
                                table.Cell().Element(c => FilaFondo(c, bg)).AlignRight().Text(i.ValorTotal.ToString("N2")).FontColor(AzulOscuro).SemiBold();
                                alt = !alt;
                            }

                            // Total
                            table.Cell().ColumnSpan(7).Element(c => CeldaTotales(c)).Text("VALOR TOTAL DEL INVENTARIO").SemiBold();
                            table.Cell().Element(c => CeldaTotales(c)).AlignRight()
                                .Text(data.Sum(x => x.ValorTotal).ToString("N2")).FontColor(AzulOscuro).SemiBold();
                        });
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // =============================================
        // 6. REPORTE DE CÉLULAS
        // =============================================
        public byte[] GenerarReporteCelulas(List<CelulaDetalleDTO> data, string sede)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor("#212121").FontFamily("Arial"));

                    page.Header().Element(c => Encabezado(c, "REPORTE DE CÉLULAS", sede, $"Total células: {data.Count}"));

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(CeldaEncabezado).Text("Nombre Célula");
                                h.Cell().Element(CeldaEncabezado).Text("Líder");
                                h.Cell().Element(CeldaEncabezado).Text("Día");
                                h.Cell().Element(CeldaEncabezado).Text("Hora");
                                h.Cell().Element(CeldaEncabezado).Text("Sede");
                            });

                            bool alt = false;
                            foreach (var c2 in data)
                            {
                                var bg = alt ? GrisFondo : Blanco;
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(c2.NombreCelula).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(c2.Lider).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(c2.DiaReunion).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(c2.HoraReunion).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(c2.NombreSede).FontSize(9);
                                alt = !alt;
                            }
                        });
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // =============================================
        // 7. INFORME DE SECRETARÍA
        // =============================================
        public byte[] GenerarInformeSecretaria(List<RegistroSecretariaDetalleDTO> data, string periodo)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor("#212121").FontFamily("Arial"));

                    page.Header().Element(c => Encabezado(c, "INFORME DE SECRETARÍA", periodo, $"Total registros: {data.Count}"));

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(75);
                                c.ConstantColumn(90);
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(CeldaEncabezado).Text("Fecha");
                                h.Cell().Element(CeldaEncabezado).Text("Tipo");
                                h.Cell().Element(CeldaEncabezado).Text("Título");
                                h.Cell().Element(CeldaEncabezado).Text("Descripción");
                                h.Cell().Element(CeldaEncabezado).Text("Sede");
                            });

                            bool alt = false;
                            foreach (var r in data)
                            {
                                var bg = alt ? GrisFondo : Blanco;
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(r.Fecha.ToString("dd/MM/yyyy")).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(r.TipoRegistro).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(r.Titulo).FontSize(9);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(r.Descripcion).FontSize(8).FontColor(Colors.Grey.Darken1);
                                table.Cell().Element(c => FilaFondo(c, bg)).Text(r.NombreSede).FontSize(9);
                                alt = !alt;
                            }
                        });
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // =============================================
        // COMPONENTES REUTILIZABLES
        // =============================================
        private void Encabezado(IContainer container, string titulo, string subtitulo, string detalle)
        {
            container.BorderBottom(2).BorderColor(AzulOscuro).PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("IGLESIA PENTECOSTAL UNIDA LATINOAMERICANA")
                        .FontSize(13).Bold().FontColor(AzulOscuro);
                    col.Item().PaddingTop(2).Text(titulo)
                        .FontSize(11).SemiBold().FontColor(AzulMedio);
                    col.Item().PaddingTop(2).Row(r =>
                    {
                        r.RelativeItem().Text(subtitulo).FontSize(9).FontColor(Colors.Grey.Darken2);
                        r.AutoItem().Text(detalle).FontSize(9).FontColor(Colors.Grey.Darken2);
                    });
                    col.Item().PaddingTop(2).Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(8).Italic().FontColor(Colors.Grey.Medium);
                });

                row.Spacing(10);

                if (File.Exists(RutaLogo))
                    row.ConstantItem(70).AlignRight().Image(RutaLogo).FitArea();
            });
        }

        private void TituloSeccion(IContainer container, string titulo)
        {
            container.Background(AzulOscuro).Padding(6)
                .Text(titulo).FontSize(10).Bold().FontColor(Blanco);
        }

        private void CardIndicador(IContainer container, string titulo, string valor, string color)
        {
            container.Border(1).BorderColor(GrisBorde).Background(GrisFondo).Padding(10).Column(c =>
            {
                c.Item().Text(titulo).FontSize(8).SemiBold().FontColor(Colors.Grey.Darken2);
                c.Item().PaddingTop(4).Text(valor).FontSize(18).Bold().FontColor(color);
            });
        }

        private IContainer CeldaEncabezado(IContainer container) =>
            container.Background(AzulOscuro).Padding(6)
                .DefaultTextStyle(x => x.FontColor(Blanco).SemiBold().FontSize(9));

        private IContainer CeldaTotales(IContainer container) =>
            container.Background(GrisFondo).BorderTop(1).BorderColor(AzulOscuro)
                .Padding(6).DefaultTextStyle(x => x.SemiBold().FontSize(9));

        private IContainer FilaFondo(IContainer container, string color) =>
            container.Background(color).BorderBottom(1).BorderColor(GrisBorde).Padding(5);

        private void FilaTabla(TableDescriptor table, string etiqueta, string valor, bool alterno)
        {
            var bg = alterno ? GrisFondo : Blanco;
            table.Cell().Element(c => FilaFondo(c, bg)).Text(etiqueta).FontSize(9);
            table.Cell().Element(c => FilaFondo(c, bg)).AlignCenter().Text(valor).FontSize(9).SemiBold();
        }

        private void FilaTablaTotal(TableDescriptor table, string etiqueta, string valor)
        {
            table.Cell().Element(c => CeldaTotales(c)).Text(etiqueta).SemiBold();
            table.Cell().Element(c => CeldaTotales(c)).AlignCenter().Text(valor).SemiBold().FontColor(AzulOscuro);
        }

        private void FilaDatos(TableDescriptor table, string etiqueta, string valor)
        {
            table.Cell().BorderBottom(1).BorderColor(GrisBorde).PaddingVertical(4).PaddingHorizontal(2)
                .Text(etiqueta).FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);
            table.Cell().BorderBottom(1).BorderColor(GrisBorde).PaddingVertical(4).PaddingHorizontal(2)
                .Text(valor).FontSize(9);
        }

        private void PiePagina(IContainer container)
        {
            container.BorderTop(1).BorderColor(GrisBorde).PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text("Iglesia Pentecostal Unida Latinoamericana — Documento generado por el Sistema de Gestión Eclesial")
                    .FontSize(7).FontColor(Colors.Grey.Medium);

                row.AutoItem().Text(x =>
                {
                    x.Span("Página ").FontSize(7).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Medium);
                    x.Span(" de ").FontSize(7).FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontSize(7).FontColor(Colors.Grey.Medium);
                });
            });
        }
    }
}