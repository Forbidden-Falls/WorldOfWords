namespace Data
{
    using System.Data.Entity;
    using System.Data.Entity.SqlServer;

    public class WorldOfWordsDbConfiguration : DbConfiguration
    {
        public WorldOfWordsDbConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
        }
    }
}
