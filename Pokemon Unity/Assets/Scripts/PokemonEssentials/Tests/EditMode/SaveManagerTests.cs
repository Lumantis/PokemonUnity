using NUnit.Framework;
using PokemonUnity.Interface.UnityEngine;

namespace PokemonUnity.Interface.UnityEngine.Tests
{
    /// <summary>EditMode NUnit tests for <see cref="SaveData"/>.</summary>
    public class SaveDataTests
    {
        [Test]
        public void SaveData_DefaultVersion_IsCurrentVersion()
        {
            var data = new SaveData();
            Assert.AreEqual(SaveData.CURRENT_VERSION, data.version);
        }

        [Test]
        public void SaveData_Timestamp_IsSet()
        {
            var data = new SaveData();
            Assert.IsFalse(string.IsNullOrEmpty(data.timestamp));
        }

        [Test]
        public void GetDisplayHeader_ContainsPlayerName()
        {
            var data = new SaveData { playerName = "Ash", badgeCount = 3 };
            string header = data.GetDisplayHeader();
            StringAssert.Contains("Ash", header);
            StringAssert.Contains("3", header);
        }

        [Test]
        public void SaveData_Serialization_RoundTrip()
        {
            var original = new SaveData
            {
                playerName    = "Misty",
                badgeCount    = 8,
                playtimeSeconds = 3600,
                money         = 9999
            };

            string json   = Newtonsoft.Json.JsonConvert.SerializeObject(original);
            var    loaded = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveData>(json);

            Assert.AreEqual(original.playerName,    loaded.playerName);
            Assert.AreEqual(original.badgeCount,    loaded.badgeCount);
            Assert.AreEqual(original.playtimeSeconds, loaded.playtimeSeconds);
            Assert.AreEqual(original.money,         loaded.money);
        }
    }
}
