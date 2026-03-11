using NUnit.Framework;
using PokemonUnity.Interface.UnityEngine;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine.Tests
{
    /// <summary>EditMode NUnit tests for achievement logic (without MonoBehaviour).</summary>
    public class AchievementManagerTests
    {
        // We test AchievementDefinition and AchievementCollection directly,
        // since AchievementManager is a MonoBehaviour (needs PlayMode or EditMode with
        // GameObject setup).

        [Test]
        public void AchievementDefinition_ProgressGoal_DefaultIsOne()
        {
            var def = ScriptableObject.CreateInstance<AchievementDefinition>();
            Assert.AreEqual(1, def.progressGoal);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void AchievementDefinition_IsHidden_DefaultFalse()
        {
            var def = ScriptableObject.CreateInstance<AchievementDefinition>();
            Assert.IsFalse(def.isHidden);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void AchievementCollection_Get_ReturnsNull_WhenEmpty()
        {
            var col = ScriptableObject.CreateInstance<AchievementCollection>();
            Assert.IsNull(col.Get("nonexistent_id"));
            Object.DestroyImmediate(col);
        }

        [Test]
        public void AchievementCategory_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementCategory), "Story"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementCategory), "Battle"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementCategory), "Collection"));
        }
    }
}
