# 🔥 Semester Project 2: **Danfoss** Heating Optimization  

This is the **Group 3** repository for Semester Project 2. Below is the structure and file distribution for our **Danfoss** heating project.  

---

## 📁 **Project Structure**  
```plaintext
SEMESTER-PROJECT-2/
├── 🏗️ bin/                           # Compiled binaries and executable files
├── 📂 Data/                          # Contains system-related datasets
│   ├── 📄 heat_demand.csv            # Heating demand data
│   ├── 📄 production_units.json      # Information about production units
├── ⚙️ obj/                           # Build artifacts and temporary files
├── 📦 Source/                        # Core source files
│   ├── 🏢 AssetManager/              # Asset Manager module
│   │   ├── 📜 AssetManager.cs        # Manages static system data
│   │   ├── 🏭 ProductionUnit.cs      # Defines production units (Boilers, Motor, and Pump)
│   ├── 🔍 SourceDataManager/         # Source Data Manager module
│   │   ├── 🔥 HeatDemand.cs          # Stores seasonal heat demand records
│   │   ├── 📊 SourceDataManager.cs   # Manages dynamic system data
│   ├── 🚀 Program.cs                 # Main application logic
├── 📄 .gitignore                     # Git ignore file
├── 📘 README.md                      # Project documentation
├── 📝 Semester-project-2.sln         # Solution file
└── 🔧 Semester-project-2.csproj      # Project file
```

---

## 📜 **Key Components**  

### **📊 Data Files**  
- 📄 **`heat_demand.csv`** – Stores heating demand data for analysis.  
- 📄 **`production_units.json`** – Contains details about production units, such as energy production, consumption, and costs.  

### 🔧 Source Files  
- 📜 **`AssetManager.cs`** – Manages and loads static system data, including production units and heat demand from JSON and CSV files.  
- 🏭 **`ProductionUnit.cs`** – Defines the `ProductionUnit` class, which stores data about energy production units, including max heat, electricity, costs, and emissions.  
- 🔥 **`HeatDemand.cs`** – Defines the `HeatDemand` class, which stores winter and summer heat demand records.
- 📊 **`SourceDataManager.cs`** – Handles dynamic data processing and ensures efficient management of system-related information.  
- 🚀 **`Program.cs`** – Main application logic and execution flow.

---

## 👥 **Contribution Guidelines**  
💡 When contributing, make sure your comments are clear, and your changes are isolated to your assigned area. This helps avoid conflicts and ensures smooth integration of everyone’s work.  

---
