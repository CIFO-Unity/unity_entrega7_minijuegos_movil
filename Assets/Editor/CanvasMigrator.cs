using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public static class CanvasMigrator
{
    [MenuItem("Tools/Canvas Migrator/Migrate Selected Canvas...")]
    public static void MigrateSelectedCanvasMenu()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("Canvas Migrator","Selecciona un GameObject Canvas en la jerarquía antes de ejecutar.", "OK");
            return;
        }

        var go = Selection.activeGameObject;
        var canvas = go.GetComponent<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Canvas Migrator","El objeto seleccionado no tiene componente Canvas.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Confirmar migración", $"Crear un nuevo Canvas y mover hijos de '{go.name}' a '{go.name}_migrated'?", "Sí", "No"))
            return;

        DoMigrate(canvas.gameObject, disableOldCanvas: true);
    }

    [MenuItem("Tools/Canvas Migrator/Migrate Canvas named 'Canvas' in Scene")]
    public static void MigrateSceneRootCanvas()
    {
        var root = GameObject.Find("Canvas");
        if (root == null)
        {
            EditorUtility.DisplayDialog("Canvas Migrator","No se encontró un GameObject llamado 'Canvas' en la escena.", "OK");
            return;
        }

        var canvas = root.GetComponent<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Canvas Migrator","El GameObject 'Canvas' no tiene componente Canvas.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Confirmar migración", $"Crear un nuevo Canvas y mover hijos de 'Canvas' a 'Canvas_migrated'?", "Sí", "No"))
            return;

        DoMigrate(canvas.gameObject, disableOldCanvas: true);
    }

    static void DoMigrate(GameObject oldCanvasGO, bool disableOldCanvas)
    {
        // Registro para undo
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        // Crear nuevo GameObject Canvas
        string newName = oldCanvasGO.name + "_migrated";
        GameObject newCanvas = new GameObject(newName);
        Undo.RegisterCreatedObjectUndo(newCanvas, "Create migrated Canvas");

        // Añadir componentes básicos
        var newRect = newCanvas.AddComponent<RectTransform>();
        var newCanvasComp = newCanvas.AddComponent<Canvas>();
        var newScaler = newCanvas.AddComponent<CanvasScaler>();
        newCanvas.AddComponent<GraphicRaycaster>();

        // Colocar en la misma posición de jerarquía que el antiguo Canvas
        newCanvas.transform.SetParent(oldCanvasGO.transform.parent, false);

        // Copiar RectTransform settings del root antiguo
        var oldRect = oldCanvasGO.GetComponent<RectTransform>();
        if (oldRect != null)
        {
            newRect.anchorMin = oldRect.anchorMin;
            newRect.anchorMax = oldRect.anchorMax;
            newRect.anchoredPosition = oldRect.anchoredPosition;
            newRect.sizeDelta = oldRect.sizeDelta;
            newRect.pivot = oldRect.pivot;
            newRect.localRotation = oldRect.localRotation;
            newRect.localScale = Vector3.one; // aseguramos escala limpia
        }

        // Copiar propiedades del Canvas
        var oldCanvasComp = oldCanvasGO.GetComponent<Canvas>();
        if (oldCanvasComp != null)
        {
            newCanvasComp.renderMode = oldCanvasComp.renderMode;
            newCanvasComp.pixelPerfect = oldCanvasComp.pixelPerfect;
            newCanvasComp.overrideSorting = oldCanvasComp.overrideSorting;
            newCanvasComp.sortingLayerID = oldCanvasComp.sortingLayerID;
            newCanvasComp.sortingOrder = oldCanvasComp.sortingOrder;
            newCanvasComp.additionalShaderChannels = oldCanvasComp.additionalShaderChannels;
            newCanvasComp.targetDisplay = oldCanvasComp.targetDisplay;
            newCanvasComp.gameObject.layer = oldCanvasComp.gameObject.layer;

            // Copiar camera si existe
            newCanvasComp.worldCamera = oldCanvasComp.worldCamera;
        }

        // Copiar CanvasScaler si existe
        var oldScaler = oldCanvasGO.GetComponent<CanvasScaler>();
        if (oldScaler != null)
        {
            newScaler.uiScaleMode = oldScaler.uiScaleMode;
            newScaler.referencePixelsPerUnit = oldScaler.referencePixelsPerUnit;
            newScaler.scaleFactor = oldScaler.scaleFactor;
            newScaler.referenceResolution = oldScaler.referenceResolution;
            newScaler.screenMatchMode = oldScaler.screenMatchMode;
            newScaler.matchWidthOrHeight = oldScaler.matchWidthOrHeight;
            newScaler.physicalUnit = oldScaler.physicalUnit;
            newScaler.fallbackScreenDPI = oldScaler.fallbackScreenDPI;
            newScaler.defaultSpriteDPI = oldScaler.defaultSpriteDPI;
            newScaler.dynamicPixelsPerUnit = oldScaler.dynamicPixelsPerUnit;
        }

        // Mover hijos: copiamos la lista primero para evitar modificación durante iteración
        var children = new List<Transform>();
        foreach (Transform child in oldCanvasGO.transform)
            children.Add(child);

        foreach (var child in children)
        {
            // Reparent sin mantener mundo (mantener locales) para preservar posiciones relativas al nuevo root
            Undo.SetTransformParent(child, newCanvas.transform, "Reparent UI to migrated Canvas");
            // Normalizar escala local del hijo para evitar heredar escalas extrañas
            child.localScale = Vector3.one;
        }

        // Opcional: desactivar canvas antiguo o dejarlo para revisión
        if (disableOldCanvas)
        {
            // Desactivar componente Canvas (mantener GameObject visibile para revisión)
            var comp = oldCanvasGO.GetComponent<Canvas>();
            if (comp != null)
                comp.enabled = false;
            else
                oldCanvasGO.SetActive(false);

            Undo.RegisterCompleteObjectUndo(oldCanvasGO, "Disable old Canvas");
        }

        // Seleccionar nuevo canvas en editor
        Selection.activeGameObject = newCanvas;

        Undo.CollapseUndoOperations(group);

        EditorUtility.DisplayDialog("Canvas Migrator", $"Migración completada. Nuevo Canvas: '{newName}'. El antiguo {(disableOldCanvas?"fue desactivado":"se dejó activo")}.", "OK");
    }
}
#endif
