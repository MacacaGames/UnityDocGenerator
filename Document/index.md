# UnityDocGenerator
A helper to make docfx based document in Unity3D

## Features
- Document generate based on [Docfx](https://github.com/dotnet/docfx) in Unity3D
- Generate document depend on Unity [Assembly definitions](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html) (.asmdef) files.
- Document web preview hosting in Unity3D.
- Tested on Windows Editor, OSX Editor.
## Installtion
Add the following line to Packages/manifest.json:
```json
"com.macacagames.macacaapi": "https://github.com/MacacaGames/UnityDocGenerator.git",
```

UnityDocGenerator use LitJson, so add make sure [LitJson](https://litjson.net/) in your project.

Or using UPM package to install LitJson. (This is a third party package)
```json
"com.ms.litjson":"https://github.com/wlgys8/LitJsonUPM.git",
```

## Prereuqired
### macOS, Linux
Make sure Mono Framework runtime is installed in your computer.
[Download Here](https://www.mono-project.com/download/stable/)
### Windows
Make sure MSBuild >= 1.5 is installed in your computer.
One of the easiest way to install is Install the Visual Studio >= 2013.
[Get Visual Studio](https://visualstudio.microsoft.com/zh-hant/downloads/)

## Getting Started
### 1. Open Editor
Open the editor via Unity's toolbar, MacacaGames > DocGenerator

### 2. Get the full path of your mono install.
macOS, Linux only! Windows user can skip this step.

```bash
which mono
```
If the Mono Framework is install correctlly, you may got the return like this.
```bash
/Library/Frameworks/Mono.framework/Versions/Current/Commands/mono
```
Then paste the value to the Mono Path field on Editor.

<img src="Img/01.png">

### 3. Select your working folder
Type the working folder path or use **Select Folder** select via dialog.

<img src="Img/02.png">

UnityDocGenerator will find out all Unity [Assembly definitions](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html) (.asmdef) files inside the folder (include all sub folder) as the DocFx target.

For example, the screeshot shows there will be 2 files set to DocFx target.

<img src="Img/03.png">

### 4. Generate!!
Click **Generate Document** and wait for the task complete.
The Unity Editor may hangs during the docfx runing, it is normal and usually resume in 1 min (Based on your project size).

You can see the DocFx result in Unity Editor Console.

<img src="Img/04.png">

### 5. Hosting and preview (Optional)
You can preview you generated document via UnityDocGenerator.
Click **Start Hosting** to enable hosting, navigate to http://127.0.0.1:18080/index to view your generated document.

Hosgint power by [sableangle/UnityHTTPServer](https://github.com/sableangle/UnityHTTPServer)

<img src="Img/05.png">

### 6. Next Step?
Upload your document to your web server.

The Project is already optimized for [Github Page](https://pages.github.com/), so you can just push and enable it on your repository.

## Project Struction
UnityDocGenerator will generate two folders in your working folder.
Only modify them if you understand what you're doing, eg.. more custom modify for docfx.

UnityDocGenerator use Document/docfx.json as the DocFx setting to generate document.

```
 - root (The working folder you select)
  - your sub folder etc..
  - your files etc..
  - Document (The DocFx project generate by UnityDocGenerator)
  - docs (The generated document)
```

## Troubleshooting
- Try to find the error message in Unity Editor Console.
- Make sure you Mono Framework is install correctlly.
- Open an issue on Github if there is any bug, and provide the project which reproduce the issue (if possiable).
- Linux Editor is supported in theory but not tested.


## TODO
- Migrate UI into PackageManagerUI ?
- More custom setting for docfx.


