using Microsoft.AspNetCore.Components;

namespace CruddyDemo.Components
{
    public partial class Cruddy4 : CruddyBase4
    {
        /// <summary>
        /// The rows retrieved from the database table in <seealso cref="TableName"/>.
        /// </summary>
        protected IQueryable<Models.Customer>? Rows;

        protected override async Task FillRowsAsync()
        {
            Rows = await GetTableRowsAsync<Models.Customer>();
        }
    }
}
