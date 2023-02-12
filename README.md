## UnityEditorBuild

Requirements: `Unity 2021 LTS+`, `.NET Standard 2.1`

![UwU](https://user-images.githubusercontent.com/115415388/218286170-7d1291af-4e16-4e7e-b609-8bf2de2fac72.png)

##### Example:
```csharp
string versionStr =
	string.Concat("Version ", Application.version,
	" (", BuildInfo.BuildNumber, ")\n", BuildInfo.Timestamp.ToString("MMMM M, yyyy"));
```

##### Result:
![OwO](https://user-images.githubusercontent.com/115415388/218285850-3f78dd24-25c0-4277-8c2d-13e8e42a28d3.png)

> The code is based on [JesusLuvsYooh](https://github.com/JesusLuvsYooh/BuildStripper)'s idea
