namespace FutbolManager.Infrastructure;

/// <summary>
/// Marcador de ensamblado para la capa <b>Infrastructure</b>.
/// </summary>
/// <remarks>
/// <para>
/// Esta clase no contiene lógica. Sirve únicamente para localizar el ensamblado
/// vía <c>typeof(AssemblyMarker).Assembly</c>.
/// </para>
/// <para>
/// <b>Responsabilidad de la capa Infrastructure:</b>
/// implementa los servicios técnicos definidos como puertos en Application
/// (autenticación JWT, envío de email, almacenamiento de archivos, caché,
/// logging estructurado, integraciones HTTP a terceros, proveedor de fecha/hora).
/// <b>No contiene acceso a la base de datos</b>: eso vive en Persistence.
/// </para>
/// </remarks>
public sealed class AssemblyMarker
{
}
