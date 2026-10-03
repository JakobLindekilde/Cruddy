using QuickGrid.Toolkit;
using System.Reflection;

namespace Cruddy.Helpers
{
    static public class CruddyHelper
    {
        /// <summary>
        /// Make sure to get the correct overload of method AddSimple() in ColumnManager in QuickGrid.Toolkit.
        /// </summary>
        /// <returns>AddSimple from ColumnManager</returns>
        public static MethodInfo? GetAddSimpleMethod<TEntity>()
        {
            var m = typeof(ColumnManager<TEntity>).GetMethods()
                .FirstOrDefault(m =>
                    m.Name == "AddSimple" &&
                    m.GetParameters().Count() >= 8 &&
                    m.GetParameters()[0].ParameterType.Name.StartsWith("Expression") &&
                    m.GetParameters()[1].ParameterType.Name.StartsWith("ColumnInfo") &&
                    m.GetParameters()[2].ParameterType.Name.StartsWith("String") &&
                    m.GetParameters()[3].ParameterType.Name.StartsWith("Align") &&
                    m.GetParameters()[4].ParameterType.Name.StartsWith("CellStyleMap") &&
                    m.GetParameters()[5].ParameterType.Name.StartsWith("GridSort") &&
                    m.GetParameters()[6].ParameterType.Name.StartsWith("Bool") &&
                    m.GetParameters()[7].ParameterType.Name.StartsWith("String"));
            return m;
        }

    }
}
