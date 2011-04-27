using System.Collections.Generic;
using Bottles.Configuration;
using Bottles.Deployment;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Writing;
using Bottles.Tests.Deployment.Writing;
using FubuCore;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;

namespace Bottles.Tests.Deployment.Parsing
{
    [TestFixture]
    public class ProfileReaderIntegratedTester
    {
        private IEnumerable<HostManifest> theHosts;

        [SetUp]
        public void SetUp()
        {
            var writer = new ProfileWriter("clonewars");

            var recipeDefinition = writer.RecipeFor("r1");
            var host = recipeDefinition.HostFor("h1");

            host.AddDirective(new SimpleSettings
            {
                One = "one",
                Two = "two"
            });

            host.AddDirective(new OneSettings()
            {
                Name = "Jeremy",
                Age = 37
            });

            host.AddReference(new BottleReference()
            {
                Name = "bottle1"
            });

            host.AddReference(new BottleReference()
            {
                Name = "bottle2",
                Relationship = "binaries"
            });

            recipeDefinition.HostFor("h2").AddProperty<ThreeSettings>(x => x.Direction, "North");
            recipeDefinition.HostFor("h3").AddProperty<TwoSettings>(x => x.City, "Austin");


            writer.RecipeFor("r2").HostFor("h3").AddProperty<SimpleSettings>(x => x.One, "one");
            writer.RecipeFor("r3").HostFor("h3").AddProperty<SimpleSettings>(x => x.Two, "two");
            writer.RecipeFor("r4").HostFor("h4").AddProperty<SimpleSettings>(x => x.Two, "ten");
            writer.RecipeFor("r4").HostFor("h5").AddProperty<SimpleSettings>(x => x.Two, "ten");
            writer.RecipeFor("r4").HostFor("h5").AddProperty<SimpleSettings>(x => x.One, "*{dbName}*");

            writer.AddEnvironmentSetting<SimpleSettings>(x => x.Two, "h4", "env-value");
            writer.AddEnvironmentSetting("dbName", "blue");

            writer.Flush(FlushOptions.Wipeout);

            var reader = new ProfileReader(new RecipeSorter(), new DeploymentSettings(){
                RecipesDirectory = "clonewars\\recipes",
                EnvironmentFile = "clonewars\\" + ProfileFiles.EnvironmentSettingsFileName
            }, new FileSystem());

            theHosts = reader.Read();
        }

        [Test]
        public void environment_properties_are_substituted_within_host_settings()
        {
            theHosts.First(x => x.Name == "h5").GetDirective<SimpleSettings>()
                .One.ShouldEqual("*blue*");
        }

        [Test]
        public void got_all_the_unique_hosts()
        {
            theHosts.Select(x => x.Name).ShouldHaveTheSameElementsAs("h1", "h2", "h3", "h4", "h5");
        }

        [Test]
        public void environment_settings_are_applied_to_a_host()
        {
            theHosts.First(x => x.Name == "h4").GetDirective<SimpleSettings>().Two.ShouldEqual("env-value");
        }
    }

    [TestFixture]
    public class RecipeReaderIntegratedTester
    {
        private IEnumerable<Recipe> theRecipes;

        [SetUp]
        public void SetUp()
        {
            var writer = new ProfileWriter("starwars");

            var recipeDefinition = writer.RecipeFor("r1");
            var host = recipeDefinition.HostFor("h1");

            host.AddDirective(new SimpleSettings
            {
                One = "one",
                Two = "two"
            });

            host.AddDirective(new OneSettings()
            {
                Name = "Jeremy",
                Age = 37
            });

            host.AddReference(new BottleReference()
            {
                Name = "bottle1"
            });

            host.AddReference(new BottleReference()
            {
                Name = "bottle2",
                Relationship = "binaries"
            });

            recipeDefinition.HostFor("h2").AddProperty<ThreeSettings>(x => x.Direction, "North");
            recipeDefinition.HostFor("h3").AddProperty<TwoSettings>(x => x.City, "Austin");


            writer.RecipeFor("r2").HostFor("h3").AddProperty<SimpleSettings>(x => x.One, "one");
            writer.RecipeFor("r3").HostFor("h3").AddProperty<SimpleSettings>(x => x.Two, "two");
            writer.RecipeFor("r4").HostFor("h4").AddProperty<SimpleSettings>(x => x.Two, "ten");
            writer.RecipeFor("r4").HostFor("h5").AddProperty<SimpleSettings>(x => x.Two, "ten");
            writer.RecipeFor("r4").HostFor("h5").AddProperty<SimpleSettings>(x => x.One, "setting is {setting}");


            writer.Flush(FlushOptions.Wipeout);

            var environmentSettings = new EnvironmentSettings();
            environmentSettings.Overrides["setting"] = "environment setting";

            theRecipes = RecipeReader.ReadRecipes("starwars\\recipes", environmentSettings);
        }

        [Test]
        public void environment_substitutions_happen_in_the_recipe_reader()
        {
            theRecipes.First(x => x.Name == "r4").HostFor("h5")
                .GetDirective<SimpleSettings>().One.ShouldEqual(
                "setting is environment setting");
        }

        [Test]
        public void reads_all_the_recipes()
        {
            theRecipes.Select(x => x.Name).ShouldHaveTheSameElementsAs("r1", "r2", "r3", "r4");
        }

        [Test]
        public void each_recipe_has_all_the_configured_hosts()
        {
            theRecipes.First(x => x.Name == "r1").Hosts.Select(x => x.Name)
                .ShouldHaveTheSameElementsAs("h1", "h2", "h3");
        
        
            theRecipes.First(x => x.Name == "r2").Hosts.Select(x => x.Name).ShouldHaveTheSameElementsAs("h3");
            theRecipes.First(x => x.Name == "r3").Hosts.Select(x => x.Name).ShouldHaveTheSameElementsAs("h3");
            theRecipes.First(x => x.Name == "r4").Hosts.Select(x => x.Name).ShouldHaveTheSameElementsAs("h4", "h5");
        }


    }
}