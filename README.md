# FindByGuid

## 🔍 What is FindByGuid?

**FindByGuid** is a BIM Vision plugin that allows you to search for specific elements within an open IFC model using their Global ID (GUID).

### ✨ What It Does

- Prompts the user to input a Global ID.
- Scans all elements in the currently open IFC file.
- Selects the elements that match the given Global ID.
- Automatically focuses the view on the selected elements for quick inspection.

This is especially useful for quickly locating and inspecting elements based on their unique identifiers in coordination or debugging workflows.

---

## 🛠️ Building the Project

To build this project, you'll need the following dependencies:

- [.NET Framework 4.5.2 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net452)  
- [Microsoft Build Tools 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48159)
- [.NET Framework 3.5 runtime](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net35-sp1)
  
If the project fails to build after installing the required tools, try the following:

1. Remove the `UnmanagedExports` and `UnmanagedExports.Repack` packages via NuGet.
2. Reinstall them to ensure proper configuration.

![Build Screenshot](https://github.com/user-attachments/assets/13869e4b-a665-49da-b394-3e310299b653)

---

## 📦 Using the Plugin

To use this plugin with BIM Vision:

1. Download the compiled `.dll` files from the [Releases](https://github.com/matheushsaba/BIMVision-FindByGuid/releases) section.
2. Paste the files into the appropriate BIM Vision plugins directory.

**Plugin installation paths:**

- **32-bit version:**  
  `C:\Program Files (x86)\Datacomp\BIM Vision\plugins`

- **64-bit version:**  
  `C:\Program Files (x86)\Datacomp\BIM Vision\plugins_x64`

![image](https://github.com/user-attachments/assets/c7d2735a-7cf1-49d0-b099-f78a1e058620)
![image](https://github.com/user-attachments/assets/3a07f941-db23-4d3e-a93d-046bdb1c3673)

---

## 🧩 Troubleshooting

### 🔹 Plugin not appearing in the BIM Vision ribbon

If the plugin does not show up in the BIM Vision interface after installation, make sure you're using **BIM Vision version 3.0.1**. Compatibility with other versions is not guaranteed and may cause the plugin to be ignored or fail to load.

![image](https://github.com/user-attachments/assets/ca2c58e3-f1fc-4eb5-8c22-e8050c7d3a27)

