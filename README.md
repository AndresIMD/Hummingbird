# Hummingbird

A personal glucose tracking app built with .NET MAUI for managing daily blood sugar readings, insulin doses, and meal tracking.

## Features

- **Daily glucose logging** with pre/post meal measurement types
- **Insulin dose calculator** based on configurable correction factors
- **Meal tracking** with carbohydrate counting
- **History view** with grouped readings by date
- **Dashboard** with summary statistics
- **Configurable settings** for glucose targets and ranges

## Tech Stack

- .NET 10
- .NET MAUI (multi-platform: Android, iOS, Windows, macOS)
- SQLite for local data storage

## Project Structure

```
Hummingbird/
├── Hummingbird/              # Shared library (models, services, views, view models)
├── Hummingbird.Droid/        # Android head project
├── Hummingbird.iOS/          # iOS head project
├── Hummingbird.WinUI/        # Windows head project
└── Hummingbird.Mac/          # macOS head project
```

## Building

1. Open `Hummingbird.sln` in Visual Studio 2026+
2. Set the desired platform project as startup (e.g., `Hummingbird.Droid`)
3. Select a device/emulator and press F5

## License

This project is for personal use.
