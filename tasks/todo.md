# PokemonUnity — Plan d'implémentation Sections 1–4

## État : EN COURS

---

## Wave 1 — Fondations (indépendants, parallélisables)

- [ ] `ObjectPool.cs` — pool générique C# + AudioSourcePool
- [ ] `IServiceLocator.cs` + `ServiceLocator.cs` — DI container
- [ ] `PokemonDataSO.cs` + `MoveDataSO.cs` + `ItemDataSO.cs` — ScriptableObjects
- [ ] `SaveData.cs` + `SaveManager.cs` — sauvegarde versionnée
- [ ] `LocalizationManager.cs` + `ResourceLoader.cs` — i18n + ressources
- [ ] `NewInputController.cs` — migration Input System

## Wave 2 — Systèmes de combat

- [ ] `IBattleState.cs` + `BattleContext.cs` + `BattleStateMachine.cs`
- [ ] États: `BattleStartState`, `ChooseCommandState`, `ExecuteMoveState`, `FaintState`, `BattleEndState`
- [ ] `MoveScorer.cs` + `TrainerAI.cs` — IA dresseurs

## Wave 3 — UX & Audio

- [ ] `AdaptiveMusicManager.cs` — musique adaptative
- [ ] `VirtualGamepad.cs` — contrôles mobile
- [ ] `MoveVFXData.cs` + `MoveVFXLibrary.cs` + `BattleVFXManager.cs` — VFX

## Wave 4 — Contenu long terme

- [ ] `AchievementDefinition.cs` + `AchievementCollection.cs` + `AchievementManager.cs`
- [ ] `ProceduralPokemonGenerator.cs`
- [ ] `RuntimeMapEditor.cs`
- [ ] `IModPlugin.cs` + `ModLoader.cs`

## Wave 5 — Tests & CI/CD

- [ ] `PokemonEssentials.Tests.asmdef`
- [ ] EditMode tests (Save, ServiceLocator, ObjectPool, Achievements)
- [ ] PlayMode tests (Input)
- [ ] `.github/workflows/unity-build.yml`

## Final

- [ ] Commit toutes les vagues
- [ ] Push sur `claude/unity-6.3-migration-XtOmH`
- [ ] Mise à jour `EvolutionPlan.html` avec statut "Implémenté"
