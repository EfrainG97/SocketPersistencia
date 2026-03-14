# Persistencia en sistemas cliente–servidor

> Repositorio: **[PON_AQUI_EL_LINK_DEL_REPOSITORIO](https://example.com)**

## 1?? ¿Qué diferencia existe entre almacenar datos en un archivo y en una base de datos?

- **Archivo**: guarda información como texto o binario sin reglas estrictas de estructura global. La lógica para buscar, validar y actualizar datos la implementa la aplicación.
- **Base de datos**: organiza los datos en estructuras definidas (tablas, relaciones, índices) y ofrece un motor especializado para consultar, validar, proteger y mantener consistencia.

## 2?? ¿Qué ventajas ofrece una base de datos frente a archivos planos?

- **Consultas más eficientes** (filtros, búsquedas, ordenamiento, índices).
- **Integridad de datos** (tipos, restricciones, claves primarias/foráneas).
- **Concurrencia controlada** (múltiples usuarios/procesos al mismo tiempo).
- **Seguridad** (usuarios, roles, permisos).
- **Transacciones** (operaciones atómicas y recuperación ante fallos).
- **Escalabilidad y mantenimiento** superiores en proyectos reales.

## 3?? ¿Por qué la persistencia es importante en sistemas cliente–servidor?

Porque permite que la información **permanezca disponible** después de terminar una sesión o cerrar la aplicación. En un modelo cliente–servidor, múltiples clientes dependen de que el servidor conserve un estado confiable para consultas futuras, auditoría, reportes y continuidad operativa.

## 4?? ¿Qué ocurriría si el servidor se reinicia y los datos solo estaban en memoria?

Se **perderían los datos no persistidos** (RAM es volátil). Al reiniciar, el servidor volvería sin historial reciente, lo que puede provocar:

- pérdida de registros enviados por clientes,
- inconsistencias en el sistema,
- mala experiencia de usuario,
- posibles impactos funcionales o de negocio.

## 5?? Menciona tres sistemas reales que utilicen comunicación cliente–servidor con persistencia.

1. **Banca en línea** (apps/web de bancos con base de datos de cuentas y transacciones).
2. **Plataformas de e-commerce** (catálogo, pedidos, pagos, inventario).
3. **Redes sociales** (usuarios, publicaciones, mensajes, interacciones).
