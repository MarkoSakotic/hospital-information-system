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
        public static HISContext CreateInMemoryHISContext()
        {
            var options = new DbContextOptionsBuilder<HISContext>()
               .UseInMemoryDatabase(databaseName: "HISContextTest")
               .Options;

            return new HISContext(options);
            //return new HISContext(null);
        }


    }
}
