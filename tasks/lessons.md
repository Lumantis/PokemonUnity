# Lessons apprises — PokemonUnity

## Règles de codage Unity 6 (apprises par correction)

### 1. [RequireComponent] — valide seulement sur Component
- **Erreur** : `[RequireComponent(typeof(AudioClip))]` — AudioClip est un Asset, pas un Component
- **Erreur** : `[RequireComponent(typeof(UnityEngine.Rect))]` — Rect est une struct, pas un Component
- **Règle** : Utiliser `[RequireComponent]` uniquement avec des types qui héritent de `Component`
- **Correction** : Supprimer l'attribut ou remplacer `Rect` par `RectTransform`

### 2. Coroutines — toujours appeler via StartCoroutine()
- **Erreur** : Appeler `update()` directement quand la méthode retourne `IEnumerator`
- **Règle** : En C#, appeler une méthode iterator sans l'itérer ne fait rien
- **Correction** : `StartCoroutine(update())`

### 3. Regex — Groups vs Captures
- **Erreur** : `m.Captures[1].Value` pour extraire le groupe 1
- **Règle** : `Captures[0]` = match complet (collection avec un seul élément généralement) ; `Groups[1]` = premier groupe de capture
- **Correction** : `m.Groups[1].Value`

### 4. LeanTween alphaCanvas — plage 0.0–1.0 pas 0–255
- **Erreur** : `LeanTween.alphaCanvas(cg, 255, duration)` → canvas invisible
- **Correction** : `LeanTween.alphaCanvas(cg, 1f, duration)`

### 5. Lazy initialization — null check sens correct
- **Erreur** : `if (field != null) field = new Thing()` — écrase quand il existe, ne crée jamais
- **Règle** : Lazy init = créer SEULEMENT quand null
- **Correction** : `if (field == null) field = new Thing()`

### 6. Namespace guards — using UnityEngine hors #if
- **Erreur** : `#if (DEBUG == false || UNITY_EDITOR == true) using UnityEngine; using UnityEditor;`
- **Règle** : `using UnityEngine` nécessaire dans tous les builds (runtime)
- **Correction** : `using UnityEngine;` en haut, `#if UNITY_EDITOR using UnityEditor; #endif` séparé

### 7. #if UNITY_EDITOR — pas de syntaxe booléenne
- **Erreur** : `#if (UNITY_EDITOR == true)` — syntaxe invalide
- **Règle** : Les directives de préprocesseur Unity n'acceptent pas `==`, utiliser `#if UNITY_EDITOR`

### 8. AND bit à bit — Object & bool
- **Erreur** : `s.clip & s.isPlaying` — AND bit à bit entre Object et bool
- **Correction** : `s.clip != null && s.isPlaying`

### 9. [SerializeField] sur les champs protected Unity
- **Règle** : Un champ `protected` sans `[SerializeField]` n'apparaît pas dans l'inspecteur
- **Correction** : Ajouter `[SerializeField]` sur tous les champs qui doivent être assignés depuis l'inspecteur

### 10. StringBuilder en boucle sur TMP
- **Règle** : `text.text += char` dans une boucle = O(n²) allocations
- **Correction** : Utiliser `StringBuilder` + `text.SetText(sb)`

### 11. Cast explicite — this vs champ
- **Erreur** : `((IInterface)field).property = value` quand `field` n'implémente pas l'interface
- **Correction** : `((IInterface)this).property = value` si c'est la classe courante qui implémente l'interface

### 12. P/Invoke Windows — pas cross-platform
- **Règle** : `[DllImport("user32")]` ne compile pas sur Android/iOS/WebGL/Mac/Linux
- **Correction** : Utiliser des alternatives Unity natives ou stub non-fonctionnel pour les builds non-Windows

### 13. Bounds check — direction du comparateur
- **Erreur** : `if (array.Length <= i && array[i] != null)` — accède array[i] quand i >= Length
- **Correction** : `if (array.Length > i && array[i] != null)`

## Règles de workflow

### W1. Lire avant d'éditer
- Toujours lire le fichier entier avant toute modification
- Vérifier l'indentation exacte (tabs vs espaces) avec Grep avant Edit

### W2. Vérifier chaque correctif avec git diff
- Après chaque Edit, vérifier le diff pour confirmer que les bonnes lignes ont changé

### W3. Un commit par vague logique
- Ne pas mélanger les correctifs de bugs et les nouvelles fonctionnalités dans le même commit
