using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RepositoryProject.Context;
using System.IO;
using System.Text;

namespace TestsProject.Common
{
    public static class TestContextFactory
    {
        public static HisContext CreateInMemoryHisContext()
        {
            var options = new DbContextOptionsBuilder<HisContext>()
               .UseInMemoryDatabase(databaseName: "HisContextTest")
               .Options;

            return new HisContext(options);
            //return new HisContext(null);
        }


    }
}
