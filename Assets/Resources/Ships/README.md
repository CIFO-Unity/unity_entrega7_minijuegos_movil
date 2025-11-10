# 🚀 Sistema de Sprites Dinámicos por Jugador

## 📁 Estructura de Carpetas

Coloca los sprites de las naves en:
```
Assets/Resources/Ships/
├── Lorena.png          (Nave normal de Lorena)
├── Lorena_Ghost.png    (Nave fantasma de Lorena)
├── Albert.png          (Nave normal de Albert)
├── Albert_Ghost.png    (Nave fantasma de Albert)
├── Felix.png           (Nave normal de Felix)
├── Felix_Ghost.png     (Nave fantasma de Felix)
└── ...
```

⚠️ **IMPORTANTE**: Los sprites **DEBEN** estar dentro de una carpeta llamada `Resources` para que Unity pueda cargarlos dinámicamente.

## 🎮 Cómo Usar

### 1. Para la Nave del Jugador Local:

1. Añade el componente `PlayerShipSpriteLoader` al GameObject de la nave del jugador
2. Deja `Is Ghost Ship` **desactivado** (false)
3. Arrastra un sprite por defecto al campo `Default Sprite` (opcional)
4. El sprite se cargará automáticamente según el nombre guardado en `PlayerPrefs`

### 2. Para la Nave Fantasma:

1. Añade el componente `PlayerShipSpriteLoader` al prefab de la nave fantasma
2. **Activa** `Is Ghost Ship` (true)
3. Arrastra un sprite por defecto al campo `Default Sprite` (opcional)
4. Buscará sprites con sufijo `_Ghost` (ej: `Lorena_Ghost.png`)

## 🖼️ Convención de Nombres

- **Nave normal**: `[NombreJugador].png`
  - Ejemplo: `Lorena.png`, `Albert.png`, `Felix.png`

- **Nave fantasma**: `[NombreJugador]_Ghost.png`
  - Ejemplo: `Lorena_Ghost.png`, `Albert_Ghost.png`

## 🔧 Configuración Avanzada

### Cambiar la carpeta de sprites:
En el Inspector, modifica el campo `Sprite Folder Path` (por defecto: "Ships")

### Cambiar sprite manualmente desde código:
```csharp
GetComponent<PlayerShipSpriteLoader>().ChangeSprite("Lorena");
```

## ✅ Ventajas

- ✅ Carga automática según nombre del jugador
- ✅ Sistema flexible y reutilizable
- ✅ Fácil de añadir nuevos jugadores (solo añadir sprite a la carpeta)
- ✅ Sprite por defecto si no se encuentra el sprite específico
- ✅ Mismo sistema para nave normal y nave fantasma

## 🐛 Troubleshooting

**No carga el sprite:**
- Verifica que el sprite esté en `Assets/Resources/Ships/`
- Verifica que el nombre coincida exactamente con el `PlayerName` en PlayerPrefs
- Mira la consola para ver mensajes de debug

**Sprite por defecto no aparece:**
- Asegúrate de haber arrastrado un sprite al campo `Default Sprite` en el Inspector
