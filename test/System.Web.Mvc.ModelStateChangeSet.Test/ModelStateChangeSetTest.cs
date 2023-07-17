using NUnit.Framework;
using System.Globalization;
using System.Linq;

namespace System.Web.Mvc.ModelStateChangeSet.Test
{
    public class ModelStateChangeSetTest
    {
        [Test]
        public void TestGetPropertyAccessors()
        {
            // Arrange
            var entity = new Entity
            {
                Id = 1,
                Account = new Account
                {
                    Address = new Address
                    {
                        City = "Chicago",
                        Country = "USA"
                    }
                }
            };

            // Act
            var accessors = typeof(Entity).GetPropertyAccessors<Entity>();

            // Assert
            Assert.AreEqual(11, accessors.Count);

            Assert.IsTrue(accessors.ContainsKey("Id"));
            Assert.AreEqual(1, accessors["Id"](entity));

            Assert.IsTrue(accessors.ContainsKey("Account.Address.City"));
            Assert.AreEqual("Chicago", accessors["Account.Address.City"](entity));

            Assert.IsTrue(accessors.ContainsKey("Account.Address.Country"));
            Assert.AreEqual("USA", accessors["Account.Address.Country"](entity));
            Assert.Pass();
        }

        [Test]
        public void TestCreatePropertyMapWithNullObject()
        {
            // Arrange
            var entity = new Entity
            {
                Id = 1,
                Account = null
            };

            // Act
            var accessors = typeof(Entity).GetPropertyAccessors<Entity>();

            // Assert
            Assert.AreEqual(11, accessors.Count);

            Assert.IsTrue(accessors.ContainsKey("Id"));
            Assert.AreEqual(1, accessors["Id"](entity));

            Assert.IsTrue(accessors.ContainsKey("Account.Address.City"));
            Assert.IsNull(accessors["Account.Address.City"](entity));

            Assert.IsTrue(accessors.ContainsKey("Account.Address.Country"));
            Assert.IsNull(accessors["Account.Address.Country"](entity));
        }

        [Test]
        public void TestModelStateGetChangeSet()
        {
            // Arrange
            var newModel = new Entity
            {
                Id = 1,
                Account = new Account
                {
                    Address = new Address
                    {
                        Street = "Michigan Ave",
                        City = "Chicago",        
                        Country = "USA"
                    }
                }
            };

            var oldModel = new Entity
            {
                Id = 1,
                Account = new Account
                {
                    Address = new Address
                    {
                        City = "Detroit",                        
                        Country = "USA"
                    }
                }
            };

            var modelState = new ModelStateDictionary
            {
                { "Id", new ModelState { Value = new ValueProviderResult(1, "1", CultureInfo.CurrentCulture) } },
                { "Account.Address.Street", new ModelState { Value = new ValueProviderResult("Michigan Ave", "Michigan Ave", CultureInfo.CurrentCulture) } },
                { "Account.Address.City", new ModelState { Value = new ValueProviderResult("Chicago", "Chicago", CultureInfo.CurrentCulture) } },
                { "Account.Address.Country", new ModelState { Value = new ValueProviderResult("USA", "USA", CultureInfo.CurrentCulture) } }
            };

            // Act
            var changeSet = modelState.GetChangeSet(newModel, oldModel);

            // Assert
            Assert.AreEqual(2, changeSet.Count);
            Assert.AreEqual(null, changeSet.First(change => change.Property == "Account.Address.Street").OldValue);
            Assert.AreEqual("Michigan Ave", changeSet.First(change => change.Property == "Account.Address.Street").NewValue);
            Assert.AreEqual("Detroit", changeSet.First(change => change.Property == "Account.Address.City").OldValue);
            Assert.AreEqual("Chicago", changeSet.First(change => change.Property == "Account.Address.City").NewValue);

            // Act
            var changesToSave = changeSet.GetValuesToSave().Include(oldModel, "Id");

            // Assert
            Assert.AreEqual(3, changesToSave.Count);
            Assert.AreEqual(1, (int)changesToSave["Id"]);
            Assert.AreEqual("Michigan Ave", (string)changesToSave["Account.Address.Street"]);
            Assert.AreEqual("Chicago", (string)changesToSave["Account.Address.City"]);

            // Act
            var addressChangeToSave = changesToSave.Subset("Account.Address.");

            // Assert
            Assert.AreEqual(2, addressChangeToSave.Count);
            Assert.AreEqual("Michigan Ave", (string)addressChangeToSave["Street"]);
            Assert.AreEqual("Chicago", (string)addressChangeToSave["City"]);
        }
    }
}