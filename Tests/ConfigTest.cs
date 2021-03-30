using System;
using mParticle.LoadGenerator;
using Xunit;

namespace Tests
{
    public sealed class ConfigTest
    {
        [Fact]
        public void Config_Null()
        {
            // Test a null path.
            Config arguments = Config.GetArguments(null);
            Assert.Null(arguments);

            // Test a path that does not name a file.
            arguments = Config.GetArguments("Gazonk+7");
            Assert.Null(arguments);
        }

        [Fact]
        public void Config_Valid()
        {
            // Test well-formed arguments.
            Config config = Config.ParseArguments("{ \"serverURL\" : \"https://www.mparticle.com\" , \"targetRPS\" : 10, \"authKey\" : \"Whatever\", \"userName\" : \"Fred\"}");
            Assert.NotNull(config);
            Assert.Equal("https://www.mparticle.com", config.ServerURL);
            Assert.Equal(10u, config.TargetRPS);
            Assert.Equal("Whatever", config.AuthKey);
            Assert.Equal("Fred", config.UserName);
        }

        [Fact]
        public void Config_MissingRequired()
        {
            // Test missing arguments.
            Config config = Config.ParseArguments("{ \"targetRPS\" : 10, \"authKey\" : \"Whatever\", \"userName\" : \"Fred\" }");
            Assert.Null(config);

            config = Config.ParseArguments("{ \"serverURL\" : \"https://www.mparticle.com\" , \"authKey\" : \"Whatever\", \"userName\" : \"Fred\"  }");
            Assert.Null(config);

            config = Config.ParseArguments("{ \"serverURL\" : \"https://www.mparticle.com\" , \"targetRPS\" : 10, \"userName\" : \"Fred\" }");
            Assert.Null(config);

            config = Config.ParseArguments("{ \"serverURL\" : \"https://www.mparticle.com\" , \"targetRPS\" : 10, \"authKey\" : \"Whatever\" }");
            Assert.Null(config);
        }

        [Fact]
        public void Config_InvalidValues()
        {
            // Test arguments with invalid values.
            Config config = Config.ParseArguments("{ \"serverURL\" : \"https://www.mparticle.com\" , \"targetRPS\" : 0, \"authKey\" : \"Whatever\", \"userName\" : \"Fred\"}");
            Assert.Null(config);

            config = Config.ParseArguments("{ \"serverURL\" : \"https://www.mparticle.com\" , \"targetRPS\" : 10, \"authKey\" : \"\", \"userName\" : \"Fred\" }");
            Assert.Null(config);

            config = Config.ParseArguments("{ \"serverURL\" : \"https://www.mparticle.com\" , \"targetRPS\" : 10, \"authKey\" : \"Whatever\", \"userName\" : \"\" }");
            Assert.Null(config);
        }
    }
}
