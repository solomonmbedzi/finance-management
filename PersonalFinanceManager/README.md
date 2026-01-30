# Personal Finance Manager

Simple console-based personal finance manager.

## Run locally

Open PowerShell and run:

```powershell
$env:PATH="$env:LOCALAPPDATA\dotnet;" + $env:PATH
cd "c:\Users\Hp\Documents\PersonalFinanceManager"
dotnet run
```

## Build

```powershell
dotnet build
```

## Debug in VS Code

1. Open folder `c:\Users\Hp\Documents\PersonalFinanceManager` in VS Code.
2. Press F5 (Start Debugging). Launch configuration uses `integratedTerminal`.

## Files changed

- `Program.cs` — main application (null-safe input handling added)
- `.vscode/launch.json` — VS Code debug configuration
- `.vscode/tasks.json` — build task
- `README.md` — this file

## Notes

- Data is stored in CSV files in the project folder: `users.csv`, `transactions.csv`, `goals.csv`.
- The app is a console app; use the integrated terminal when debugging to interact with prompts.
