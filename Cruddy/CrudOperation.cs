namespace Cruddy
{
    /// <summary>
    /// Represents the CRUD operations that can be performed on a data entity. 
    /// This enum is used to specify the type of operation being performed, 
    /// such as creating, reading, or updating an entity.
    /// </summary>
    public enum CrudOperation
    {
        /// <summary>
        /// Represents the creation of a new entity. This operation is used when adding a new record to the database or data source.
        /// </summary>
        Create = 1,
        /// <summary>
        /// Represents the reading or retrieval of an existing entity. This operation is used when fetching data from the database or data source.
        /// </summary>
        Read,
        /// <summary>
        /// Represents the updating of an existing entity. This operation is used when modifying an existing record in the database or data source.
        /// </summary>
        Update
    }
}
