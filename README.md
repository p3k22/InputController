# P3k Input Controller

Config-driven input layer built on Unity's Input System, with runtime rebinding, per-input options (analog keyboard/mouse, analog gamepad/stick, invert Y), profile save/load, and an optional UI Toolkit bindings window.

This package creates its own `InputActionAsset` at runtime from an `InputConfig` ScriptableObject, exposes each input as an `IInputState`, and supports saving/loading profiles as JSON under `Application.persistentDataPath/InputProfiles`.

## Features

- **Config-driven inputs** – `InputConfig` → list of `InputDefinition` entries
- **Two binding types** – `Button` and `Composite2D` (digital WASD-style and optional analog mouse/stick modes)
- **Runtime values + events** per input:
  - `Value1D`, `Value2D`, `Value3D`
  - `IsPressed`, `IsHeld`, `IsReleased`, `IsRepeated`
  - `OnPressed`, `OnReleased`, `OnHeld`, `OnRepeated`
- **Direct access to underlying InputActions** for subscriptions / custom integrations
- **Interactive rebinding** with cleanup when the override matches the default path
- **Generated InputActionAsset** for use with `InputActionReference` fields (3rd party package support)
- **Profiles** – Save/load binding overrides + option flags (analog keyboard, analog gamepad, inverted Y), stored as JSON
- **UI Toolkit bindings window** – Toggle show/hide (default key `F2`), device toggle, profile field + dropdown, save/reset, optional runtime stats overlay
- **Editor tooling** – Custom inspector for `InputConfig` with dropdowns and a "Bind" listening mode, auto-sync `InputActionAsset` generation

## Requirements

- Unity **Input System** package (`com.unity.inputsystem`)
- Unity **UI Toolkit** (`com.unity.ui`)
- Unity 2022.3+ (tested with 6000.2+)

## Quick Start

### 1. Create an InputConfig

Create via **Create → P3k → Input Config**.

Each `InputDefinition` includes:

- `Id`
- `Type` – `Button` or `Composite2D`
- `Keyboard` bindings
- `Gamepad` bindings
- `AllowAnalogKeyboard` – Composite2D → allow mouse delta mode
- `AllowAnalogGamepad` – Composite2D → allow stick mode

### 2. Generate the InputActionAsset

In the `InputConfig` inspector, use the **InputActionAsset Sync** section:

- **Auto-Sync on Change** – Automatically regenerates the `.inputactions` file when you modify the config
- **Sync InputActionAsset** – Manually regenerate the asset
- **Select Asset** – Navigate to the generated asset in the Project window

The generated asset is saved alongside your `InputConfig` (e.g., `MyConfig_Actions.inputactions`) and can be used to assign `InputActionReference` fields in third-party packages.

### 3. Add the InputController

Add `P3k.InputController.Adapters.Components.InputController` to a GameObject and assign your `InputConfig`. On `Awake`, it builds an `InputActionAsset` (action map name: `Player`), loads the current profile (default `default`), then enables all states.

### 4. Use Inputs at Runtime

```csharp
using UnityEngine;
using P3k.InputController.Abstractions.Interfaces.Core;

public sealed class ExampleConsumer : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _controllerBehaviour;

    private IInputController Controller => _controllerBehaviour as IInputController;

    private void Update()
    {
        var move = Controller.Get("Move");
        var jump = Controller.Get("Jump");

        var moveVec = move.Value2D;

        if (jump.IsPressed)
        {
            // Perform jump
        }
    }
}
```

### 5. Using InputActionReference (3rd Party Packages)

For packages that require an `InputActionReference` (e.g., a developer console toggle):

1. Generate the `InputActionAsset` from your `InputConfig` (see step 2)
2. Drag the desired action from the generated asset into the package's `InputActionReference` field
3. At runtime, the `InputController` automatically syncs rebind overrides to the generated asset

This ensures that when a user rebinds an input, the `InputActionReference` responds to the new binding.

```csharp
// Example: A 3rd party console package
public class ConsolePackage : MonoBehaviour
{
    public InputActionReference toggleConsoleAction; // Assign from generated asset

    private void OnEnable()
    {
        toggleConsoleAction.action.performed += OnToggleConsole;
    }

    private void OnToggleConsole(InputAction.CallbackContext ctx)
    {
        // This will fire even after the user rebinds the key
    }
}
```

### 6. Add the Bindings UI Window (Optional)

Attach `InputControllerUI` to a GameObject with a `UIDocument`.

- Assign the `InputController` to `_inputController`
- Provide the UI document root element named `root`

Create the default UXML via **Assets/Create/P3k/InputControllerUIDoc**.

## Architecture

The system uses two `InputActionAsset` instances:

| Asset | Purpose |
|-------|---------|
| **Runtime Asset** | Created in memory at startup. Used by `InputController.Get()` for all gameplay input. Receives rebind overrides. |
| **Generated Asset** | Persisted on disk (`.inputactions` file). Used by `InputActionReference` fields. Receives synced overrides from the runtime asset. |

Both assets use **identical binding GUIDs** (generated via stable MD5 hash), allowing rebind overrides to be copied between them.

```
┌─────────────────────────────────────────────────────────────┐
│                         EDITOR                              │
│  InputConfig ──► InputActionAssetGenerator ──► .inputactions│
│                         (same GUIDs)                        │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                         RUNTIME                             │
│  InputController.Awake()                                    │
│       ├── CreateActionAsset() → Runtime Asset (same GUIDs)  │
│       ├── ProfileLoad() → applies overrides                 │
│       ├── SyncOverridesToGeneratedAsset()                   │
│       └── EnableGeneratedAsset()                            │
│                                                             │
│  Rebind() → override on runtime asset → sync to generated   │
│                                                             │
│  InputActionReference → Generated Asset → responds to rebind│
└─────────────────────────────────────────────────────────────┘
```

## Profiles

Saved to: `Application.persistentDataPath/InputProfiles/<profileName>.json`

Includes:

- Binding overrides JSON
- Lists for analog keyboard, analog gamepad, and inverted Y inputs

API:

```csharp
ProfileLoad(profileName);
ProfileSave(profileName);
ProfileExists(profileName);
ProfileCanBeSaved(profileName);
ResetBindingsToDefault();
```

## Rebinding

```csharp
Rebind(id, device, bindingName, onComplete, onCancel);
RebindCancel();
```

Composite2D inputs correctly restrict analog/digital overlap during rebinding.

After any rebind completes, the override is automatically synced to the generated `InputActionAsset`, ensuring `InputActionReference` fields respond to the new binding.