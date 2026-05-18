using HotelLux.Finance.Business.DTOs;
using HotelLux.Finance.Business.Exceptions;
using HotelLux.Finance.Business.Services;
using HotelLux.Finance.DataManagement.Interfaces;
using HotelLux.Finance.DataManagement.Models;
using Moq;
using Xunit;

namespace HotelLux.Finance.Business.Tests;

public class FacturaServiceRegeneracionReservaTests
{
    private static FacturaLineaGeneracionDto LineaValida() => new()
    {
        Descripcion = "Habitación test",
        Cantidad = 1,
        PrecioUnitario = 10,
        Subtotal = 10,
        ValorIva = 0,
        Descuento = 0,
        Total = 10
    };

    [Fact]
    public async Task Reserva_sin_facturas_previas_no_anula()
    {
        var mock = new Mock<IFacturaDataService>();
        mock.Setup(x => x.ListarPorReservaGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FacturaDataModel>());
        mock.Setup(x => x.CrearAsync(It.IsAny<FacturaDataModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FacturaDataModel m, CancellationToken _) =>
            {
                m.FacturaGuid = Guid.NewGuid();
                m.NumeroFactura = "FAC-RES-2026-000001";
                return m;
            });

        var svc = new FacturaService(mock.Object);
        await svc.GenerarConLineasAsync("RESERVA", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            new List<FacturaLineaGeneracionDto> { LineaValida() }, "tester", default);

        mock.Verify(x => x.AnularAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        mock.Verify(x => x.CrearAsync(It.IsAny<FacturaDataModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reserva_con_factura_EMI_sin_abonos_anula_y_crea()
    {
        var fg = Guid.NewGuid();
        var prev = new FacturaDataModel
        {
            FacturaGuid = fg,
            TipoFactura = "RESERVA",
            Estado = "EMI",
            NumeroFactura = "FAC-RES-OLD",
            Total = 100,
            SaldoPendiente = 100
        };

        var mock = new Mock<IFacturaDataService>();
        mock.Setup(x => x.ListarPorReservaGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { prev });
        mock.Setup(x => x.AnularAsync(fg, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mock.Setup(x => x.CrearAsync(It.IsAny<FacturaDataModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FacturaDataModel m, CancellationToken _) =>
            {
                m.FacturaGuid = Guid.NewGuid();
                m.NumeroFactura = "FAC-RES-NEW";
                return m;
            });

        var svc = new FacturaService(mock.Object);
        await svc.GenerarConLineasAsync("RESERVA", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            new List<FacturaLineaGeneracionDto> { LineaValida() }, "tester", default);

        mock.Verify(x => x.AnularAsync(fg, It.IsAny<string>(), "tester", It.IsAny<CancellationToken>()), Times.Once);
        mock.Verify(x => x.CrearAsync(It.IsAny<FacturaDataModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reserva_con_factura_PAG_lanza_validacion()
    {
        var mock = new Mock<IFacturaDataService>();
        mock.Setup(x => x.ListarPorReservaGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new FacturaDataModel
                {
                    FacturaGuid = Guid.NewGuid(),
                    TipoFactura = "RESERVA",
                    Estado = "PAG",
                    NumeroFactura = "X",
                    Total = 100,
                    SaldoPendiente = 0
                }
            });

        var svc = new FacturaService(mock.Object);
        await Assert.ThrowsAsync<ValidationException>(() =>
            svc.GenerarConLineasAsync("RESERVA", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                new List<FacturaLineaGeneracionDto> { LineaValida() }, "tester", default));
    }

    [Fact]
    public async Task Reserva_con_EMI_y_saldo_parcial_lanza_validacion()
    {
        var mock = new Mock<IFacturaDataService>();
        mock.Setup(x => x.ListarPorReservaGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new FacturaDataModel
                {
                    FacturaGuid = Guid.NewGuid(),
                    TipoFactura = "RESERVA",
                    Estado = "EMI",
                    NumeroFactura = "X",
                    Total = 100,
                    SaldoPendiente = 40
                }
            });

        var svc = new FacturaService(mock.Object);
        await Assert.ThrowsAsync<ValidationException>(() =>
            svc.GenerarConLineasAsync("RESERVA", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                new List<FacturaLineaGeneracionDto> { LineaValida() }, "tester", default));
    }
}
