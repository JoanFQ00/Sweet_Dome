using UnityEngine;

public class PumpMovementValidator : ButtonValidator
{
    public enum DirectionType
    {
        Left,
        Right
    }

    [Header("Referencias de la Consola")]
    [Tooltip("Arrastra aquí el script Machine01 que maneja el switch de la cabina.")]
    [SerializeField] private Machine01 machineSource;

    [Header("Configuración del Botón")]
    [Tooltip("Determina si este validador protegerá al botón de ir a la Izquierda o a la Derecha.")]
    [SerializeField] private DirectionType buttonDirection = DirectionType.Left;

    public override bool CanPress()
    {
        // Seguridad por si falta alguna referencia en la escena
        if (machineSource == null) return false;

        // 1. Conseguimos dinámicamente cuál de las dos bombas es la que está operando ahora mismo
        LiquidPump activePump = machineSource.GetActivePump();
        if (activePump == null) return false;

        // 2. Extraemos el índice en el que está parado el surtidor activo
        int currentIndex = activePump.CurrentFunnelIndex;

        // 3. Evaluamos las colisiones lógicas según la dirección que tenga este botón
        if (buttonDirection == DirectionType.Left)
        {
            // Si va a la izquierda y el índice ya es 0, no puede ir más a la izquierda (Bloqueado)
            if (currentIndex <= 0)
            {
                return false;
            }
        }
        else if (buttonDirection == DirectionType.Right)
        {
            // Si va a la derecha, necesitamos saber el límite máximo de waypoints del surtidor activo.
            // Para evitar modificar más el script original de la bomba, calculamos el límite 
            // basándonos en el tamaño de sus arrays (el que sea menor o el de posiciones).
            // Asumimos que pumpPositions define el recorrido físico.

            // Nota: Si pumpPositions es privado, puedes usar un valor por defecto o añadir otra propiedad,
            // pero como en tu script de LiquidPump usas tanto funnelArray como pumpPositions para el límite,
            // si no tienes acceso a ellos, podemos asumir que el límite estándar es 1 (puesto que se desplaza de 0 a 1).
            // Para hacerlo robusto y automático, asumiremos que tus bombas tienen 2 posiciones (0 y 1).
            int maxLimit = 1;

            if (currentIndex >= maxLimit)
            {
                return false; // Llegó al tope derecho (Bloqueado)
            }
        }

        // Si no se cumple ningún límite de carrera, el botón se puede pulsar libremente
        return true;
    }
}