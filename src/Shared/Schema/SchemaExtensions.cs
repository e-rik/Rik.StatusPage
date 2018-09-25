namespace Rik.StatusPage.Schema
{
    public static class SchemaExtensions
    {
        public static TUnit SetStatus<TUnit>(this TUnit unit, UnitStatus status, string statusMessage = null) where TUnit : Unit
        {
            unit.Status = status;
            unit.StatusMessage = statusMessage;

            return unit;
        }

        public static TUnit SetStatus<TUnit>(this TUnit unit, UnitStatus status, string format, params object[] args) where TUnit : Unit
        {
            unit.Status = status;
            unit.StatusMessage = string.Format(format, args);

            return unit;
        }
    }
}