# Contributing to AGE of Joy

We appreciate your interest in contributing to AGE of Joy! This document will provide guidelines and best practices to help you contribute effectively.

## Table of Contents

- [Preserving the Simulation](#preserving-the-simulation)
- [Submitting Changes](#submitting-changes)
- [Bug Reports and Feature Requests](#bug-reports-and-feature-requests)
- [Code of Conduct](#code-of-conduct)

## Preserving the Simulation

One of the core principles of AGE of Joy is to create a high-quality, immersive nostalgic experience that authentically simulates the arcade of yesterday. To maintain this experience, it's crucial to ensure that no elements exist in the game that wouldn't have been present in a real arcade of that era.

When contributing, please adhere to the following guidelines:

1. Do not introduce menus, GUI elements, or debugging tools that would break the simulation's authenticity.
2. Ensure that any changes or additions maintain the overall atmosphere and aesthetic of a late 20th-century arcade.
3. Test your changes thoroughly to ensure they do not create any visual or functional inconsistencies that might detract from the player's experience.

## Submitting Changes

To submit changes to the AGE of Joy project, follow these steps:

1. Fork the repository.
2. Create a new feature branch in your fork based on the `main` branch, with an appropriate name that reflects the changes you're making (e.g., `feature/adding-npc-hairstyles`, `issues/fix-rendering-issue`).
3. Implement your changes, ensuring they adhere to the guidelines outlined in [Preserving the Simulation](#preserving-the-simulation).
4. Commit your changes to your branch, using clear and concise commit messages.
5. Push your changes to your fork.
6. Create a pull request targeting the `main` branch of the AGE of Joy repository.  Please include how it was tested.

After submitting your pull request, maintainers will review your changes and provide feedback. Please be prepared to make revisions if necessary.

## Bug Reports and Feature Requests

To report a bug or request a new feature, please use the [GitHub Issues](https://github.com/curif/AgeOfJoy-2022.1/issues) page. When reporting a bug, provide as much information as possible, including steps to reproduce the issue and any relevant error messages.

When requesting a new feature, please ensure that it aligns with the goals of preserving the simulation and maintaining authenticity.

For troubleshooting support please see the [discord channel](https://discord.com/channels/1066438667989696645/1070495784816083014).

## How To Build
### Basic usage

To build the project with default settings on macOS or Linux you can use the build.sh script

```bash
./build.sh
```

On Windows you use the build.cmd instead:

```batch
./build.cmd
```

### Customizing the Scenes

You may want to test only a subsection of the project, for instance debugging a single room.  You can include only specific scenes using the `--customScenes` parameter 

```bash
./build.sh -customScenes="Assets/Scenes/Scene1.unity,Assets/Scenes/Scene2.unity"
```

### Overriding Unity path and build output path

To override the default Unity Editor path and build output path, use the UNITY_EDITOR_PATH and UNITY_BUILD_PATH environment variables:

```bash
UNITY_EDITOR_PATH="/path/to/unity_editor" UNITY_BUILD_PATH="/path/to/output" ./build.sh
```

Or for Windows:
```batch
set "UNITY_EDITOR_PATH=C:\path with space\to\unity_editor" && set "UNITY_BUILD_PATH=C:\path with space\to\output" && build.cmd
```

These options can be combined, so you can specify a specific Unity version AND a custom set of scenes